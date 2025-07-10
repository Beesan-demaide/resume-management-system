namespace ResumeManagement.Application.DTOs
{
    public class ResumeSearchDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Skill { get; set; }
        public string? Education { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
