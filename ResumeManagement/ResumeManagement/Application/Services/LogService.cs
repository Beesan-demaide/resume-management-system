using ResumeManagement.Application.Interfaces;
using ResumeManagement.Models;
using ResumeManagement.Persistence;

namespace ResumeManagement.Application.Services
{
    public class LogService : ILogService
    {
        private readonly ResumeDbContext _dbContext;

        public LogService(ResumeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Log(string action, string performedBy, string details)
        {
            var log = new Log
            {
                Id = Guid.NewGuid(),
                Action = action,
                PerformedBy = performedBy,
                Timestamp = DateTime.UtcNow,
                Details = details
            };

            await _dbContext.Logs.AddAsync(log);
            await _dbContext.SaveChangesAsync();
        }
    }
}
