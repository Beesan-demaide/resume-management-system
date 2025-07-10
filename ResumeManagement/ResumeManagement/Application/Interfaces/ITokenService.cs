using ResumeManagement.Models;

namespace ResumeManagement.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }

}
