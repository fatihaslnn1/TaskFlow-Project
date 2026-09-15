using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        // Global Arama Endpoint'i (22. Madde - CTRL+K / CMD+K Arama Altyapısı)
        [HttpGet]
        public async Task<IActionResult> GlobalSearch([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            {
                return BadRequest("Arama yapmak için en az 2 karakter girmelisiniz.");
            }

            var searchTerm = query.Trim().ToLower();
            int.TryParse(searchTerm, out var parsedId);

            // 1. Issue Araması (IssueKey veya Title üzerinden)
            var issues = await _context.Issues
                .Where(i => i.IssueKey.ToLower().Contains(searchTerm) || i.Title.ToLower().Contains(searchTerm))
                .Select(i => new
                {
                    i.Id,
                    i.IssueKey,
                    i.Title,
                    i.Status,
                    i.Priority,
                    i.ProjectId
                })
                .Take(10)
                .ToListAsync();

            // 2. Project Araması (Id üzerinden güvenli arama)
            var projects = await _context.Projects
                .Where(p => p.Id == parsedId || p.Id.ToString().Contains(searchTerm))
                .Select(p => new
                {
                    p.Id
                })
                .Take(5)
                .ToListAsync();

            // 3. User Araması (FullName veya Email üzerinden)
            var users = await _context.Users
                .Where(u => u.FullName.ToLower().Contains(searchTerm) || u.Email.ToLower().Contains(searchTerm))
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email
                })
                .Take(5)
                .ToListAsync();

            var searchResult = new
            {
                Query = query,
                Results = new
                {
                    Issues = issues,
                    Projects = projects,
                    Users = users
                }
            };

            return Ok(searchResult);
        }
    }
}