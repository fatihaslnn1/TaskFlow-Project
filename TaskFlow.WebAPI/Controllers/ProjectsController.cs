using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.WebAPI.Hubs;

namespace TaskFlow.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<TaskFlowHub> _hubContext;

        public ProjectsController(AppDbContext context, IHubContext<TaskFlowHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,PM")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            if (await _context.Projects.AnyAsync(p => p.ProjectKey == dto.ProjectKey))
                return BadRequest("Bu proje anahtarı zaten kullanılıyor.");

            var project = new Project
            {
                ProjectName = dto.ProjectName,
                ProjectKey = dto.ProjectKey.ToUpper(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // EVENT: SprintUpdated (Proje/Sprint seviyesi değişiklikler)
            await _hubContext.Clients.All.SendAsync("SprintUpdated", project);

            return Ok(project);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            return Ok(await _context.Projects.ToListAsync());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,PM")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] CreateProjectDto dto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound("Proje bulunamadı.");

            project.ProjectName = dto.ProjectName;
            await _context.SaveChangesAsync();

            // EVENT: SprintUpdated
            await _hubContext.Clients.Group($"project:{project.Id}").SendAsync("SprintUpdated", project);

            return Ok(project);
        }
    }
}