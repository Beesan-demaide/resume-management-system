using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ResumeManagement.Models;
using ResumeManagement.Persistence;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Commons.Actions.Contexts;
using ResumeManagement.Application.DTOs;
using ResumeManagement.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;

namespace ResumeManagement.Application.Services
{
    public class ResumeService: IResumeService
    {
        private readonly ResumeDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private readonly ILogService _logService;
        private readonly ILogger<ResumeService> _logger;

        public ResumeService(ResumeDbContext dbContext, IDistributedCache cache,ILogService logService, ILogger<ResumeService> logger)
        {
            _dbContext = dbContext;
            _logService = logService;
            _cache = cache;
            _logger = logger;

        }

        public async Task<Resume> UploadResumeAsync(IFormFile file, Guid userId)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var extractedText = ExtractTextFromPdf(fileBytes);

            var name = ExtractName(extractedText);
            var email = ExtractEmail(extractedText);
            var phone = ExtractPhone(extractedText);
            var skills = ExtractSkills(extractedText);
            var experience = ExtractExperience(extractedText);
            var education = ExtractEducation(extractedText);
/*
            var isDuplicate = await _dbContext.Resumes
         .AnyAsync(r => r.Email != null && r.Name != null
                        && r.UserId != Guid.Empty
                        && r.Email.ToLower() == email.ToLower()
                        && r.Name.ToLower() == name.ToLower()
                        && r.UserId == userId);





            if (isDuplicate)
                throw new Exception("This resume has already been uploaded with this email");*/

            var resume = new Resume
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                Email = email,
                Phone = phone,
                Skills = skills,
                Experience = experience,
                Education = education,
                PdfFile = fileBytes,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Resumes.AddAsync(resume);
            await _dbContext.SaveChangesAsync();

            await _logService.Log(
                action: "Upload Resume",
                performedBy: userId.ToString(),
                details: $"Uploaded resume for: {name}, Email: {email}"
            );

            return resume;
        }

        public string ExtractTextFromPdf(byte[] fileBytes)
        {
            using var stream = new MemoryStream(fileBytes);
            using var reader = new PdfReader(stream);
            using var pdfDoc = new PdfDocument(reader);

            var strategy = new SimpleTextExtractionStrategy();
            var sb = new StringBuilder();

            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                var pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                sb.Append(pageContent);
            }

            return sb.ToString();
        }
     
            public string ExtractEmail(string text)
            {
                var emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
                var match = Regex.Match(text, emailPattern);
                return match.Success ? match.Value : string.Empty;
            }

            public string ExtractPhone(string text)
            {
                var phonePattern = @"\+?\d[\d\s\-()]{7,}";
                var match = Regex.Match(text, phonePattern);
                return match.Success ? match.Value : string.Empty;
            }

        public string ExtractName(string text)
        {
            var lines = text.Split('\n')
                            .Select(line => line.Trim())
                            .Where(line => line.Length > 1)
                            .ToList();

            int emailIndex = lines.FindIndex(line => line.Contains("@"));
            if (emailIndex >= 0 && emailIndex + 1 < lines.Count)
            {
                var nextLine = lines[emailIndex + 1];
                if (!nextLine.Contains("@") && !Regex.IsMatch(nextLine, @"\d"))
                {
                    return nextLine;
                }
            }
            foreach (var line in lines)
            {
                if (line.Length > 5 && !line.Contains("@") && !Regex.IsMatch(line, @"\d"))
                    return line;
            }

            return string.Empty;
        }

        public string ExtractExperience(string text)
        {
            var pattern = @"(?i)(Experience|Work Experience|Professional Experience)\s*:?(\n|\r\n)?([\s\S]*?)(?=\n[A-Z][a-z]*\s?:|\Z)";
            var match = Regex.Match(text, pattern);
            return match.Success ? match.Groups[3].Value.Trim() : string.Empty;
        }
        public string ExtractEducation(string text)
        {
            var pattern = @"(?i)(Education|Academic Background|Qualifications)\s*:?(\n|\r\n)?([\s\S]*?)(?=\n[A-Z][a-z]*\s?:|\Z)";
            var match = Regex.Match(text, pattern);
            return match.Success ? match.Groups[3].Value.Trim() : string.Empty;
        }
        public string ExtractSkills(string text)
        {
            var pattern = @"(?i)(Skills|Technical Skills|Key Skills)\s*:?\s*(.*)";
            var match = Regex.Match(text, pattern);
            return match.Success ? match.Groups[2].Value.Trim() : string.Empty;
        }

        public async Task<List<Resume>> Search(ResumeSearchDto dto)
        {
            var cacheKey = $"resume_search:{dto.Name}:{dto.Email}:{dto.Phone}:{dto.Skill}:{dto.Education}:{dto.PageNumber}:{dto.PageSize}";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<Resume>>(cachedData)!;
            }

            var query = _dbContext.Resumes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Name))
                query = query.Where(r => r.Name.ToLower().Contains(dto.Name.ToLower()));

            if (!string.IsNullOrWhiteSpace(dto.Email))
                query = query.Where(r => r.Email.ToLower().Contains(dto.Email.ToLower()));

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                query = query.Where(r => r.Phone.Contains(dto.Phone));

            if (!string.IsNullOrWhiteSpace(dto.Skill))
                query = query.Where(r => r.Skills.ToLower().Contains(dto.Skill.ToLower()));

            if (!string.IsNullOrWhiteSpace(dto.Education))
                query = query.Where(r => r.Education.ToLower().Contains(dto.Education.ToLower()));

            var result = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((dto.PageNumber - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            var serialized = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync(cacheKey, serialized, options);
             await _logService.Log(
                        action: "Search Resumes",
                        performedBy: "System", 
                        details: $"Search by: Name={dto.Name}, Email={dto.Email}," +
                        $" Phone={dto.Phone}, Skill={dto.Skill}, Education={dto.Education}"
                                                                                           );

            return result;
        }

        public async Task<byte[]> DownloadResume(Guid resumeId, Guid userId)
        {
            var resume = await _dbContext.Resumes
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId);

            if (resume == null || resume.PdfFile == null || resume.PdfFile.Length == 0)
            {
                Console.WriteLine($"No PDF data for Resume ID: {resumeId}");
                throw new FileNotFoundException("Resume not found");
            }

            await _logService.Log(action: "Download Resume", performedBy: userId.ToString(),
                details: $"Downloaded resume with ID: {resumeId}");

            return resume.PdfFile;
        }
        }

    }
