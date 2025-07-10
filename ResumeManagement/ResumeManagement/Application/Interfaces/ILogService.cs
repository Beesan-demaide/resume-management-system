namespace ResumeManagement.Application.Interfaces
{
    public interface ILogService
    {
        Task Log(string action, string performedBy, string details);
    }

}
