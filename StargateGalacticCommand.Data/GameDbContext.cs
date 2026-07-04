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
        public DbSet<GateAddress> GateAddresses { get; set; }
        public DbSet<KnownGateAddress> KnownGateAddresses { get; set; }
        public DbSet<MissionTeam> MissionTeams { get; set; }
        public DbSet<GateMission> GateMissions { get; set; }
        public DbSet<GateMissionReport> GateMissionReports { get; set; }
        public DbSet<SectorControl> SectorControls { get; set; }
        public DbSet<SectorClaim> SectorClaims { get; set; }
        public DbSet<LocalActionReport> LocalActionReports { get; set; }
        public DbSet<PlanetMarketOrder> PlanetMarketOrders { get; set; }
        public DbSet<PlanetMarketTransaction> PlanetMarketTransactions { get; set; }
        public DbSet<TradeTaxRule> TradeTaxRules { get; set; }
        public DbSet<TradeReport> TradeReports { get; set; }
        public DbSet<BaseShips> BaseShips { get; set; }
        public DbSet<ShipyardQueueItem> ShipyardQueueItems { get; set; }
        public DbSet<FleetMovement> FleetMovements { get; set; }
        public DbSet<FleetReport> FleetReports { get; set; }

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
            modelBuilder.Entity<PlanetSector>().Property(s => s.SectorType).HasConversion<int>();
            modelBuilder.Entity<SectorControl>().HasIndex(c => c.PlanetSectorId).IsUnique();
            modelBuilder.Entity<SectorControl>().HasOne(c => c.PlanetSector).WithOne(s => s.SectorControl).HasForeignKey<SectorControl>(c => c.PlanetSectorId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SectorControl>().HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SectorClaim>().HasIndex(c => new { c.PlanetSectorId, c.IsCompleted });
            modelBuilder.Entity<SectorClaim>().HasOne(c => c.PlanetSector).WithMany().HasForeignKey(c => c.PlanetSectorId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SectorClaim>().HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<LocalActionReport>().HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<LocalActionReport>().HasOne(r => r.PlanetSector).WithMany().HasForeignKey(r => r.PlanetSectorId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PlayerBase>().HasIndex(b => b.PlanetSectorId).IsUnique();
            modelBuilder.Entity<PlayerBase>().HasOne(b => b.Resources).WithOne().HasForeignKey<ResourceStock>(r => r.PlayerBaseId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PlayerBase>().HasOne(b => b.BuildingLevels).WithOne().HasForeignKey<BuildingLevels>(l => l.PlayerBaseId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PlayerBase>().HasOne(b => b.Ships).WithOne(s => s.PlayerBase).HasForeignKey<BaseShips>(s => s.PlayerBaseId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PlayerBase>().HasOne(b => b.PlanetSector).WithOne(s => s.PlayerBase).HasForeignKey<PlayerBase>(b => b.PlanetSectorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>().HasOne(u => u.ResearchLevels).WithOne(l => l.User).HasForeignKey<ResearchLevels>(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ResearchQueueItem>().HasOne(q => q.User).WithMany(u => u.ResearchQueue).HasForeignKey(q => q.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ResearchQueueItem>().Property(q => q.ResearchType).HasConversion<int>();
            modelBuilder.Entity<GateAddress>().Property(a => a.Code).IsRequired().HasMaxLength(20);
            modelBuilder.Entity<GateAddress>().HasIndex(a => a.Code).IsUnique();
            modelBuilder.Entity<GateAddress>().HasOne(a => a.Planet).WithOne().HasForeignKey<GateAddress>(a => a.PlanetId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<KnownGateAddress>().HasIndex(k => new { k.UserId, k.GateAddressId }).IsUnique();
            modelBuilder.Entity<KnownGateAddress>().HasOne(k => k.User).WithMany(u => u.KnownGateAddresses).HasForeignKey(k => k.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<KnownGateAddress>().HasOne(k => k.GateAddress).WithMany(a => a.KnownByUsers).HasForeignKey(k => k.GateAddressId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MissionTeam>().HasOne(t => t.User).WithMany(u => u.MissionTeams).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MissionTeam>().Property(t => t.Type).HasConversion<int>();
            modelBuilder.Entity<GateMission>().HasOne(m => m.User).WithMany(u => u.GateMissions).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<GateMission>().HasOne(m => m.MissionTeam).WithMany().HasForeignKey(m => m.MissionTeamId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GateMission>().Property(m => m.MissionType).HasConversion<int>();
            modelBuilder.Entity<GateMissionReport>().HasOne(r => r.User).WithMany(u => u.GateMissionReports).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<GateMissionReport>().Property(r => r.Outcome).HasConversion<int>();
            modelBuilder.Entity<PlanetMarketOrder>().HasIndex(o => new { o.PlanetId, o.CompletedAtUtc, o.CancelledAtUtc, o.ExpiresAtUtc });
            modelBuilder.Entity<PlanetMarketOrder>().Property(o => o.OfferedResource).HasConversion<int>();
            modelBuilder.Entity<PlanetMarketOrder>().Property(o => o.RequestedResource).HasConversion<int>();
            modelBuilder.Entity<PlanetMarketOrder>().HasOne(o => o.Planet).WithMany().HasForeignKey(o => o.PlanetId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PlanetMarketOrder>().HasOne(o => o.SellerUser).WithMany().HasForeignKey(o => o.SellerUserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PlanetMarketOrder>().HasOne(o => o.SellerBase).WithMany().HasForeignKey(o => o.SellerBaseId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PlanetMarketTransaction>().HasIndex(t => t.PlanetMarketOrderId).IsUnique();
            modelBuilder.Entity<PlanetMarketTransaction>().Property(t => t.OfferedResource).HasConversion<int>();
            modelBuilder.Entity<PlanetMarketTransaction>().Property(t => t.RequestedResource).HasConversion<int>();
            modelBuilder.Entity<PlanetMarketTransaction>().HasOne(t => t.PlanetMarketOrder).WithMany().HasForeignKey(t => t.PlanetMarketOrderId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PlanetMarketTransaction>().HasOne(t => t.Planet).WithMany().HasForeignKey(t => t.PlanetId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PlanetMarketTransaction>().HasOne(t => t.SellerUser).WithMany().HasForeignKey(t => t.SellerUserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PlanetMarketTransaction>().HasOne(t => t.BuyerUser).WithMany().HasForeignKey(t => t.BuyerUserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TradeReport>().HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TradeReport>().HasOne(r => r.PlanetMarketOrder).WithMany().HasForeignKey(r => r.PlanetMarketOrderId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<ShipyardQueueItem>().Property(q => q.ShipType).HasConversion<int>();
            modelBuilder.Entity<ShipyardQueueItem>().HasOne(q => q.PlayerBase).WithMany(b => b.ShipyardQueue).HasForeignKey(q => q.PlayerBaseId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FleetMovement>().Property(f => f.ShipType).HasConversion<int>();
            modelBuilder.Entity<FleetMovement>().Property(f => f.MissionType).HasConversion<int>();
            modelBuilder.Entity<FleetMovement>().Property(f => f.Status).HasConversion<int>();
            modelBuilder.Entity<FleetMovement>().HasOne(f => f.OriginBase).WithMany().HasForeignKey(f => f.OriginBaseId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FleetMovement>().HasOne(f => f.TargetBase).WithMany().HasForeignKey(f => f.TargetBaseId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FleetReport>().HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
