using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Domain.Entities;
using TaskFlow.WebAPI.Hubs;

namespace TaskFlow.WebAPI.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<TaskFlowHub> _hubContext;

        public NotificationBackgroundService(IServiceProvider serviceProvider, IHubContext<TaskFlowHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var now = DateTime.UtcNow;
                    var createdNotifications = new List<Notification>();

                    // 1. Yaklaşan Görevler (24 saatten az kalanlar)
                    var upcomingIssues = await dbContext.Issues
                        .Where(i => i.DueDate.HasValue && i.DueDate.Value > now && i.DueDate.Value <= now.AddHours(24) && i.Status != "Done" && i.AssigneeId.HasValue)
                        .ToListAsync(stoppingToken);

                    foreach (var issue in upcomingIssues)
                    {
                        bool alreadyExists = await dbContext.Notifications.AnyAsync(n => 
                            n.RelatedIssueId == issue.Id && n.Type == "DueDateApproaching" && n.UserId == issue.AssigneeId!.Value, stoppingToken);

                        if (!alreadyExists)
                        {
                            var notif = new Notification
                            {
                                UserId = issue.AssigneeId!.Value,
                                Title = "Due Date Yaklaşıyor",
                                Message = $"'{issue.Title}' görevinin son teslim tarihine 24 saatten az kaldı.",
                                Type = "DueDateApproaching",
                                RelatedIssueId = issue.Id,
                                IsRead = false,
                                CreatedAt = now
                            };
                            dbContext.Notifications.Add(notif);
                            createdNotifications.Add(notif);
                        }
                    }

                    // 2. Geciken Görevler (Overdue)
                    var overdueIssues = await dbContext.Issues
                        .Where(i => i.DueDate.HasValue && i.DueDate.Value < now && i.Status != "Done" && i.AssigneeId.HasValue)
                        .ToListAsync(stoppingToken);

                    foreach (var issue in overdueIssues)
                    {
                        bool alreadyExists = await dbContext.Notifications.AnyAsync(n => 
                            n.RelatedIssueId == issue.Id && n.Type == "Overdued" && n.UserId == issue.AssigneeId!.Value, stoppingToken);

                        if (!alreadyExists)
                        {
                            var notif = new Notification
                            {
                                UserId = issue.AssigneeId!.Value,
                                Title = "Görev Gecikti",
                                Message = $"'{issue.Title}' görevinin süresi doldu ve henüz tamamlanmadı!",
                                Type = "Overdued",
                                RelatedIssueId = issue.Id,
                                IsRead = false,
                                CreatedAt = now
                            };
                            dbContext.Notifications.Add(notif);
                            createdNotifications.Add(notif);
                        }
                    }

                    // Kaydet ve SignalR üzerinden resmi 'NotificationReceived' eventini fırlat
                    if (createdNotifications.Any())
                    {
                        await dbContext.SaveChangesAsync(stoppingToken);
                        
                        foreach (var notif in createdNotifications)
                        {
                            await _hubContext.Clients.User(notif.UserId.ToString())
                                             .SendAsync("NotificationReceived", notif, cancellationToken: stoppingToken);
                        }
                    }
                }

                // Saatte bir çalışması için bekleme süresi
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}