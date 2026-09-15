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
    public class IssueTemplatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IssueTemplatesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Tüm Şablonları Listeleme
        [HttpGet]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _context.IssueTemplates.ToListAsync();
            return Ok(templates);
        }

        // 2. Issue Tipine Göre Şablon Getirme (Örn: "Bug")
        [HttpGet("type/{issueType}")]
        public async Task<IActionResult> GetTemplateByType(string issueType)
        {
            var template = await _context.IssueTemplates
                .FirstOrDefaultAsync(t => t.IssueType.ToLower() == issueType.ToLower());

            if (template == null) return NotFound("Bu issue tipi için şablon bulunamadı.");

            return Ok(template);
        }

        // 3. Yeni Şablon Oluşturma (Admin/PM kullanabilir)
        [HttpPost]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateDto dto)
        {
            var template = new IssueTemplate
            {
                Name = dto.Name,
                IssueType = dto.IssueType,
                Content = dto.Content
            };

            _context.IssueTemplates.Add(template);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Şablon başarıyla oluşturuldu.", template });
        }
    }

    public class CreateTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string IssueType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}