using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.WebAPI.Hubs
{
    [Authorize] // 1. KURAL: SignalR tamamen JWT koruması altında
    public class TaskFlowHub : Hub
    {
        private readonly AppDbContext _context;

        public TaskFlowHub(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        // GÜVENLİK YAMASI: 4. KURAL UYGULANDI (Frontend'den gelen ID'ye güvenme)
        public async Task JoinProjectGroup(int projectId)
        {
            var userIdStr = Context.UserIdentifier;
            if (userIdStr == null) throw new HubException("Kimlik doğrulanamadı.");

            int userId = int.Parse(userIdStr);
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            bool hasAccess = false;
            
            if (role == "Admin")
            {
                hasAccess = true; // Admin her projeye girebilir
            }
            else
            {
                // Kullanıcı gerçekten bu projenin üyesi mi kontrolü
                hasAccess = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
            }

            if (hasAccess)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"project:{projectId}");
            }
            else
            {
                // Yetkisi yoksa isteği reddet (Odaya almaz)
                throw new HubException("Bu projeye erişim yetkiniz bulunmamaktadır.");
            }
        }

        public async Task LeaveProjectGroup(int projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project:{projectId}");
        }
    }
}