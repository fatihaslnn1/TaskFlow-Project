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
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
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

        // 1. Audit Logları Listeleme (Rol Matrisine Uygun Sınırlama ile)
        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int? issueId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var query = _context.AuditLogs.AsQueryable();

            // Eğer özel bir Issue ID verilmişse filtrele
            if (issueId.HasValue)
            {
                query = query.Where(a => a.IssueId == issueId.Value);
            }

            // --- YETKİ KONTROLÜ (27. Rol ve Yetki Matrisi: Audit Log görme) ---
            // Müşteri ve Reporter sadece kendi işlemlerini veya kendi açtıkları işlerin loglarını görebilir (Sınırlı).
            if (currentUserRole == "Customer" || currentUserRole == "Reporter")
            {
                var userIssueIds = await _context.Issues
                    .Where(i => i.ReporterId == currentUserId)
                    .Select(i => i.Id)
                    .ToListAsync();

                query = query.Where(a => a.UserId == currentUserId || userIssueIds.Contains(a.IssueId));
            }
            // Admin, PM ve Developer rolleri tüm logları görebilir (Evet).
            // -----------------------------------------------------------------

            // Kullanıcı ve Görev tabloları ile güvenli birleştirme (Join) yapıyoruz
            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Join(_context.Users,
                      a => a.UserId,
                      u => u.Id,
                      (a, u) => new { AuditLog = a, User = u })
                .GroupJoin(_context.Issues,
                           x => x.AuditLog.IssueId,
                           i => i.Id,
                           (x, issues) => new { x.AuditLog, x.User, Issue = issues.FirstOrDefault() })
                .Select(res => new
                {
                    res.AuditLog.Id,
                    res.AuditLog.Action,
                    res.AuditLog.Details,
                    res.AuditLog.CreatedAt,
                    UserName = res.User != null ? res.User.FullName : "Bilinmiyor",
                    IssueKey = res.Issue != null ? res.Issue.IssueKey : null
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}