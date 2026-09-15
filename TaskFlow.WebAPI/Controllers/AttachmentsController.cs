using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Services;
using TaskFlow.WebAPI.Hubs;

namespace TaskFlow.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttachmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FileService _fileService;
        private readonly IHubContext<TaskFlowHub> _hubContext;

        public AttachmentsController(AppDbContext context, FileService fileService, IHubContext<TaskFlowHub> hubContext)
        {
            _context = context;
            _fileService = fileService;
            _hubContext = hubContext;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAttachment(IFormFile file, [FromForm] int? issueId, [FromForm] int? commentId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            // --- YETKİ KONTROLÜ (27. Rol ve Yetki Matrisi: Attachment yükleme) ---
            if (issueId.HasValue)
            {
                var issue = await _context.Issues.FindAsync(issueId.Value);
                if (issue == null) return NotFound("İlgili görev (Issue) bulunamadı.");

                // Eğer kullanıcı "Customer" (Müşteri) ise, sadece kendi açtığı işe dosya yükleyebilir.
                if (role == "Customer" && issue.ReporterId != userId)
                {
                    return Forbid("Müşteri rolüyle yalnızca kendi oluşturduğunuz (Reporter'ı olduğunuz) işlere dosya yükleyebilirsiniz.");
                }
            }
            // -----------------------------------------------------------------

            var (isValid, message, filePath, originalName) = await _fileService.SaveFileAsync(file);
            if (!isValid) return BadRequest(new { message });

            var attachment = new Attachment
            {
                FileName = originalName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                IssueId = issueId,
                CommentId = commentId,
                UserId = userId
            };

            _context.Attachments.Add(attachment);

            // AUDIT LOG: Dosya eklendi
            if (issueId.HasValue)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    IssueId = issueId.Value,
                    UserId = userId,
                    Action = "AttachmentAdded",
                    Details = $"'{originalName}' isimli dosya eklendi.",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            if (issueId.HasValue)
            {
                var targetIssue = await _context.Issues.FindAsync(issueId.Value);
                if (targetIssue != null)
                {
                    await _hubContext.Clients.Group($"project:{targetIssue.ProjectId}").SendAsync("AttachmentAdded", attachment);
                }
            }

            return Ok(new { message = "Dosya başarıyla yüklendi.", attachmentId = attachment.Id, filePath });
        }
    }
}