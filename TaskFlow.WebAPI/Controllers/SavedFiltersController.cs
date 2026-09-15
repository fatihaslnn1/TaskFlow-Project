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
    public class SavedFiltersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SavedFiltersController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Yeni Filtre Kaydetme
        [HttpPost]
        public async Task<IActionResult> CreateSavedFilter([FromBody] CreateSavedFilterDto dto)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists) return NotFound("Kullanıcı bulunamadı.");

            var savedFilter = new SavedFilter
            {
                UserId = dto.UserId,
                Name = dto.Name,
                CriteriaJson = dto.CriteriaJson
            };

            _context.SavedFilters.Add(savedFilter);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Filtre başarıyla kaydedildi.", savedFilter });
        }

        // 2. Kullanıcıya Ait Kaydedilmiş Filtreleri Listeleme
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetFiltersByUser(int userId)
        {
            var filters = await _context.SavedFilters
                .Where(sf => sf.UserId == userId)
                .ToListAsync();

            return Ok(filters);
        }

        // 3. Kaydedilmiş Filtreyi Silme
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSavedFilter(int id)
        {
            var filter = await _context.SavedFilters.FindAsync(id);
            if (filter == null) return NotFound("Kaydedilmiş filtre bulunamadı.");

            _context.SavedFilters.Remove(filter);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kaydedilmiş filtre silindi." });
        }
    }

    public class CreateSavedFilterDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;       // Örn: "Benim Kritik İşlerim"
        public string CriteriaJson { get; set; } = string.Empty; // Örn: "assigneeId=5&priority=Critical"
    }
}