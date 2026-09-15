using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SprintsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SprintsController(AppDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // --- YARDIMCI METOT: Rol Matrisi Kuralı (Sprint Yönetme) ---
        private bool CanManageSprints(string role)
        {
            // Matrise göre sprint yönetme yetkisi yalnızca PM ve Admin rollerindedir.
            return role == "Admin" || role == "PM";
        }
        // -----------------------------------------------------------

        // 1. Sprint Oluşturma
        [HttpPost]
        public async Task<IActionResult> CreateSprint([FromBody] CreateSprintDto dto)
        {
            var currentUserRole = GetCurrentUserRole();

            // --- YETKİ KONTROLÜ (27. Rol ve Yetki Matrisi: Sprint yönetme) ---
            if (!CanManageSprints(currentUserRole))
            {
                return Forbid("Sprint yönetme yetkisi yalnızca PM (Project Manager) ve Admin rollerine aittir.");
            }
            // -----------------------------------------------------------------

            var projectExists = await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId);
            if (!projectExists) return NotFound("Proje bulunamadı.");

            var sprint = new Sprint
            {
                ProjectId = dto.ProjectId,
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Goal = dto.Goal,
                Status = "Planned"
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sprint başarıyla oluşturuldu.", sprint });
        }

        // 2. Projedeki Sprintleri Listeleme (Matris: Projeyi ve işleri görme - Tüm roller için Evet)
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetSprintsByProject(int projectId)
        {
            var sprints = await _context.Sprints
                .Where(s => s.ProjectId == projectId)
                .ToListAsync();

            return Ok(sprints);
        }

        // 3. Sprint Sonu Raporu Endpoint'i (19. Madde Detayı)
        [HttpGet("{id}/report")]
        public async Task<IActionResult> GetSprintReport(int id)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Issues)
                    .ThenInclude(i => i.WorkLogs) // Efor ve süre hesaplaması için WorkLog'ları dahil ediyoruz
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null)
                return NotFound("Sprint bulunamadı.");

            var totalIssues = sprint.Issues.Count;
            
            // Durumu "Done" veya "Completed" olan işler tamamlanmış sayılır
            var completedIssues = sprint.Issues.Count(i => i.Status.ToLower() == "done" || i.Status.ToLower() == "completed");
            var remainingIssues = totalIssues - completedIssues;

            // Toplam Tahmini Efor (EstimatedEffort)
            var totalEstimatedEffort = sprint.Issues.Sum(i => i.EstimatedEffort ?? 0);

            // Toplam Gerçekleşen Süre (WorkLogs tablosundaki HoursSpent toplamı)
            var totalHoursSpent = sprint.Issues
                .SelectMany(i => i.WorkLogs)
                .Sum(w => w.HoursSpent);

            var report = new
            {
                SprintId = sprint.Id,
                SprintName = sprint.Name,
                Goal = sprint.Goal,
                Status = sprint.Status,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                Metrics = new
                {
                    TotalIssues = totalIssues,
                    CompletedIssues = completedIssues,
                    RemainingIssues = remainingIssues,
                    CompletionPercentage = totalIssues > 0 ? Math.Round((double)completedIssues / totalIssues * 100, 2) : 0,
                    TotalEstimatedEffort = totalEstimatedEffort, // Tahmini Efor
                    TotalHoursSpent = totalHoursSpent          // Gerçekleşen Süre
                }
            };

            return Ok(report);
        }
    }

    public class CreateSprintDto
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Goal { get; set; } = string.Empty;
    }
}