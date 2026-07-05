using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(GameDbContext context, GateMissionService gateMissionService, bool useMigrations = true)
        {
            if (context == null) return;
            EnsureDatabaseCreated(context, useMigrations);
            SeedFactions(context);
            SeedStartPlanet(context);
            context.SaveChanges();
            EnsureResearchLevels(context);
            SeedGateAddresses(context);
            EnsureGateAccess(context, gateMissionService);
            SeedTradeTaxRule(context);
            EnsureBaseShips(context);
            EnsureProtectionStatuses(context);
            context.SaveChanges();
        }
        private static void EnsureDatabaseCreated(GameDbContext context, bool useMigrations)
        {
            if (useMigrations && context.Database.GetMigrations().Any())
            {
                context.Database.Migrate();
                return;
            }

            // EnsureCreated() is intentionally used while this prototype has no
            // migrations. If an older partial SQLite database already exists,
            // EnsureCreated() leaves it unchanged, so verify the schema before
            // seeding and rebuild the local file with a backup when tables are
            // missing.
            context.Database.EnsureCreated();
            if (!HasAllModelTables(context))
            {
                BackupSqliteDatabase(context);
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }

        private static bool HasAllModelTables(GameDbContext context)
        {
            var missingTables = GetModelTableNames(context).Where(table => !TableExists(context, table)).ToList();
            return missingTables.Count == 0;
        }

        private static IEnumerable<string> GetModelTableNames(GameDbContext context)
        {
            return context.Model.GetEntityTypes()
                .Select(entityType => entityType.GetTableName())
                .Where(tableName => !string.IsNullOrWhiteSpace(tableName))
                .Select(tableName => tableName!)
                .Distinct();
        }

        private static bool TableExists(GameDbContext context, string tableName)
        {
            var connection = context.Database.GetDbConnection();
            var shouldClose = connection.State == System.Data.ConnectionState.Closed;

            if (shouldClose) connection.Open();
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $tableName LIMIT 1";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "$tableName";
                    parameter.Value = tableName;
                    command.Parameters.Add(parameter);
                    return command.ExecuteScalar() != null;
                }
            }
            finally
            {
                if (shouldClose) connection.Close();
            }
        }

        private static void BackupSqliteDatabase(GameDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            if (connection is not SqliteConnection || string.IsNullOrWhiteSpace(connection.DataSource) || connection.DataSource == ":memory:")
            {
                return;
            }

            var databasePath = Path.GetFullPath(connection.DataSource);
            if (!File.Exists(databasePath))
            {
                return;
            }

            var backupPath = databasePath + ".backup-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            File.Copy(databasePath, backupPath, overwrite: false);
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
            AddPlanetAddress(context, "P3X-742", "Startplanet mit aktiver Stargate-Lichtung.", 1);
            AddPlanetAddress(context, "P4X-650", "Bewohnbarer Waldmond mit aktiver Stargate-Zone.", 2);
            AddPlanetAddress(context, "P9G-844", "Wüstenkolonie mit stabiler Gate-Düne.", 2);
            AddPve(context, "P4X-219", "verlassene Menschenkolonie", 3);
            AddPve(context, "P2X-885", "alte Goa’uld-Ruine", 5);
            AddPve(context, "P7X-331", "Triniumvorkommen", 4);
            AddPve(context, "P9C-117", "instabile Gate-Adresse", 8);
            AddPve(context, "P3R-636", "neutraler Handelskontakt", 2);
        }
        private static void AddPlanetAddress(GameDbContext context, string code, string description, int risk)
        {
            var planet = context.Planets.SingleOrDefault(p => p.Name == code);
            if (planet != null && !context.GateAddresses.Any(a => a.Code == code)) context.GateAddresses.Add(new GateAddress { Planet = planet, Code = code, WorldName = code, Description = description, IsNeutralPve = false, RiskLevel = risk });
        }
        private static void AddPve(GameDbContext context, string code, string description, int risk)
        {
            if (!context.GateAddresses.Any(a => a.Code == code)) context.GateAddresses.Add(new GateAddress { Code = code, WorldName = code, Description = description, IsNeutralPve = true, RiskLevel = risk });
        }
        private static void EnsureGateAccess(GameDbContext context, GateMissionService service)
        {
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
        private static void EnsureProtectionStatuses(GameDbContext context)
        {
            var existing = context.PlayerProtectionStatuses.Select(p => p.UserId).ToList();
            foreach (var user in context.Users.Where(u => !existing.Contains(u.Id)).ToList())
            {
                context.PlayerProtectionStatuses.Add(new PlayerProtectionStatus { UserId = user.Id, ProtectedUntilUtc = user.CreatedAtUtc.AddDays(3), Score = 0 });
            }
        }
        private static void SeedStartPlanet(GameDbContext context)
        {
            AddSeedPlanet(context, "P3X-742", "Grenzwelt", "geteilt", new[] { "Stargate-Lichtung", "lokale Siedlung", "Siedlungssektor 3", "Siedlungssektor 4", "Siedlungssektor 5", "Siedlungssektor 6", "Triniumfeld", "alte Goa’uld-Ruine", "Naquadah-Vorkommen", "Orbitalkorridor" });
            AddSeedPlanet(context, "P4X-650", "Waldmond", "umkämpft", new[] { "Stargate-Ring", "Flusssiedlung", "Siedlungsplateau 3", "Siedlungsplateau 4", "Siedlungsplateau 5", "Siedlungsplateau 6", "Triniumader", "verlassener Tempel", "Naquadah-Senke", "Handelspfad" });
            AddSeedPlanet(context, "P9G-844", "Wüstenkolonie", "neutral", new[] { "Gate-Düne", "Oasenstadt", "Siedlungskamm 3", "Siedlungskamm 4", "Siedlungskamm 5", "Siedlungskamm 6", "Triniumbruch", "Goa’uld-Ausgrabung", "Naquadah-Schlucht", "Karawanenposten" });
        }

        private static void AddSeedPlanet(GameDbContext context, string name, string type, string status, string[] names)
        {
            if (context.Planets.Any(p => p.Name == name)) return;
            var planet = new Planet { Name = name, Galaxy = "Milchstraße", Type = type, StargateActive = true, Status = status };
            SectorType[] types = { SectorType.StargateZone, SectorType.LocalSettlement, SectorType.SettlementSector, SectorType.SettlementSector, SectorType.SettlementSector, SectorType.SettlementSector, SectorType.TriniumField, SectorType.GoauldRuin, SectorType.NaquadahDeposit, SectorType.TradingPost };
            for (int i = 0; i < names.Length; i++) planet.Sectors.Add(new PlanetSector { Number = i + 1, Name = names[i], IsSettlementSector = types[i] == SectorType.LocalSettlement || types[i] == SectorType.SettlementSector, SectorType = types[i] });
            context.Planets.Add(planet);
        }
    }
}
