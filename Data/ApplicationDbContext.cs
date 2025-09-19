using FootballTeamManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;

namespace FootballTeamManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ensure jersey numbers are unique
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.JerseyNumber)
                .IsUnique();

            modelBuilder.Entity<Player>()
                .Property(p => p.Position)
                .HasConversion<string>();
        }
    }
}
