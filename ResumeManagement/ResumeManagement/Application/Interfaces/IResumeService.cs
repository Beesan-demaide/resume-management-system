using ResumeManagement.Application.DTOs;
using ResumeManagement.Models;

namespace ResumeManagement.Application.Interfaces
{
    public interface IResumeService
    {
        Task<List<Resume>> Search(ResumeSearchDto dto);
        Task<byte[]> DownloadResume(Guid resumeId, Guid userId);


    }
}
