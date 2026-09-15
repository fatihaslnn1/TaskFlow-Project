using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.WebAPI.Hubs;

namespace TaskFlow.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<TaskFlowHub> _hubContext;

        public CommentsController(AppDbContext context, IHubContext<TaskFlowHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentDto dto)
        {
            var issue = await _context.Issues.FindAsync(dto.IssueId);
            if (issue == null) return NotFound("Görev bulunamadı.");

            var currentUserId = GetCurrentUserId();

            var comment = new Comment
            {
                IssueId = dto.IssueId,
                UserId = currentUserId,
                Content = dto.Text, 
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            
            // YENİ EKLENEN KISIM: AUDIT LOG (Yorum Eklendi Logu)
            _context.AuditLogs.Add(new AuditLog
            {
                IssueId = dto.IssueId,
                UserId = currentUserId,
                Action = "CommentAdded",
                Details = "Göreve yeni bir yorum eklendi.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(); // Hem yorumu hem logu tek seferde kaydeder

            // EVENT: CommentAdded
            await _hubContext.Clients.Group($"project:{issue.ProjectId}").SendAsync("CommentAdded", comment);

            var notificationsToSend = new List<Notification>();

            if (issue.AssigneeId.HasValue && issue.AssigneeId.Value != currentUserId)
            {
                var notif = new Notification
                {
                    UserId = issue.AssigneeId.Value,
                    Title = "Yeni Yorum",
                    Message = $"'{issue.Title}' görevine yeni bir yorum yapıldı.",
                    Type = "NewComment",
                    RelatedIssueId = issue.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                notificationsToSend.Add(notif);
            }

            var mentionRegex = new Regex(@"@(\w+)");
            var matches = mentionRegex.Matches(dto.Text);

            foreach (Match match in matches)
            {
                var username = match.Groups[1].Value;
                var mentionedUser = await _context.Users.FirstOrDefaultAsync(u => u.FullName.Contains(username));
                
                if (mentionedUser != null && mentionedUser.Id != currentUserId)
                {
                    if (!notificationsToSend.Any(n => n.UserId == mentionedUser.Id))
                    {
                        var mentionNotif = new Notification
                        {
                            UserId = mentionedUser.Id,
                            Title = "Bir Yorumda Bahsedildiniz",
                            Message = $"'{issue.Title}' görevindeki bir yorumda sizden bahsedildi.",
                            Type = "Mentioned",
                            RelatedIssueId = issue.Id,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        notificationsToSend.Add(mentionNotif);

                        // EVENT: MentionReceived
                        await _hubContext.Clients.User(mentionedUser.Id.ToString()).SendAsync("MentionReceived", mentionNotif);
                    }
                }
            }

            if (notificationsToSend.Any())
            {
                _context.Notifications.AddRange(notificationsToSend);
                await _context.SaveChangesAsync();

                foreach (var notif in notificationsToSend)
                {
                    // EVENT: NotificationReceived
                    await _hubContext.Clients.User(notif.UserId.ToString()).SendAsync("NotificationReceived", notif);
                }
            }

            return Ok(comment);
        }
    }

    public class CreateCommentDto
    {
        public int IssueId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}