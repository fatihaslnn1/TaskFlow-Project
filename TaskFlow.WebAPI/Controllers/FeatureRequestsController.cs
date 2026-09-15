using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.WebAPI.Hubs;

namespace TaskFlow.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FeatureRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<TaskFlowHub> _hubContext;

        public FeatureRequestsController(AppDbContext context, IHubContext<TaskFlowHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateFeatureRequestStatus(int id, [FromBody] string status)
        {
            var featureRequest = await _context.FeatureRequests.FindAsync(id);
            if (featureRequest == null) return NotFound("Özellik talebi bulunamadı.");

            featureRequest.Status = status;

            var notification = new Notification
            {
                UserId = featureRequest.UserId,
                Title = "Feature Request Durumu",
                Message = $"'{featureRequest.Title}' başlıklı özellik talebiniz {status}!", 
                Type = "FeatureRequestResult",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // EVENT: NotificationReceived
            await _hubContext.Clients.User(featureRequest.UserId.ToString())
                             .SendAsync("NotificationReceived", notification);

            return Ok(new { message = "Talep durumu güncellendi.", featureRequest });
        }
    }
}