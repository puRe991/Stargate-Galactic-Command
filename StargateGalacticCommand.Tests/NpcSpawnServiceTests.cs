using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class NpcSpawnServiceTests
    {
        [Fact]
        public void EnsureNpcPresence_DoesNothing_WhenNoHumanHasSettledOnThePlanetYet()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite(connection).Options;
                using (var db = new GameDbContext(options))
                {
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()), useMigrations: false);
                    var planet = db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase).Single(p => p.Name == "P3X-742");
                    var service = new NpcSpawnService(db, new EconomyService());

                    int created = service.EnsureNpcPresence(planet, DateTime.UtcNow);

                    Assert.Equal(0, created);
                    Assert.Empty(db.Users.Where(u => u.IsNpc));
                }
            }
        }

        [Fact]
        public void EnsureNpcPresence_DoesNothing_WhenAllSettlementSectorsAreTaken()
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
                    var planet = db.Planets.Include(p => p.Sectors).Single(p => p.Name == "P3X-742");
                    int index = 1;
                    foreach (var sector in planet.Sectors.Where(s => s.IsSettlementSector).OrderBy(s => s.Number))
                    {
                        db.Users.Add(new User { ServerId = serverId, UserName = "human" + index, Email = "human" + index + "@example.test", PasswordHash = "h", PasswordSalt = "s", Faction = faction, ResearchLevels = new ResearchLevels(), Bases = { new PlayerBase { Name = "Basis " + index, Faction = faction, PlanetSector = sector, Resources = new ResourceStock(), BuildingLevels = new BuildingLevels(), Ships = new BaseShips() } } });
                        index++;
                    }
                    db.SaveChanges();

                    var reloadedPlanet = db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase).Single(p => p.Id == planet.Id);
                    var service = new NpcSpawnService(db, new EconomyService());

                    int created = service.EnsureNpcPresence(reloadedPlanet, DateTime.UtcNow);

                    Assert.Equal(0, created);
                    Assert.Empty(db.Users.Where(u => u.IsNpc));
                }
            }
        }

        [Fact]
        public void EnsureNpcPresence_FillsAnEmptySettlementSector_WhenAHumanAlreadySettledThere()
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
                    var planet = db.Planets.Include(p => p.Sectors).Single(p => p.Name == "P3X-742");
                    var firstSettlementSector = planet.Sectors.Where(s => s.IsSettlementSector).OrderBy(s => s.Number).First();
                    db.Users.Add(new User { ServerId = serverId, UserName = "human1", Email = "human1@example.test", PasswordHash = "h", PasswordSalt = "s", Faction = faction, ResearchLevels = new ResearchLevels(), Bases = { new PlayerBase { Name = "Basis 1", Faction = faction, PlanetSector = firstSettlementSector, Resources = new ResourceStock(), BuildingLevels = new BuildingLevels(), Ships = new BaseShips() } } });
                    db.SaveChanges();

                    var reloadedPlanet = db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase).Single(p => p.Id == planet.Id);
                    var service = new NpcSpawnService(db, new EconomyService());

                    int created = service.EnsureNpcPresence(reloadedPlanet, DateTime.UtcNow);

                    Assert.Equal(1, created);
                    var npcUser = db.Users.Single(u => u.IsNpc);
                    var npcBase = db.PlayerBases.Include(b => b.PlanetSector).Single(b => b.UserId == npcUser.Id);
                    Assert.Equal(planet.Id, npcBase.PlanetSector.PlanetId);
                    Assert.True(npcBase.PlanetSector.IsSettlementSector);
                    Assert.NotEqual(firstSettlementSector.Id, npcBase.PlanetSectorId);
                }
            }
        }

        [Fact]
        public void EnsureNpcPresence_CalledAgain_DoesNotExceedTheCap()
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
                    var planet = db.Planets.Include(p => p.Sectors).Single(p => p.Name == "P3X-742");
                    var firstSettlementSector = planet.Sectors.Where(s => s.IsSettlementSector).OrderBy(s => s.Number).First();
                    db.Users.Add(new User { ServerId = serverId, UserName = "human1", Email = "human1@example.test", PasswordHash = "h", PasswordSalt = "s", Faction = faction, ResearchLevels = new ResearchLevels(), Bases = { new PlayerBase { Name = "Basis 1", Faction = faction, PlanetSector = firstSettlementSector, Resources = new ResourceStock(), BuildingLevels = new BuildingLevels(), Ships = new BaseShips() } } });
                    db.SaveChanges();
                    var service = new NpcSpawnService(db, new EconomyService());

                    var firstPass = db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase).Single(p => p.Id == planet.Id);
                    int firstCreated = service.EnsureNpcPresence(firstPass, DateTime.UtcNow);
                    var secondPass = db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase).Single(p => p.Id == planet.Id);
                    int secondCreated = service.EnsureNpcPresence(secondPass, DateTime.UtcNow);

                    Assert.Equal(1, firstCreated);
                    Assert.Equal(0, secondCreated);
                    Assert.Single(db.Users.Where(u => u.IsNpc));
                }
            }
        }

        [Fact]
        public void EnsureNpcPresence_NpcBasesAreExcludedFromGalaxyRelevantQueries()
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
                    var planet = db.Planets.Include(p => p.Sectors).Single(p => p.Name == "P3X-742");
                    var firstSettlementSector = planet.Sectors.Where(s => s.IsSettlementSector).OrderBy(s => s.Number).First();
                    db.Users.Add(new User { ServerId = serverId, UserName = "human1", Email = "human1@example.test", PasswordHash = "h", PasswordSalt = "s", Faction = faction, ResearchLevels = new ResearchLevels(), Bases = { new PlayerBase { Name = "Basis 1", Faction = faction, PlanetSector = firstSettlementSector, Resources = new ResourceStock(), BuildingLevels = new BuildingLevels(), Ships = new BaseShips() } } });
                    db.SaveChanges();

                    var reloadedPlanet = db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase).Single(p => p.Id == planet.Id);
                    new NpcSpawnService(db, new EconomyService()).EnsureNpcPresence(reloadedPlanet, DateTime.UtcNow);

                    var attackableBasesOnPlanet = db.PlayerBases.Include(b => b.User).Where(b => b.PlanetSector.PlanetId == planet.Id && !b.User.IsNpc).ToList();

                    Assert.Single(attackableBasesOnPlanet);
                    Assert.Equal("human1", attackableBasesOnPlanet.Single().User.UserName);
                }
            }
        }
    }
}
