using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;
using TaskFlow.WebAPI.Hubs;

namespace TaskFlow.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkLogsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<TaskFlowHub> _hubContext;

        public WorkLogsController(AppDbContext context, IHubContext<TaskFlowHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Authorize(Roles = "Developer,PM,Admin")]
        public async Task<IActionResult> CreateWorkLog([FromBody] CreateWorkLogDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var workLog = new WorkLog
            {
                IssueId = dto.IssueId,
                UserId = int.Parse(userIdClaim),
                HoursSpent = dto.HoursSpent,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.WorkLogs.Add(workLog);
            await _context.SaveChangesAsync();

            var issue = await _context.Issues.FindAsync(dto.IssueId);
            if (issue != null)
            {
                // EVENT: WorkLogAdded
                await _hubContext.Clients.Group($"project:{issue.ProjectId}").SendAsync("WorkLogAdded", workLog);
            }

            return Ok(new { message = "Çalışma süresi başarıyla eklendi.", workLog });
        }

        [HttpGet("issue/{issueId}")]
        public async Task<IActionResult> GetWorkLogsByIssue(int issueId)
        {
            var logs = await _context.WorkLogs.Where(w => w.IssueId == issueId).ToListAsync();
            var totalSpent = logs.Sum(w => w.HoursSpent);
            var issue = await _context.Issues.FindAsync(issueId);
            var estimated = issue?.EstimatedEffort ?? 0;

            return Ok(new
            {
                WorkLogs = logs,
                TotalSpent = totalSpent,
                EstimatedEffort = estimated,
                ProgressPercentage = estimated > 0 ? Math.Min(100, (double)((totalSpent / estimated) * 100)) : 0
            });
        }
    }
}