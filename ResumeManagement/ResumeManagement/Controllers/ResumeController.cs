using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
    using global::ResumeManagement.Application.DTOs;
    using global::ResumeManagement.Application.Interfaces;
    using global::ResumeManagement.Application.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    namespace ResumeManagement.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class ResumeController : ControllerBase
        {
            private readonly ResumeService _resumeService;
            private readonly ILogService _logService;


            public ResumeController(ResumeService resumeService, ILogService logService)
            {
                _resumeService = resumeService;
                _logService = logService;
            }

            [Authorize(Roles = "Admin,HR")]
            [HttpPost("upload")]
            public async Task<IActionResult> UploadResume(IFormFile file)
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Invalid file");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid token");

                var resume = await _resumeService.UploadResumeAsync(file, userId);

                await _logService.Log(
               action: "Upload Resume",
               performedBy: userId.ToString(),
               details: $"Uploaded resume with ID: {resume.Id}"
           );
                return Ok(resume);
            }
            [Authorize(Roles = "Admin,HR,Recruiter")]
            [HttpPost("search")]
            public async Task<IActionResult> Search([FromBody] ResumeSearchDto dto)
            {
                var results = await _resumeService.Search(dto);
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    await _logService.Log(
                 action: "Search Resumes",
                   performedBy: userId.ToString(),
                        details: $"Performed search with filters: Name = " +
                        $"{dto.Name}, Email = {dto.Email}, Skill = {dto.Skill}"
                    );
                }
                return Ok(results);
            }
        [Authorize(Roles = "Admin,HR,Recruiter")]
        [HttpGet("download/{resumeId}")]
            public async Task<IActionResult> DownloadResume(Guid resumeId)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || 

                    !Guid.TryParse(userIdClaim, out var userId))

                    return Unauthorized("Invalid token");

                var resumeFile = await _resumeService.DownloadResume(resumeId, userId);

                
  

                return File(resumeFile, "application/pdf", "Resume.pdf");
            }




    }

}
