using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // Proje Bazlı Kapsamlı Dashboard Raporu (21. Madde)
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetProjectDashboard(int projectId)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            if (!projectExists) return NotFound("Proje bulunamadı.");

            // Projeye ait tüm görevleri, ilişkili atanan kişileri, çalışma loglarını ve sprintleri çekiyoruz
            var issues = await _context.Issues
                .Where(i => i.ProjectId == projectId)
                .Include(i => i.Assignee)
                .Include(i => i.WorkLogs)
                .Include(i => i.Sprint)
                .ToListAsync();

            var totalIssues = issues.Count;
            
            // Tamamlanan ve Açık İş Sayıları
            var completedIssues = issues.Count(i => 
                i.Status.Equals("Done", StringComparison.OrdinalIgnoreCase) || 
                i.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
            
            var openIssues = totalIssues - completedIssues;
            
            // Geciken İş Sayısı (Due Date geçmiş ve henüz tamamlanmamış)
            var now = DateTime.UtcNow;
            var overdueIssues = issues.Count(i => 
                i.DueDate.HasValue && 
                i.DueDate.Value < now && 
                !i.Status.Equals("Done", StringComparison.OrdinalIgnoreCase) && 
                !i.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
            
            // Kritik Öncelikli İş Sayısı
            var criticalIssues = issues.Count(i => i.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase));

            // Bug / Task / Feature Dağılımı (Type Distribution)
            var typeDistribution = issues
                .GroupBy(i => i.IssueType)
                .ToDictionary(g => g.Key, g => g.Count());

            // Status Dağılımı (Status Distribution)
            var statusDistribution = issues
                .GroupBy(i => i.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Kullanıcı Bazlı İş Yükü (User Workload)
            var userWorkload = issues
                .Where(i => i.Assignee != null)
                .GroupBy(i => i.Assignee!.FullName)
                .Select(g => new
                {
                    UserName = g.Key,
                    AssignedIssuesCount = g.Count(),
                    TotalEstimatedEffort = g.Sum(i => i.EstimatedEffort ?? 0)
                })
                .ToList();

            // Tahmini ve Gerçekleşen Toplam Efor
            var totalEstimatedEffort = issues.Sum(i => i.EstimatedEffort ?? 0);
            var totalHoursSpent = issues.SelectMany(i => i.WorkLogs).Sum(w => w.HoursSpent);

            // Sprint İlerleme Oranları (Sprint Progress)
            var sprints = await _context.Sprints
                .Where(s => s.ProjectId == projectId)
                .Include(s => s.Issues)
                .ToListAsync();

            var sprintProgress = sprints.Select(s => new
            {
                SprintId = s.Id,
                SprintName = s.Name,
                Status = s.Status,
                TotalIssues = s.Issues.Count,
                CompletedIssues = s.Issues.Count(i => 
                    i.Status.Equals("Done", StringComparison.OrdinalIgnoreCase) || 
                    i.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
                ProgressPercentage = s.Issues.Count > 0 
                    ? Math.Round((double)s.Issues.Count(i => i.Status.Equals("Done", StringComparison.OrdinalIgnoreCase) || i.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)) / s.Issues.Count * 100, 2) 
                    : 0
            }).ToList();

            // Tüm verileri tek bir Dashboard JSON paketi halinde topluyoruz
            var dashboardData = new
            {
                ProjectId = projectId,
                Metrics = new
                {
                    TotalIssues = totalIssues,
                    OpenIssues = openIssues,
                    CompletedIssues = completedIssues,
                    OverdueIssues = overdueIssues,
                    CriticalIssues = criticalIssues
                },
                Distributions = new
                {
                    TypeDistribution = typeDistribution,
                    StatusDistribution = statusDistribution
                },
                UserWorkload = userWorkload,
                EffortSummary = new
                {
                    TotalEstimatedEffort = totalEstimatedEffort, // Tahmini Efor (Saat / Puan)
                    TotalHoursSpent = totalHoursSpent           // Gerçekleşen Efor (WorkLog üzerinden)
                },
                SprintProgress = sprintProgress
            };

            return Ok(dashboardData);
        }
    }
}