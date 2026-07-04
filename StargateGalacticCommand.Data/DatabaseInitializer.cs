using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(GameDbContext context)
        {
            if (context == null) return;
            context.Database.EnsureCreated();
            SeedFactions(context);
            SeedStartPlanet(context);
            EnsureResearchLevels(context);
            SeedGateAddresses(context);
            EnsureGateAccess(context);
            SeedTradeTaxRule(context);
            EnsureBaseShips(context);
            context.SaveChanges();
        }
        private static void SeedFactions(GameDbContext context)
        {
            if (context.Factions.Any()) return;
            context.Factions.AddRange(
                new Faction { Id = 1, Name = "Tau’ri / Stargate Command", ShortName = "SGC" },
                new Faction { Id = 2, Name = "Freie Jaffa", ShortName = "Jaffa" },
                new Faction { Id = 3, Name = "Tok’ra", ShortName = "Tok’ra" },
                new Faction { Id = 4, Name = "Lucian Alliance", ShortName = "Lucian" });
        }
        private static void EnsureResearchLevels(GameDbContext context)
        {
            foreach (var user in context.Users.Where(u => u.ResearchLevels == null).ToList())
            {
                context.ResearchLevels.Add(new ResearchLevels { UserId = user.Id });
            }
        }
        private static void SeedGateAddresses(GameDbContext context)
        {
            var startPlanet = context.Planets.SingleOrDefault(p => p.Name == "P3X-742");
            if (startPlanet != null && !context.GateAddresses.Any(a => a.Code == "P3X-742"))
                context.GateAddresses.Add(new GateAddress { Planet = startPlanet, Code = "P3X-742", WorldName = "P3X-742", Description = "Startplanet mit aktiver Stargate-Lichtung.", IsNeutralPve = false, RiskLevel = 1 });
            AddPve(context, "P4X-219", "verlassene Menschenkolonie", 3);
            AddPve(context, "P2X-885", "alte Goa’uld-Ruine", 5);
            AddPve(context, "P7X-331", "Triniumvorkommen", 4);
            AddPve(context, "P9C-117", "instabile Gate-Adresse", 8);
            AddPve(context, "P3R-636", "neutraler Handelskontakt", 2);
        }
        private static void AddPve(GameDbContext context, string code, string description, int risk)
        {
            if (!context.GateAddresses.Any(a => a.Code == code)) context.GateAddresses.Add(new GateAddress { Code = code, WorldName = code, Description = description, IsNeutralPve = true, RiskLevel = risk });
        }
        private static void EnsureGateAccess(GameDbContext context)
        {
            var service = new StargateGalacticCommand.Core.Services.GateMissionService(new StargateGalacticCommand.Core.Services.ResourceService());
            var start = context.GateAddresses.SingleOrDefault(a => a.Code == "P3X-742");
            if (start == null) return;
            foreach (var user in context.Users.ToList())
            {
                if (!context.KnownGateAddresses.Any(k => k.UserId == user.Id && k.GateAddressId == start.Id))
                    context.KnownGateAddresses.Add(new KnownGateAddress { UserId = user.Id, GateAddress = start, DiscoveredAtUtc = System.DateTime.UtcNow, DiscoveryMethod = "Startplanet" });
                if (!context.MissionTeams.Any(t => t.UserId == user.Id))
                {
                    context.Entry(user).Reference(u => u.Faction).Load();
                    context.MissionTeams.Add(service.CreateFactionTeam(user));
                }
            }
        }
        private static void SeedTradeTaxRule(GameDbContext context)
        {
            if (!context.TradeTaxRules.Any()) context.TradeTaxRules.Add(new TradeTaxRule { BaseFeeRate = 0.02, LucianAllianceReduction = 0.25, TradingPostReduction = 0.05, MaxIntelAmount = 25 });
        }
        private static void EnsureBaseShips(GameDbContext context)
        {
            var existing = context.BaseShips.Select(s => s.PlayerBaseId).ToList();
            foreach (var b in context.PlayerBases.Where(b => !existing.Contains(b.Id)).ToList()) context.BaseShips.Add(new BaseShips { PlayerBaseId = b.Id });
        }
        private static void SeedStartPlanet(GameDbContext context)
        {
            if (context.Planets.Any(p => p.Name == "P3X-742")) return;
            var planet = new Planet { Name = "P3X-742", Galaxy = "Milchstraße", Type = "Grenzwelt", StargateActive = true, Status = "geteilt" };
            string[] names = { "Stargate-Lichtung", "lokale Siedlung", "Siedlungssektor 3", "Siedlungssektor 4", "Siedlungssektor 5", "Siedlungssektor 6", "Triniumfeld", "alte Goa’uld-Ruine", "Naquadah-Vorkommen", "Orbitalkorridor" };
            SectorType[] types = { SectorType.StargateZone, SectorType.LocalSettlement, SectorType.SettlementSector, SectorType.SettlementSector, SectorType.SettlementSector, SectorType.SettlementSector, SectorType.TriniumField, SectorType.GoauldRuin, SectorType.NaquadahDeposit, SectorType.OrbitalCorridor };
            for (int i = 0; i < names.Length; i++) planet.Sectors.Add(new PlanetSector { Number = i + 1, Name = names[i], IsSettlementSector = types[i] == SectorType.LocalSettlement || types[i] == SectorType.SettlementSector, SectorType = types[i] });
            context.Planets.Add(planet);
        }
    }
}
