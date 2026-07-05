using System;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Data;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class NpcSpawnServiceTests
    {
        [Fact]
        public void EnsureNpcPresence_DoesNotSpawnNpcs_WhenEnoughActiveHumanPlayersExist()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<GameDbContext>().UseSqlite(connection).Options;
                using (var db = new GameDbContext(options))
                {
                    DatabaseInitializer.Initialize(db, new GateMissionService(new ResourceService()), useMigrations: false);
                    var faction = db.Factions.OrderBy(f => f.Id).First();
                    var sectors = db.PlanetSectors
                        .Include(s => s.PlayerBase)
                        .Where(s => s.IsSettlementSector && s.PlayerBase == null)
                        .OrderBy(s => s.PlanetId)
                        .ThenBy(s => s.Number)
                        .Take(3)
                        .ToList();

                    for (int i = 0; i < sectors.Count; i++)
                    {
                        db.Users.Add(CreateHumanUser(i + 1, faction, sectors[i]));
                    }
                    db.SaveChanges();

                    var service = new NpcSpawnService();
                    int created = service.EnsureNpcPresence(db, new DateTime(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc), targetNpcCount: 3);

                    Assert.Equal(0, created);
                    Assert.Empty(db.Users.Where(u => u.IsNpc));
                    Assert.Equal(3, db.PlayerBases.Count(b => !b.User.IsNpc));
                }
            }
        }

        private static User CreateHumanUser(int index, Faction faction, PlanetSector sector)
        {
            var user = new User
            {
                UserName = "human" + index,
                Email = "human" + index + "@example.test",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                Faction = faction,
                CreatedAtUtc = DateTime.UtcNow,
                ResearchLevels = new ResearchLevels(),
            };

            user.Bases.Add(new PlayerBase
            {
                Name = "Human Base " + index,
                Faction = faction,
                PlanetSector = sector,
                Resources = new ResourceStock(),
                BuildingLevels = new BuildingLevels(),
                Ships = new BaseShips(),
                LastResourceUpdateUtc = DateTime.UtcNow
            });

            return user;
        }
    }
}
