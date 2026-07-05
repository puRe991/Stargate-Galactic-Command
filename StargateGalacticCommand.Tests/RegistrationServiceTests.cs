using System.Linq;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
                    var service = new RegistrationService(new EconomyService(), new PasswordService());
                    var user = service.CreateUserWithStartBase(db.Users, db.Factions, db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase), "sam", "sam@example.test", "Password123", 1);
                    db.Users.Add(user); db.SaveChanges();
                    var created = db.Users.Include(u => u.Bases).ThenInclude(b => b.Resources).Include(u => u.Bases).ThenInclude(b => b.BuildingLevels).Single(u => u.UserName == "sam");
                    var startBase = created.Bases.Single();
                    Assert.Equal(500, startBase.Resources.Naquadah);
                    Assert.Equal(1, startBase.BuildingLevels.CommandCenter);
                    Assert.Equal(0, startBase.BuildingLevels.NaquadahRefinery);
                    Assert.Equal(2, db.PlayerBases.Include(b => b.PlanetSector).Single().PlanetSector.Number);
                    Assert.Single(db.Reports.Where(r => r.UserId == created.Id));
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
                    var faction = db.Factions.Single(f => f.Id == 1);
                    var fullPlanet = db.Planets.Include(p => p.Sectors).Single(p => p.Name == "P3X-742");
                    int index = 1;
                    foreach (var sector in fullPlanet.Sectors.Where(s => s.IsSettlementSector).OrderBy(s => s.Number))
                    {
                        db.Users.Add(new User { UserName = "taken" + index, Email = "taken" + index + "@example.test", PasswordHash = "h", PasswordSalt = "s", Faction = faction, ResearchLevels = new ResearchLevels(), Bases = { new PlayerBase { Name = "Basis " + index, Faction = faction, PlanetSector = sector, Resources = new ResourceStock(), BuildingLevels = new BuildingLevels(), Ships = new BaseShips() } } });
                        index++;
                    }
                    db.SaveChanges();

                    var service = new RegistrationService(new EconomyService(), new PasswordService());
                    var user = service.CreateUserWithStartBase(db.Users, db.Factions, db.Planets.Include(p => p.Sectors).ThenInclude(s => s.PlayerBase), "janet", "janet@example.test", "Password123", 1);
                    db.Users.Add(user); db.SaveChanges();

                    var createdBase = db.PlayerBases.Include(b => b.PlanetSector).ThenInclude(s => s.Planet).Single(b => b.Name == "janet Hauptbasis");
                    Assert.Equal("P4X-650", createdBase.PlanetSector.Planet.Name);
                    Assert.Equal(2, createdBase.PlanetSector.Number);
                }
            }
        }

    }
}
