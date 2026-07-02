using Microsoft.EntityFrameworkCore;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        public DbSet<Planet> Planets { get; set; }
        public DbSet<BaseSector> BaseSectors { get; set; }
        public DbSet<ResourceStock> ResourceStocks { get; set; }
        public DbSet<Building> Buildings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Planet>().Property(p => p.Name).IsRequired().HasMaxLength(80);
            modelBuilder.Entity<Planet>().Property(p => p.GateAddress).HasMaxLength(32);
            modelBuilder.Entity<Planet>().HasIndex(p => p.Name).IsUnique();

            modelBuilder.Entity<BaseSector>().Property(b => b.Name).IsRequired().HasMaxLength(80);
            modelBuilder.Entity<BaseSector>().HasOne(b => b.Planet).WithMany(p => p.BaseSectors).HasForeignKey(b => b.PlanetId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BaseSector>().HasOne(b => b.Resources).WithOne().HasForeignKey<ResourceStock>(r => r.Id).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Building>().HasOne(b => b.BaseSector).WithMany(s => s.Buildings).HasForeignKey(b => b.BaseSectorId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Building>().HasIndex(b => new { b.BaseSectorId, b.Type }).IsUnique();

            modelBuilder.Entity<Planet>().HasData(
                new { Id = 1, Name = "P3X-984", GateAddress = "12-18-27-03-09-31", IsCanonicalRestricted = false },
                new { Id = 2, Name = "Velona Outlands", GateAddress = "04-22-16-08-30-19", IsCanonicalRestricted = false },
                new { Id = 3, Name = "Tegalus Frontier", GateAddress = "17-05-28-11-02-24", IsCanonicalRestricted = false },
                new { Id = 4, Name = "Erde", GateAddress = (string)null, IsCanonicalRestricted = true },
                new { Id = 5, Name = "Atlantis", GateAddress = (string)null, IsCanonicalRestricted = true },
                new { Id = 6, Name = "Dakara", GateAddress = (string)null, IsCanonicalRestricted = true });

            modelBuilder.Entity<ResourceStock>().HasData(new { Id = 1, Naquadah = 500, Trinium = 300, Deuterium = 150, Supplies = 700 });
            modelBuilder.Entity<BaseSector>().HasData(new { Id = 1, Name = "Außenposten Phoenix", Faction = Faction.TauriSgc, PlanetId = 1 });
            modelBuilder.Entity<Building>().HasData(
                new { Id = 1, BaseSectorId = 1, Type = BuildingType.CommandBunker, Level = 1 },
                new { Id = 2, BaseSectorId = 1, Type = BuildingType.NaquadahMine, Level = 1 },
                new { Id = 3, BaseSectorId = 1, Type = BuildingType.SupplyDepot, Level = 1 });
        }
    }
}
