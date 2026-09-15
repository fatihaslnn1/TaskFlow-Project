using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,PM")]
public class ProjectMembersController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProjectMembersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddMember([FromBody] AddProjectMemberDto dto)
    {
        var exists = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == dto.ProjectId && pm.UserId == dto.UserId);

        if (exists)
            return BadRequest("Kullanıcı zaten bu projenin üyesidir.");

        var member = new ProjectMember
        {
            ProjectId = dto.ProjectId,
            UserId = dto.UserId
        };

        _context.ProjectMembers.Add(member);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Kullanıcı projeye başarıyla eklendi." });
    }
}

public class AddProjectMemberDto
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }
}