using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Data
{
    public static class DatabaseInitializer
    {
        public const int GeneratedWorldCount = 320;
        public const int GalaxySeed = 20260706;
        public const string DefaultServerName = "Alpha";

        public static void Initialize(GameDbContext context, GateMissionService gateMissionService, bool useMigrations = true)
        {
            if (context == null) return;
            EnsureDatabaseCreated(context, useMigrations);
            SeedFactions(context);
            SeedTradeTaxRule(context);
            context.SaveChanges();

            var serverService = new GameServerService(context, gateMissionService);
            if (!context.GameServers.Any())
            {
                serverService.CreateServer(DefaultServerName, "Der erste Sektor der Galaktischen Allianz.");
            }
            context.SaveChanges();

            EnsureResearchLevels(context);
            foreach (var server in context.GameServers.ToList())
            {
                EnsureGateAccess(context, gateMissionService, server.Id);
            }
            EnsureBaseShips(context);
            EnsureProtectionStatuses(context);
            context.SaveChanges();
        }
        private static void EnsureDatabaseCreated(GameDbContext context, bool useMigrations)
        {
            // Migrate() does not create the model tables when the assembly has no
            // migrations. This prototype currently has no migration files, so fall
            // back to EnsureCreated() to avoid a startup crash while seeding.
            if (useMigrations && context.Database.GetMigrations().Any())
            {
                context.Database.Migrate();
                return;
            }

            context.Database.EnsureCreated();
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

        // Seeds one server's independent galaxy (start planets, gate addresses,
        // procedurally generated worlds). Called both for the default server at
        // app startup and whenever the admin creates a new server at runtime.
        internal static void SeedGalaxyForServer(GameDbContext context, int serverId, int galaxySeed)
        {
            SeedStartPlanet(context, serverId);
            context.SaveChanges();
            SeedGateAddresses(context, serverId);
            SeedGeneratedWorlds(context, serverId, galaxySeed);
        }

        private static void SeedGateAddresses(GameDbContext context, int serverId)
        {
            AddPlanetAddress(context, serverId, "P3X-742", "Startplanet mit aktiver Stargate-Lichtung.", 1);
            AddPlanetAddress(context, serverId, "P4X-650", "Bewohnbarer Waldmond mit aktiver Stargate-Zone.", 2);
            AddPlanetAddress(context, serverId, "P9G-844", "Wüstenkolonie mit stabiler Gate-Düne.", 2);
            AddPve(context, serverId, "P4X-219", "verlassene Menschenkolonie", 3);
            AddPve(context, serverId, "P2X-885", "alte Goa’uld-Ruine", 5);
            AddPve(context, serverId, "P7X-331", "Triniumvorkommen", 4);
            AddPve(context, serverId, "P9C-117", "instabile Gate-Adresse", 8);
            AddPve(context, serverId, "P3R-636", "neutraler Handelskontakt", 2);
        }
        private static void AddPlanetAddress(GameDbContext context, int serverId, string code, string description, int risk)
        {
            var planet = context.Planets.SingleOrDefault(p => p.ServerId == serverId && p.Name == code);
            if (planet != null && !context.GateAddresses.Any(a => a.ServerId == serverId && a.Code == code))
                context.GateAddresses.Add(new GateAddress { ServerId = serverId, Planet = planet, Code = code, WorldName = code, Description = description, IsNeutralPve = false, RiskLevel = risk });
        }
        private static void AddPve(GameDbContext context, int serverId, string code, string description, int risk)
        {
            if (!context.GateAddresses.Any(a => a.ServerId == serverId && a.Code == code))
                context.GateAddresses.Add(new GateAddress { ServerId = serverId, Code = code, WorldName = code, Description = description, IsNeutralPve = true, RiskLevel = risk });
        }
        private static void SeedGeneratedWorlds(GameDbContext context, int serverId, int galaxySeed)
        {
            // SeedGateAddresses() above only Add()s to the change tracker; a plain
            // DbSet query would miss those pending rows, so pull codes from both the
            // database and the not-yet-saved Local view to avoid duplicate codes.
            var existingCodes = context.GateAddresses.Where(a => a.ServerId == serverId).Select(a => a.Code).ToList()
                .Concat(context.GateAddresses.Local.Where(a => a.ServerId == serverId).Select(a => a.Code))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (existingCodes.Count >= GeneratedWorldCount) return;

            var generator = new GalaxyGeneratorService();
            var worlds = generator.GenerateWorlds(GeneratedWorldCount - existingCodes.Count, existingCodes, galaxySeed);
            foreach (var world in worlds) world.ServerId = serverId;
            context.GateAddresses.AddRange(worlds);
        }

        private static void EnsureGateAccess(GameDbContext context, GateMissionService service, int serverId)
        {
            var start = context.GateAddresses.SingleOrDefault(a => a.ServerId == serverId && a.Code == "P3X-742");
            if (start == null) return;
            foreach (var user in context.Users.Where(u => u.ServerId == serverId).ToList())
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
        private static void EnsureProtectionStatuses(GameDbContext context)
        {
            var existing = context.PlayerProtectionStatuses.Select(p => p.UserId).ToList();
            foreach (var user in context.Users.Where(u => !existing.Contains(u.Id)).ToList())
            {
                context.PlayerProtectionStatuses.Add(new PlayerProtectionStatus { UserId = user.Id, ProtectedUntilUtc = user.CreatedAtUtc.AddDays(3), Score = 0 });
            }
        }
        private static void SeedStartPlanet(GameDbContext context, int serverId)
        {
            AddSeedPlanet(context, serverId, "P3X-742", "Grenzwelt", "geteilt", new[] { "Stargate-Lichtung", "lokale Siedlung", "Siedlungssektor 3", "Siedlungssektor 4", "Siedlungssektor 5", "Siedlungssektor 6", "Triniumfeld", "alte Goa’uld-Ruine", "Naquadah-Vorkommen", "Orbitalkorridor" });
            AddSeedPlanet(context, serverId, "P4X-650", "Waldmond", "umkämpft", new[] { "Stargate-Ring", "Flusssiedlung", "Siedlungsplateau 3", "Siedlungsplateau 4", "Siedlungsplateau 5", "Siedlungsplateau 6", "Triniumader", "verlassener Tempel", "Naquadah-Senke", "Handelspfad" });
            AddSeedPlanet(context, serverId, "P9G-844", "Wüstenkolonie", "neutral", new[] { "Gate-Düne", "Oasenstadt", "Siedlungskamm 3", "Siedlungskamm 4", "Siedlungskamm 5", "Siedlungskamm 6", "Triniumbruch", "Goa’uld-Ausgrabung", "Naquadah-Schlucht", "Karawanenposten" });
        }

        private static void AddSeedPlanet(GameDbContext context, int serverId, string name, string type, string status, string[] names)
        {
            if (context.Planets.Any(p => p.ServerId == serverId && p.Name == name)) return;
            var planet = new Planet { ServerId = serverId, Name = name, Galaxy = "Milchstraße", Type = type, StargateActive = true, Status = status };
            SectorType[] types = { SectorType.StargateZone, SectorType.LocalSettlement, SectorType.SettlementSector, SectorType.SettlementSector, SectorType.SettlementSector, SectorType.SettlementSector, SectorType.TriniumField, SectorType.GoauldRuin, SectorType.NaquadahDeposit, SectorType.TradingPost };
            for (int i = 0; i < names.Length; i++) planet.Sectors.Add(new PlanetSector { Number = i + 1, Name = names[i], IsSettlementSector = types[i] == SectorType.LocalSettlement || types[i] == SectorType.SettlementSector, SectorType = types[i] });
            context.Planets.Add(planet);
        }
    }
}
