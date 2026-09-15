using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    // 1. Kullanıcı Girişi (JWT Token Alır)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized("E-posta veya şifre hatalı.");

        if (!user.IsActive)
            return BadRequest("Hesabınız henüz aktif edilmemiş. Lütfen aktivasyon bağlantısını kullanın.");

        var token = _tokenService.CreateToken(user);

        return Ok(new TokenResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role?.Name ?? "User" // Null uyarılarını önlemek için güvenli erişim eklendi
        });
    }

    // 2. Yalnızca Admin Kullanıcı Ekleyebilir (Dışarıya Açık Kayıt Yok)
    [Authorize(Roles = "Admin")]
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Bu e-posta adresi zaten kullanımda.");

        var activationToken = Guid.NewGuid().ToString("N");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            RoleId = dto.RoleId,
            IsActive = false,
            ActivationToken = activationToken
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Kullanıcı oluşturuldu.", ActivationToken = activationToken });
    }

    // 3. Aktivasyon Bağlantısı İle Şifre Belirleme
    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateAccountDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ActivationToken == dto.ActivationToken);

        if (user == null)
            return BadRequest("Geçersiz aktivasyon kodu.");

        user.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
        user.IsActive = true;
        user.ActivationToken = null;

        await _context.SaveChangesAsync();

        return Ok("Hesabınız başarıyla aktifleştirildi. Şimdi giriş yapabilirsiniz.");
    }
}