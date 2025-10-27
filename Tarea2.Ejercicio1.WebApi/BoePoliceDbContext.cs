using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
namespace Tarea2.Ejercicio1.WebApi
{

    public class BoePoliceDbContext : DbContext
    {
        public static readonly string DB_FILE = Path.Combine(AppContext.BaseDirectory,"Data", "PoliceCandidates.db");
        public DbSet<BoePoliceCandidate> PoliceCandidates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DB_FILE)!);
            options.UseSqlite($"Data Source={DB_FILE}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoePoliceCandidate>(entity =>
            {
                entity.ToTable(nameof(BoePoliceCandidate));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.Dni).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.LastName).IsRequired();
                entity.Property(e => e.FinalScore).HasPrecision(10, 8);
            });
        }
    }
}
