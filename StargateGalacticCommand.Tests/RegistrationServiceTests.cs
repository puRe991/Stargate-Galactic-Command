using System.Linq;
using StargateGalacticCommand.Core.Services;
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
                    DatabaseInitializer.Initialize(db);
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
    }
}
