using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound(new { message = "Bildirim bulunamadı." });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bildirim okundu olarak işaretlendi." });
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return Ok(notification);
        }
    }
}