using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}