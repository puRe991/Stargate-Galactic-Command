using Microsoft.EntityFrameworkCore;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Faction> Factions { get; set; }
        public DbSet<Planet> Planets { get; set; }
        public DbSet<PlanetSector> PlanetSectors { get; set; }
        public DbSet<PlayerBase> PlayerBases { get; set; }
        public DbSet<ResourceStock> ResourceStocks { get; set; }
        public DbSet<BuildingLevels> BuildingLevels { get; set; }
        public DbSet<BuildQueueItem> BuildQueueItems { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ResearchLevels> ResearchLevels { get; set; }
        public DbSet<ResearchQueueItem> ResearchQueueItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Faction>().Property(f => f.Name).IsRequired().HasMaxLength(80);
            modelBuilder.Entity<Faction>().HasIndex(f => f.Name).IsUnique();
            modelBuilder.Entity<User>().Property(u => u.UserName).IsRequired().HasMaxLength(40);
            modelBuilder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(160);
            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Planet>().Property(p => p.Name).IsRequired().HasMaxLength(80);
            modelBuilder.Entity<Planet>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<PlanetSector>().HasIndex(s => new { s.PlanetId, s.Number }).IsUnique();
            modelBuilder.Entity<PlanetSector>().HasOne(s => s.Planet).WithMany(p => p.Sectors).HasForeignKey(s => s.PlanetId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PlayerBase>().HasIndex(b => b.PlanetSectorId).IsUnique();
            modelBuilder.Entity<PlayerBase>().HasOne(b => b.Resources).WithOne().HasForeignKey<ResourceStock>(r => r.PlayerBaseId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PlayerBase>().HasOne(b => b.BuildingLevels).WithOne().HasForeignKey<BuildingLevels>(l => l.PlayerBaseId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PlayerBase>().HasOne(b => b.PlanetSector).WithOne(s => s.PlayerBase).HasForeignKey<PlayerBase>(b => b.PlanetSectorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>().HasOne(u => u.ResearchLevels).WithOne(l => l.User).HasForeignKey<ResearchLevels>(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ResearchQueueItem>().HasOne(q => q.User).WithMany(u => u.ResearchQueue).HasForeignKey(q => q.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ResearchQueueItem>().Property(q => q.ResearchType).HasConversion<int>();
        }
    }
}
