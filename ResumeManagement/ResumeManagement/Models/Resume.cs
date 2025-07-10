
namespace ResumeManagement.Models
{ 
public class Resume
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Skills { get; set; }
    public string Experience { get; set; }
    public string Education { get; set; }
    public byte[] PdfFile { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
}

}