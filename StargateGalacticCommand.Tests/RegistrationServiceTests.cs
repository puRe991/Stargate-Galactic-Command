using System;
using System.Linq;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StargateGalacticCommand.Web.Controllers;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class RegistrationServiceTests
    {
        [Fact]
        public void CreateUserWithStartBase_AssignsFreeSettlementSectorAndStartingState()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite(connection).Options;
                using (var db = new GameDbContext(options))
                {
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()), useMigrations: false);
                    var serverId = db.GameServers.Single().Id;
                    var service = new RegistrationService(new EconomyService(), new PasswordService());
                    var user = service.CreateUserWithStartBase(db.Users, db.Factions, db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase), serverId, "sam", "sam@example.test", "Password123", 1);
                    db.Users.Add(user); db.SaveChanges();
                    var created = db.Users.Include(u => u.Bases).ThenInclude(b => b.Resources).Include(u => u.Bases).ThenInclude(b => b.BuildingLevels).Single(u => u.UserName == "sam");
                    var startBase = created.Bases.Single();
                    Assert.Equal(500, startBase.Resources.Naquadah);
                    Assert.Equal(1, startBase.BuildingLevels.CommandCenter);
                    Assert.Equal(0, startBase.BuildingLevels.NaquadahRefinery);
                    Assert.Equal(2, db.PlayerBases.Include(b => b.PlanetSector).Single().PlanetSector.Number);
                    Assert.Single(db.Reports.Where(r => r.UserId == created.Id));
                    Assert.False(created.IsNpc);
                    Assert.Null(created.LastSeenAtUtc);
                }
            }
        }
        [Fact]
        public void CreateUserWithStartBase_UsesNextPlanetWhenFirstPlanetIsFull()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite(connection).Options;
                using (var db = new GameDbContext(options))
                {
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()), useMigrations: false);
                    var serverId = db.GameServers.Single().Id;
                    var faction = db.Factions.Single(f => f.Id == 1);
                    var fullPlanet = db.Planets.Include(p => p.Sectors).Single(p => p.Name == "P3X-742");
                    int index = 1;
                    foreach (var sector in fullPlanet.Sectors.Where(s => s.IsSettlementSector).OrderBy(s => s.Number))
                    {
                        db.Users.Add(new User { ServerId = serverId, UserName = "taken" + index, Email = "taken" + index + "@example.test", PasswordHash = "h", PasswordSalt = "s", Faction = faction, ResearchLevels = new ResearchLevels(), Bases = { new PlayerBase { Name = "Basis " + index, Faction = faction, PlanetSector = sector, Resources = new ResourceStock(), BuildingLevels = new BuildingLevels(), Ships = new BaseShips() } } });
                        index++;
                    }
                    db.SaveChanges();

                    var service = new RegistrationService(new EconomyService(), new PasswordService());
                    var user = service.CreateUserWithStartBase(db.Users, db.Factions, db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase), serverId, "janet", "janet@example.test", "Password123", 1);
                    db.Users.Add(user); db.SaveChanges();

                    var createdBase = db.PlayerBases.Include(b => b.PlanetSector).ThenInclude(s => s.Planet).Single(b => b.Name == "janet Hauptbasis");
                    Assert.Equal("P4X-650", createdBase.PlanetSector.Planet.Name);
                    Assert.Equal(2, createdBase.PlanetSector.Number);
                }
            }
        }
        [Fact]
        public void CreateUserWithStartBase_AllowsSameUserNameOnDifferentServer()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite(connection).Options;
                using (var db = new GameDbContext(options))
                {
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()), useMigrations: false);
                    var firstServerId = db.GameServers.Single().Id;
                    var serverService = new GameServerService(db, new GateMissionService(new ResourceService()));
                    var secondServer = serverService.CreateServer("Beta", "Zweiter Server");

                    var service = new RegistrationService(new EconomyService(), new PasswordService());
                    var planets = db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase);
                    var first = service.CreateUserWithStartBase(db.Users, db.Factions, planets, firstServerId, "sam", "sam1@example.test", "Password123", 1);
                    db.Users.Add(first); db.SaveChanges();

                    var second = service.CreateUserWithStartBase(db.Users, db.Factions, planets, secondServer.Id, "sam", "sam2@example.test", "Password123", 1);
                    db.Users.Add(second); db.SaveChanges();

                    Assert.Equal(2, db.Users.Count(u => u.UserName == "sam"));
                    Assert.Throws<InvalidOperationException>(() => service.CreateUserWithStartBase(db.Users, db.Factions, planets, firstServerId, "sam", "sam3@example.test", "Password123", 1));
                }
            }
        }
        [Fact]
        public void Initialize_WithDefaultMigrationMode_CreatesSchemaViaMigrations()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite(connection).Options;
                using (var db = new GameDbContext(options))
                {
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()));

                    Assert.Equal(4, db.Factions.Count());
                    Assert.Equal(1, db.GameServers.Count());
                    Assert.Equal(3, db.Planets.Count());
                    Assert.True(db.GateAddresses.Any(a => a.Code == "P3X-742"));
                    Assert.True(db.GateAddresses.Count() >= DatabaseInitializer.GeneratedWorldCount);
                    Assert.Equal(db.GateAddresses.Count(), db.GateAddresses.Select(a => a.Code).Distinct().Count());
                }
            }
        }

        [Fact]
        public void Initialize_BaselinesExistingEnsureCreatedDatabaseInsteadOfCrashing()
        {
            var dbPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sgc-baseline-test-" + Guid.NewGuid() + ".db");
            var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite("Data Source=" + dbPath).Options;
            try
            {
                // Simulate a database created by the pre-migration EnsureCreated() fallback:
                // all tables exist, but there is no __EFMigrationsHistory table/row.
                using (var db = new GameDbContext(options))
                {
                    db.Database.EnsureCreated();
                }

                using (var db = new GameDbContext(options))
                {
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()));
                    Assert.Equal(4, db.Factions.Count());
                }

                // A second run must go through Migrate() against the now-baselined database
                // without trying to re-create any tables.
                using (var db = new GameDbContext(options))
                {
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()));
                    Assert.Equal(4, db.Factions.Count());
                }
            }
            finally
            {
                System.IO.File.Delete(dbPath);
            }
        }

        [Fact]
        public void Initialize_EnablesWriteAheadLoggingOnFileBasedDatabase()
        {
            var dbPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sgc-wal-test-" + Guid.NewGuid() + ".db");
            var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite("Data Source=" + dbPath).Options;
            try
            {
                using (var db = new GameDbContext(options))
                {
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()));

                    db.Database.OpenConnection();
                    using var command = db.Database.GetDbConnection().CreateCommand();
                    command.CommandText = "PRAGMA journal_mode;";
                    var mode = (string)command.ExecuteScalar();
                    db.Database.CloseConnection();
                    Assert.Equal("wal", mode, ignoreCase: true);
                }
            }
            finally
            {
                System.IO.File.Delete(dbPath);
                System.IO.File.Delete(dbPath + "-wal");
                System.IO.File.Delete(dbPath + "-shm");
            }
        }

        [Fact]
        public void Initialize_CalledTwice_DoesNotDuplicateGeneratedWorlds()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite(connection).Options;
                using (var db = new GameDbContext(options))
                {
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()), useMigrations: false);
                    int firstCount = db.GateAddresses.Count();
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()), useMigrations: false);
                    int secondCount = db.GateAddresses.Count();

                    Assert.Equal(firstCount, secondCount);
                    Assert.Equal(1, db.GameServers.Count());
                }
            }
        }

        [Fact]
        public void GameServerService_CreateServer_SeedsIndependentGalaxy()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite(connection).Options;
                using (var db = new GameDbContext(options))
                {
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()), useMigrations: false);
                    var firstServerId = db.GameServers.Single().Id;
                    var serverService = new GameServerService(db, new GateMissionService(new ResourceService()));
                    var secondServer = serverService.CreateServer("Beta", "Zweiter Server");

                    Assert.NotEqual(firstServerId, secondServer.Id);
                    Assert.Equal(ServerStatus.Online, secondServer.Status);
                    Assert.Equal(3, db.Planets.Count(p => p.ServerId == secondServer.Id));
                    Assert.True(db.GateAddresses.Count(a => a.ServerId == secondServer.Id) >= DatabaseInitializer.GeneratedWorldCount);
                    Assert.True(db.Planets.Any(p => p.ServerId == firstServerId && p.Name == "P3X-742"));
                    Assert.True(db.Planets.Any(p => p.ServerId == secondServer.Id && p.Name == "P3X-742"));
                }
            }
        }

        [Fact]
        public void FindLoginCandidate_ReturnsNormalUserOnlyAndExcludesNpcUser()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite(connection).Options;
                using (var db = new GameDbContext(options))
                {
                    db.Database.EnsureCreated();
                    var passwordService = new PasswordService();
                    passwordService.CreateHash("Password123", out var hash, out var salt);
                    db.Factions.Add(new Faction { Id = 1, Name = "Tau'ri", ShortName = "SGC" });
                    var server = new GameServer { Name = "Alpha", Slug = "alpha", Description = string.Empty, Status = ServerStatus.Online, CreatedAtUtc = DateTime.UtcNow, GalaxySeed = 1 };
                    db.GameServers.Add(server);
                    db.SaveChanges();
                    db.Users.AddRange(
                        new User { ServerId = server.Id, UserName = "sam", Email = "sam@example.test", PasswordHash = hash, PasswordSalt = salt, IsNpc = false, FactionId = 1, ResearchLevels = new ResearchLevels() },
                        new User { ServerId = server.Id, UserName = "npc-sam", Email = "npc@example.test", PasswordHash = string.Empty, PasswordSalt = string.Empty, IsNpc = true, FactionId = 1, ResearchLevels = new ResearchLevels() });
                    db.SaveChanges();

                    var normalByName = AccountController.FindLoginCandidate(db.Users, server.Id, " Sam ");
                    var normalByEmail = AccountController.FindLoginCandidate(db.Users, server.Id, "SAM@EXAMPLE.TEST");
                    var npcByName = AccountController.FindLoginCandidate(db.Users, server.Id, "npc-sam");
                    var npcByEmail = AccountController.FindLoginCandidate(db.Users, server.Id, "npc@example.test");

                    Assert.NotNull(normalByName);
                    Assert.Equal("sam", normalByName.UserName);
                    Assert.NotNull(normalByEmail);
                    Assert.Equal("sam", normalByEmail.UserName);
                    Assert.Null(npcByName);
                    Assert.Null(npcByEmail);
                    Assert.True(db.Users.Single(u => u.UserName == "npc-sam").IsNpc);
                    Assert.Empty(db.PlayerBases.Where(b => b.UserId == db.Users.Single(u => u.UserName == "npc-sam").Id));
                }
            }
        }

    }
}
