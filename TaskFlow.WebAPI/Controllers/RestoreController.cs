using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RestoreController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestoreController(AppDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // --- YARDIMCI METOT: Rol Matrisi Kuralı (Issue Restore) ---
        private bool CanRestoreItems(string role)
        {
            // Matrise göre silinen görevleri/projeleri geri getirme yetkisi yalnızca PM ve Admin'dedir.
            return role == "Admin" || role == "PM";
        }
        // -----------------------------------------------------------

        // 1. Silinen Projeleri Listeleme
        [HttpGet("projects/deleted")]
        public async Task<IActionResult> GetDeletedProjects()
        {
            var role = GetCurrentUserRole();
            if (!CanRestoreItems(role))
            {
                return Forbid("Silinen kayıtları görüntüleme ve yönetme yetkiniz yok.");
            }

            // IgnoreQueryFilters() sayesinde IsDeleted = true olanları da çekeriz.
            var deletedProjects = await _context.Projects
                .IgnoreQueryFilters()
                .Where(p => p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.ProjectName,
                    p.ProjectKey,
                    p.DeletedAt,
                    p.DeletedBy
                })
                .ToListAsync();

            return Ok(deletedProjects);
        }

        // 2. Silinen Bir Projeyi Geri Getirme (Restore)
        [HttpPost("projects/{id}/restore")]
        public async Task<IActionResult> RestoreProject(int id)
        {
            var role = GetCurrentUserRole();
            if (!CanRestoreItems(role))
            {
                return Forbid("Projeleri geri getirme (Restore) yetkisi yalnızca PM ve Admin rollerine aittir.");
            }

            var project = await _context.Projects
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted);

            if (project == null) return NotFound("Silinmiş proje bulunamadı.");

            // Soft delete alanlarını sıfırlayarak kaydı tekrar aktifleştiriyoruz
            project.IsDeleted = false;
            project.DeletedAt = null;
            project.DeletedBy = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"'{project.ProjectName}' projesi başarıyla geri yüklendi." });
        }

        // 3. Silinen İşleri (Issues) Listeleme
        [HttpGet("issues/deleted")]
        public async Task<IActionResult> GetDeletedIssues()
        {
            var role = GetCurrentUserRole();
            if (!CanRestoreItems(role))
            {
                return Forbid("Silinen kayıtları görüntüleme ve yönetme yetkiniz yok.");
            }

            var deletedIssues = await _context.Issues
                .IgnoreQueryFilters()
                .Where(i => i.IsDeleted)
                .Select(i => new
                {
                    i.Id,
                    i.IssueKey,
                    i.Title,
                    i.DeletedAt,
                    i.DeletedBy
                })
                .ToListAsync();

            return Ok(deletedIssues);
        }

        // 4. Silinen Bir İşi (Issue) Geri Getirme (Restore)
        [HttpPost("issues/{id}/restore")]
        public async Task<IActionResult> RestoreIssue(int id)
        {
            var role = GetCurrentUserRole();
            
            // --- YETKİ KONTROLÜ (27. Rol ve Yetki Matrisi: Issue restore) ---
            if (!CanRestoreItems(role))
            {
                return Forbid("Görevleri geri getirme (Issue restore) yetkisi yalnızca PM ve Admin rollerine aittir.");
            }
            // ---------------------------------------------------------------

            var issue = await _context.Issues
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(i => i.Id == id && i.IsDeleted);

            if (issue == null) return NotFound("Silinmiş görev bulunamadı.");

            issue.IsDeleted = false;
            issue.DeletedAt = null;
            issue.DeletedBy = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"'{issue.IssueKey}' görevi başarıyla geri yüklendi." });
        }
    }
}