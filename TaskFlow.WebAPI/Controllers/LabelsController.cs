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
    public class LabelsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LabelsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Projeye Yeni Etiket (Label) Oluşturma
        [HttpPost]
        public async Task<IActionResult> CreateLabel([FromBody] CreateLabelDto dto)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId);
            if (!projectExists) return NotFound("Proje bulunamadı.");

            var label = new Label
            {
                Name = dto.Name,
                Color = dto.Color,
                ProjectId = dto.ProjectId
            };

            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            return Ok(label);
        }

        // 2. Projedeki Tüm Etiketleri Getirme
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetLabelsByProject(int projectId)
        {
            var labels = await _context.Labels
                .Where(l => l.ProjectId == projectId)
                .ToListAsync();

            return Ok(labels);
        }

        // 3. Göreve (Issue) Etiket Atama (Many-to-Many Ara Tabloya Kayıt)
        [HttpPost("issue/{issueId}")]
        public async Task<IActionResult> AddLabelToIssue(int issueId, [FromBody] AddLabelToIssueDto dto)
        {
            var issue = await _context.Issues.FindAsync(issueId);
            var label = await _context.Labels.FindAsync(dto.LabelId);

            if (issue == null || label == null) 
                return NotFound("Görev veya etiket bulunamadı.");

            // Etiket zaten ekli mi kontrolü
            var exists = await _context.IssueLabels
                .AnyAsync(il => il.IssueId == issueId && il.LabelId == dto.LabelId);
            
            if (exists) 
                return BadRequest("Bu etiket zaten bu göreve atanmış.");

            _context.IssueLabels.Add(new IssueLabel 
            { 
                IssueId = issueId, 
                LabelId = dto.LabelId 
            });
            
            await _context.SaveChangesAsync();

            return Ok(new { message = "Etiket göreve başarıyla eklendi." });
        }

        // 4. Görevden (Issue) Etiket Çıkarma
        [HttpDelete("issue/{issueId}/label/{labelId}")]
        public async Task<IActionResult> RemoveLabelFromIssue(int issueId, int labelId)
        {
            var issueLabel = await _context.IssueLabels
                .FirstOrDefaultAsync(il => il.IssueId == issueId && il.LabelId == labelId);

            if (issueLabel == null) 
                return NotFound("Bu görevde böyle bir etiket bulunamadı.");

            _context.IssueLabels.Remove(issueLabel);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Etiket görevden kaldırıldı." });
        }
    }

    public class CreateLabelDto
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#808080"; // Varsayılan renk (Gri)
        public int ProjectId { get; set; }
    }

    public class AddLabelToIssueDto
    {
        public int LabelId { get; set; }
    }
}