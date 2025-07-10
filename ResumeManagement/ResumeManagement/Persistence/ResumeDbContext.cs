using Microsoft.EntityFrameworkCore;
using ResumeManagement.Models;

namespace ResumeManagement.Persistence
{
    public class ResumeDbContext : DbContext
    {
        public ResumeDbContext(DbContextOptions<ResumeDbContext> options) : base(options) { }

        public DbSet<Log> Logs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Resume> Resumes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).HasMaxLength(50);
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.Property(u => u.PasswordHash).HasMaxLength(255);
                entity.Property(u => u.Role).HasMaxLength(20);
            });
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(bool))
                    {
                        property.SetColumnType("NUMBER(1)");
                    }
                }
            }
            modelBuilder.Entity<Resume>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).HasMaxLength(100);
                entity.Property(r => r.Email).HasMaxLength(100);
                entity.Property(r => r.Phone).HasMaxLength(20);
                entity.Property(r => r.Skills).HasColumnType("NCLOB");
                entity.Property(r => r.Experience).HasColumnType("NCLOB");
                entity.Property(r => r.Education).HasColumnType("NCLOB");
                entity.Property(r => r.PdfFile).HasColumnType("BLOB");
            });
            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("Logs", "C##RESUME_APP");  
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Action).HasMaxLength(100);
                entity.Property(l => l.PerformedBy).HasMaxLength(100);
                entity.Property(l => l.Details).HasColumnType("NCLOB");
            });

        }
    }
}