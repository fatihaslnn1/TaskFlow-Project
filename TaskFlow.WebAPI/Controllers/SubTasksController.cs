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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubTasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<TaskFlowHub> _hubContext; // SignalR eklendi

        public SubTasksController(AppDbContext context, IHubContext<TaskFlowHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext; // SignalR eklendi
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubTask([FromBody] CreateSubTaskDto dto)
        {
            // --- YETKİ KONTROLÜ (27. Rol ve Yetki Matrisi: Sub-task yönetme) ---
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!CanManageSubTasks(role))
            {
                return Forbid("Bu işlem için yetkiniz yok. (Sub-task yönetme yetkisi yalnızca Developer ve PM rollerine aittir.)");
            }
            // -----------------------------------------------------------------

            var issue = await _context.Issues.FindAsync(dto.IssueId);
            if (issue == null)
                return NotFound("Ana görev (Issue) bulunamadı.");

            var subTask = new SubTask
            {
                IssueId = dto.IssueId,
                Title = dto.Title,
                IsCompleted = false
            };

            _context.SubTasks.Add(subTask);
            await _context.SaveChangesAsync();

            // İsteğe bağlı: Yeni alt görev eklendiğinde de panoyu güncellemek için fırlatılabilir
            await _hubContext.Clients.Group($"project:{issue.ProjectId}")
                           .SendAsync("SubTaskUpdated", subTask);

            return Ok(new { message = "Alt görev başarıyla oluşturuldu.", subTask });
        }

        [HttpGet("issue/{issueId}")]
        public async Task<IActionResult> GetSubTasksByIssue(int issueId)
        {
            // Her rol projeyi ve işleri görebildiği için listelemede kısıtlama yoktur (Matris kuralı)
            var subTasks = await _context.SubTasks
                .Where(s => s.IssueId == issueId)
                .ToListAsync();

            return Ok(subTasks);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateSubTaskStatus(int id, [FromBody] UpdateSubTaskStatusDto dto)
        {
            // --- YETKİ KONTROLÜ (27. Rol ve Yetki Matrisi: Sub-task yönetme) ---
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!CanManageSubTasks(role))
            {
                return Forbid("Bu işlem için yetkiniz yok. (Sub-task yönetme yetkisi yalnızca Developer ve PM rollerine aittir.)");
            }
            // -----------------------------------------------------------------

            // Alt görevi ve bağlı olduğu Issue'yu (projeyi bulabilmek için) dahil ediyoruz
            var subTask = await _context.SubTasks
                .Include(st => st.Issue)
                .FirstOrDefaultAsync(st => st.Id == id);

            if (subTask == null)
                return NotFound("Alt görev bulunamadı.");

            subTask.IsCompleted = dto.IsCompleted;

            // YENİ EKLENEN KISIM: AUDIT LOG (Alt Görev Durumu Logu)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null)
            {
                string statusMsg = dto.IsCompleted ? "tamamlandı" : "tamamlanmadı olarak işaretlendi";
                _context.AuditLogs.Add(new AuditLog
                {
                    IssueId = subTask.IssueId,
                    UserId = int.Parse(userIdClaim),
                    Action = "SubTaskUpdated",
                    Details = $"'{subTask.Title}' alt görevi {statusMsg}.",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            // SIGNALR: Dokümandaki resmi 'SubTaskUpdated' eventi fırlatılıyor
            if (subTask.Issue != null)
            {
                await _hubContext.Clients.Group($"project:{subTask.Issue.ProjectId}")
                                         .SendAsync("SubTaskUpdated", new 
                                         {
                                             subTask.Id,
                                             subTask.IssueId,
                                             subTask.Title,
                                             subTask.IsCompleted,
                                             message = "Alt görev durumu güncellendi."
                                         });
            }

            return Ok(new { message = "Alt görev durumu güncellendi.", subTask });
        }

        // --- YARDIMCI METOT: Rol Matrisi Kuralı ---
        private bool CanManageSubTasks(string? roleName)
        {
            // Matrise göre Sub-task yönetme yetkisi sadece Developer, PM ve Admin'dedir.
            return roleName == "Admin" || roleName == "PM" || roleName == "Developer";
        }
    }

    public class CreateSubTaskDto
    {
        public int IssueId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class UpdateSubTaskStatusDto
    {
        public bool IsCompleted { get; set; }
    }
}