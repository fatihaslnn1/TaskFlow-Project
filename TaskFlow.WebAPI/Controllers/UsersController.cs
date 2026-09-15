using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Tüm Kullanıcıları Listeleme (E-posta ve ID Görme)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new 
                { 
                    u.Id, 
                    u.Email, 
                    u.FullName, 
                    u.IsActive 
                })
                .ToListAsync();
            
            return Ok(users);
        }

        // 2. Admin Tarafından Kullanıcı Oluşturma
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Bu e-posta adresi ile zaten bir kullanıcı mevcut." });

            var token = Guid.NewGuid().ToString(); // Aktivasyon token'ı üretiliyor

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = "", // İlk başta şifre yok, aktivasyonda belirlenecek
                RoleId = dto.RoleId,
                IsActive = false,
                ActivationToken = token
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Normalde burada e-posta gönderimi yapılır. Staj projesi için linki dönüyoruz:
            var activationLink = $"http://localhost:5175/activate?token={token}";

            return Ok(new { message = "Kullanıcı başarıyla oluşturuldu.", activationLink });
        }

        // 3. Kullanıcının Aktivasyon Linki ile İlk Şifresini Belirlemesi
        [HttpPost("activate")]
        public async Task<IActionResult> ActivateAccount([FromBody] SetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ActivationToken == dto.Token);

            if (user == null)
                return BadRequest(new { message = "Geçersiz veya süresi dolmuş aktivasyon token'ı." });

            user.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
            user.IsActive = true;
            user.ActivationToken = null; // Token bir kez kullanılır

            await _context.SaveChangesAsync();

            return Ok(new { message = "Şifreniz başarıyla belirlendi. Artık giriş yapabilirsiniz." });
        }
    }
}