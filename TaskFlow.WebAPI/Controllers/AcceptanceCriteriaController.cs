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
    public class AcceptanceCriteriaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AcceptanceCriteriaController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Feature Request'e Yeni Kabul Kriteri Ekleme
        [HttpPost]
        public async Task<IActionResult> CreateCriteria([FromBody] CreateCriteriaDto dto)
        {
            var featureRequestExists = await _context.FeatureRequests.AnyAsync(fr => fr.Id == dto.FeatureRequestId);
            if (!featureRequestExists) return NotFound("Feature Request bulunamadı.");

            var criteria = new AcceptanceCriteria
            {
                FeatureRequestId = dto.FeatureRequestId,
                Description = dto.Description,
                IsCompleted = false
            };

            _context.AcceptanceCriterias.Add(criteria);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kabul kriteri başarıyla eklendi.", criteria });
        }

        // 2. Bir Feature Request'in Kriterlerini Listeleme
        [HttpGet("feature-request/{featureRequestId}")]
        public async Task<IActionResult> GetCriteriasByFeatureRequest(int featureRequestId)
        {
            var criterias = await _context.AcceptanceCriterias
                .Where(ac => ac.FeatureRequestId == featureRequestId)
                .ToListAsync();

            return Ok(criterias);
        }

        // 3. Kriter Durumunu Güncelleme (Tamamlandı / Tamamlanmadı)
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleCriteria(int id)
        {
            var criteria = await _context.AcceptanceCriterias.FindAsync(id);
            if (criteria == null) return NotFound("Kabul kriteri bulunamadı.");

            criteria.IsCompleted = !criteria.IsCompleted;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kriter durumu güncellendi.", criteria });
        }

        // 4. Kriter Silme
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCriteria(int id)
        {
            var criteria = await _context.AcceptanceCriterias.FindAsync(id);
            if (criteria == null) return NotFound("Kabul kriteri bulunamadı.");

            _context.AcceptanceCriterias.Remove(criteria);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kabul kriteri silindi." });
        }
    }

    public class CreateCriteriaDto
    {
        public int FeatureRequestId { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}