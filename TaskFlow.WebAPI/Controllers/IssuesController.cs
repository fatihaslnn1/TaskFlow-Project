using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.WebAPI.Hubs;

namespace TaskFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IssuesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<TaskFlowHub> _hubContext;

        public IssuesController(AppDbContext context, IHubContext<TaskFlowHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        private async Task<bool> HasAccessToProjectAsync(int userId, string role, int projectId)
        {
            if (role == "Admin") return true;
            return await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        // --- YARDIMCI METOTLAR: Rol Matrisi Kuralları ---
        private bool CanManageDueDate(string role)
        {
            return role == "Admin" || role == "PM";
        }

        private bool CanChangePriority(string role)
        {
            // Matrise göre Müşteri ve Reporter öncelik değiştiremez (Hayır). Developer sınırlı, PM ve Admin tam yetkili.
            return role == "Admin" || role == "PM" || role == "Developer";
        }
        // ----------------------------------------------

        [HttpPost]
        public async Task<IActionResult> CreateIssue([FromBody] CreateIssueDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (!await HasAccessToProjectAsync(currentUserId, currentUserRole, dto.ProjectId))
                return Forbid("Bu projeye erişim yetkiniz bulunmamaktadır.");

            // --- YETKİ KONTROLÜ (27. Rol ve Yetki Matrisi: Due Date) ---
            if (dto.DueDate.HasValue && !CanManageDueDate(currentUserRole))
            {
                return Forbid("Due Date (Teslim Tarihi) alanını yalnızca PM (Project Manager) belirleyebilir.");
            }
            // -------------------------------------------------------------

            var project = await _context.Projects.FindAsync(dto.ProjectId);
            if (project == null) return NotFound("Proje bulunamadı.");

            var issueCountInProject = await _context.Issues.CountAsync(i => i.ProjectId == dto.ProjectId);
            var issue = new Issue
            {
                IssueKey = $"{project.ProjectKey}-{issueCountInProject + 1}",
                Title = dto.Title,
                Description = dto.Description,
                IssueType = dto.IssueType,
                Priority = dto.Priority,
                ProjectId = dto.ProjectId,
                ReporterId = currentUserId,
                AssigneeId = dto.AssigneeId,
                DueDate = dto.DueDate,
                EstimatedEffort = dto.EstimatedEffort
            };

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync(); // IssueId oluşması için önce kaydediyoruz

            // AUDIT LOG: Issue oluşturuldu
            _context.AuditLogs.Add(new AuditLog
            {
                IssueId = issue.Id,
                UserId = currentUserId,
                Action = "IssueCreated",
                Details = $"'{issue.Title}' görevi oluşturuldu.",
                CreatedAt = DateTime.UtcNow
            });

            if (dto.AssigneeId.HasValue && dto.AssigneeId.Value != currentUserId)
            {
                var notification = new Notification
                {
                    UserId = dto.AssigneeId.Value,
                    Title = "Yeni Görev Atandı",
                    Message = $"Size yeni bir görev atandı: '{issue.Title}'",
                    Type = "Assigned",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
                
                await _hubContext.Clients.User(notification.UserId.ToString())
                                     .SendAsync("NotificationReceived", notification);
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group($"project:{dto.ProjectId}").SendAsync("IssueCreated", issue);

            return Ok(issue);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIssue(int id, [FromBody] CreateIssueDto dto)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound("Görev bulunamadı.");

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (!await HasAccessToProjectAsync(currentUserId, currentUserRole, issue.ProjectId))
                return Forbid("Yetkiniz yok.");

            // --- YETKİ KONTROLÜ (27. Rol ve Yetki Matrisi: Due Date değiştirme) ---
            if (dto.DueDate != issue.DueDate && !CanManageDueDate(currentUserRole))
            {
                return Forbid("Due Date (Teslim Tarihi) alanını yalnızca PM (Project Manager) değiştirebilir.");
            }
            // ---------------------------------------------------------------------

            var oldDueDate = issue.DueDate;
            
            issue.Title = dto.Title;
            issue.Description = dto.Description;
            issue.IssueType = dto.IssueType;
            issue.Priority = dto.Priority;
            issue.AssigneeId = dto.AssigneeId;
            issue.DueDate = dto.DueDate;
            issue.EstimatedEffort = dto.EstimatedEffort;

            // AUDIT LOG: Due Date değiştirildi
            if (oldDueDate != dto.DueDate)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    IssueId = issue.Id,
                    UserId = currentUserId,
                    Action = "DueDateChanged",
                    Details = $"Teslim tarihi güncellendi: {dto.DueDate?.ToString("dd/MM/yyyy") ?? "Tarih kaldırıldı"}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group($"project:{issue.ProjectId}").SendAsync("IssueUpdated", issue);

            return Ok(issue);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateIssueStatus(int id, [FromBody] UpdateIssueStatusDto dto)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound("Görev bulunamadı.");

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (!await HasAccessToProjectAsync(currentUserId, currentUserRole, issue.ProjectId))
                return Forbid("Yetkiniz yok.");

            var oldStatus = issue.Status;
            issue.Status = dto.Status;

            if (oldStatus != dto.Status)
            {
                // AUDIT LOG: Issue durumu değiştirildi
                _context.AuditLogs.Add(new AuditLog
                {
                    IssueId = issue.Id,
                    UserId = currentUserId,
                    Action = "StatusChanged",
                    Details = $"Durum '{oldStatus}' -> '{dto.Status}' olarak değiştirildi.",
                    CreatedAt = DateTime.UtcNow
                });

                if (issue.AssigneeId.HasValue)
                {
                    var notification = new Notification
                    {
                        UserId = issue.AssigneeId.Value,
                        Title = "Görev Durumu Değişti",
                        Message = $"'{issue.Title}' adlı görevin durumu '{dto.Status}' olarak değiştirildi.",
                        Type = "StatusChanged",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notification);
                    
                    await _hubContext.Clients.User(notification.UserId.ToString())
                                         .SendAsync("NotificationReceived", notification);
                }
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group($"project:{issue.ProjectId}").SendAsync("IssueStatusChanged", new { issue.Id, issue.Status });

            return Ok(issue);
        }

        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignIssue(int id, [FromBody] UpdateAssigneeDto dto)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound("Görev bulunamadı.");

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (!await HasAccessToProjectAsync(currentUserId, currentUserRole, issue.ProjectId))
                return Forbid("Yetkiniz yok.");

            var oldAssignee = issue.AssigneeId;
            issue.AssigneeId = dto.AssigneeId;

            if (oldAssignee != dto.AssigneeId)
            {
                // AUDIT LOG: Issue başka kullanıcıya atandı
                string assignedTo = dto.AssigneeId.HasValue ? $"Kullanıcı ID: {dto.AssigneeId}" : "Atama kaldırıldı";
                _context.AuditLogs.Add(new AuditLog
                {
                    IssueId = issue.Id,
                    UserId = currentUserId,
                    Action = "Assigned",
                    Details = $"Görev ataması güncellendi: {assignedTo}",
                    CreatedAt = DateTime.UtcNow
                });

                if (dto.AssigneeId.HasValue && dto.AssigneeId.Value != currentUserId)
                {
                    var notification = new Notification
                    {
                        UserId = dto.AssigneeId.Value,
                        Title = "Yeni Görev Atandı",
                        Message = $"Size yeni bir görev atandı: '{issue.Title}'",
                        Type = "Assigned",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notification);
                    
                    await _hubContext.Clients.User(notification.UserId.ToString())
                                         .SendAsync("NotificationReceived", notification);
                }
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group($"project:{issue.ProjectId}").SendAsync("IssueAssigned", new { issue.Id, issue.AssigneeId });

            return Ok(new { message = "Görev ataması güncellendi.", issue });
        }

        [HttpPatch("{id}/priority")]
        public async Task<IActionResult> UpdatePriority(int id, [FromBody] UpdatePriorityDto dto)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound("Görev bulunamadı.");

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (!await HasAccessToProjectAsync(currentUserId, currentUserRole, issue.ProjectId))
                return Forbid("Yetkiniz yok.");

            // --- YETKİ KONTROLÜ (27. Rol ve Yetki Matrisi: Priority değiştirme) ---
            if (!CanChangePriority(currentUserRole))
            {
                return Forbid("Müşteri (Customer) ve Reporter rollerinin görev önceliğini (Priority) değiştirme yetkisi yoktur.");
            }
            // ---------------------------------------------------------------------

            var oldPriority = issue.Priority;
            issue.Priority = dto.Priority;

            // AUDIT LOG: Priority değiştirildi
            if (oldPriority != dto.Priority)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    IssueId = issue.Id,
                    UserId = currentUserId,
                    Action = "PriorityChanged",
                    Details = $"Öncelik '{oldPriority}' -> '{dto.Priority}' olarak değiştirildi.",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group($"project:{issue.ProjectId}").SendAsync("PriorityChanged", new { issue.Id, issue.Priority });

            return Ok(issue);
        }
    }

    public class CreateIssueDto { public string Title { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public string IssueType { get; set; } = "Task"; public string Priority { get; set; } = "Medium"; public int ProjectId { get; set; } public int? AssigneeId { get; set; } public DateTime? DueDate { get; set; } public decimal? EstimatedEffort { get; set; } }
    public class UpdateIssueStatusDto { public string Status { get; set; } = string.Empty; }
    public class UpdateAssigneeDto { public int? AssigneeId { get; set; } }
    public class UpdatePriorityDto { public string Priority { get; set; } = string.Empty; }
}