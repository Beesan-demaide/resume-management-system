using ResumeManagement.Application.DTOs;

namespace ResumeManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
    }

}