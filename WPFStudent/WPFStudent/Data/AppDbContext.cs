using Microsoft.EntityFrameworkCore;
using WPFStudent.Model;

namespace WPFStudent.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }

        private readonly string _dbPath;
        public AppDbContext()
        {
            // store DB next to exe
            _dbPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "students.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={_dbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.StudentId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
