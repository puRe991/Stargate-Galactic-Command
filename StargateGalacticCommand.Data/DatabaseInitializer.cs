using System;
using System.Linq;
using Microsoft.Data.Sqlite;
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
            EnableWriteAheadLogging(context);
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
            if (!useMigrations)
            {
                // Only used by the in-memory/relational test fixtures, which have no migrations.
                context.Database.EnsureCreated();
                return;
            }

            // A database file created by the old EnsureCreated() fallback (before real
            // migrations existed) has all the tables but no __EFMigrationsHistory row.
            // Migrate() would then try to CREATE TABLE for tables that already exist and
            // crash. Baseline such a database instead of touching its schema/data: stamp
            // the current migrations as already applied, exactly like the EF Core docs
            // recommend for adopting migrations on an existing database.
            if (HasPreMigrationSchema(context)) BaselineExistingDatabase(context);

            try
            {
                context.Database.Migrate();
            }
            catch (SqliteException ex) when (ex.Message.Contains("already exists"))
            {
                // The proactive check above only baselines when the *entire* current
                // schema is already present. A database that has some but not all
                // tables (e.g. left behind by a run that was killed mid-migration)
                // fails Migrate() here instead. There is no safe way to guess which
                // migrations already ran against a schema like that, so move the file
                // aside and let Migrate() build a clean one; the original is kept next
                // to it in case anything in it needs to be recovered by hand.
                if (!QuarantineAndReset(context)) throw;
                context.Database.Migrate();
            }
        }

        // True when every table the current EF model expects is already present, which
        // means this file was fully created outside of migrations (old EnsureCreated()
        // fallback or a restored backup) rather than genuinely mid-migration.
        private static bool HasPreMigrationSchema(GameDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            // For SQLite ":memory:" connections (used by tests), closing a connection we
            // did not open ourselves would destroy the in-memory database and its content.
            bool wasOpen = connection.State == System.Data.ConnectionState.Open;
            if (!wasOpen) connection.Open();
            try
            {
                if (TableExists(connection, "__EFMigrationsHistory")) return false;
                var tableNames = context.Model.GetEntityTypes()
                    .Select(e => e.GetTableName())
                    .Where(name => name != null)
                    .Distinct();
                return tableNames.All(name => TableExists(connection, name!));
            }
            finally
            {
                if (!wasOpen) connection.Close();
            }
        }

        // Renames a file-based SQLite database out of the way so Migrate() can create a
        // fresh one from scratch. Returns false (nothing done) for in-memory connections,
        // where there is no file to move and this situation should not occur outside tests.
        private static bool QuarantineAndReset(GameDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            var dataSource = connection.DataSource;
            if (string.IsNullOrEmpty(dataSource) || dataSource == ":memory:") return false;
            if (!System.IO.File.Exists(dataSource)) return false;

            connection.Close();
            // Microsoft.Data.Sqlite pools native sqlite3 handles per connection string by
            // default, so without this the next Open() can hand back a handle still tied
            // to the file we just moved instead of opening a fresh one at the same path.
            if (connection is SqliteConnection sqliteConnection) SqliteConnection.ClearPool(sqliteConnection);

            var quarantinePath = dataSource + ".broken-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            System.IO.File.Move(dataSource, quarantinePath);
            foreach (var suffix in new[] { "-wal", "-shm", "-journal" })
            {
                var sidecar = dataSource + suffix;
                if (System.IO.File.Exists(sidecar)) System.IO.File.Move(sidecar, quarantinePath + suffix);
            }
            return true;
        }

        // SQLite's default rollback-journal mode locks the whole database file for the
        // duration of a write, so readers block on writers (and vice versa). WAL mode lets
        // one writer and many readers proceed concurrently, which matters once this
        // single-file database serves more than one request at a time. WAL is a persistent,
        // one-time-per-file setting stored in the database header, so re-applying it on
        // every startup is a cheap no-op once set. Has no effect on ":memory:" databases
        // (used by tests), which SQLite always keeps in a private in-memory journal mode.
        private static void EnableWriteAheadLogging(GameDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            bool wasOpen = connection.State == System.Data.ConnectionState.Open;
            if (!wasOpen) connection.Open();
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA journal_mode=WAL;";
                command.ExecuteNonQuery();
            }
            finally
            {
                if (!wasOpen) connection.Close();
            }
        }

        private static bool TableExists(System.Data.Common.DbConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=$name";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "$name";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);
            return command.ExecuteScalar() != null;
        }

        private static void BaselineExistingDatabase(GameDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            bool wasOpen = connection.State == System.Data.ConnectionState.Open;
            if (!wasOpen) connection.Open();
            try
            {
                using (var createHistoryTable = connection.CreateCommand())
                {
                    createHistoryTable.CommandText =
                        "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (" +
                        "\"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, " +
                        "\"ProductVersion\" TEXT NOT NULL)";
                    createHistoryTable.ExecuteNonQuery();
                }

                foreach (var migrationId in context.Database.GetMigrations())
                {
                    using var insert = connection.CreateCommand();
                    insert.CommandText = "INSERT OR IGNORE INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ($id, $version)";
                    var idParameter = insert.CreateParameter();
                    idParameter.ParameterName = "$id";
                    idParameter.Value = migrationId;
                    insert.Parameters.Add(idParameter);
                    var versionParameter = insert.CreateParameter();
                    versionParameter.ParameterName = "$version";
                    versionParameter.Value = "8.0.28";
                    insert.Parameters.Add(versionParameter);
                    insert.ExecuteNonQuery();
                }
            }
            finally
            {
                if (!wasOpen) connection.Close();
            }
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
