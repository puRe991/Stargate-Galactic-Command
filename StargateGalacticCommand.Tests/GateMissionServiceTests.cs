using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class GateMissionServiceTests
    {
        [Fact]
        public void StartMission_RequiresGatePrerequisites()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var playerBase = CreateBase();
            playerBase.BuildingLevels.GateControlRoom = 0;
            Assert.Throws<InvalidOperationException>(() => service.StartMission(user, playerBase, Address(), Team(user), GateMissionType.Explore, Now));
        }

        [Fact]
        public void StartMission_SpendsCostAndDoesNotCompleteImmediately()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var playerBase = CreateBase();
            var mission = service.StartMission(user, playerBase, Address(), Team(user), GateMissionType.SecureResources, Now);
            Assert.Equal(965, playerBase.Resources.Energy);
            Assert.Equal(955, playerBase.Resources.Supplies);
            Assert.Equal(992, playerBase.Resources.Personnel);
            Assert.False(mission.IsCompleted);
            Assert.True(mission.CompletesAtUtc > Now);
        }

        [Fact]
        public void CompleteMission_CalculatesOutcomeAndCreatesReport()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var playerBase = CreateBase();
            var mission = service.StartMission(user, playerBase, Address(), Team(user), GateMissionType.SearchArtifact, Now);
            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc);
            Assert.True(mission.IsCompleted);
            Assert.NotNull(report);
            Assert.True(report.IntelFound > 0 || report.ArtifactLeadFound || report.PersonnelLost > 0);
            Assert.Contains("keine", report.Summary.ToLowerInvariant());
        }

        [Fact]
        public void ApplyFoundColonyResult_CreatesPlanetForKnownAddress()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var address = new GateAddress { Id = 7, Code = "P8X-404", WorldName = "P8X-404", Description = "neue Koloniewelt", RiskLevel = 2 };
            user.KnownGateAddresses.Add(new KnownGateAddress { UserId = user.Id, GateAddressId = address.Id, GateAddress = address });
            var planets = new System.Collections.Generic.List<Planet> { new Planet { Id = 1, Name = "P3X-742" } };

            string summary = service.ApplyFoundColonyResult(user, address, planets, Now);

            Assert.Equal(2, planets.Count);
            Assert.Equal("P8X-404", planets[1].Name);
            Assert.Equal(3, planets[1].Sectors.Count(s => s.IsSettlementSector));
            Assert.Same(planets[1], address.Planet);
            Assert.Contains("Großschiffe", summary);
        }

        [Fact]
        public void ApplyFoundColonyResult_RequiresKnownAddress()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var address = new GateAddress { Id = 8, Code = "P8X-405", RiskLevel = 2 };
            var planets = new System.Collections.Generic.List<Planet>();

            Assert.Throws<InvalidOperationException>(() => service.ApplyFoundColonyResult(user, address, planets, Now));
        }

        [Fact]
        public void DiscoverRandomAddress_ReturnsNullWhenNoCandidates()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var result = service.DiscoverRandomAddress(user, new System.Collections.Generic.List<GateAddress>(), new Random(1), "Adresse analysieren", Now);
            Assert.Null(result);
        }

        [Fact]
        public void DiscoverRandomAddress_PicksOneOfTheCandidatesForTheUser()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var candidates = new System.Collections.Generic.List<GateAddress>
            {
                new GateAddress { Id = 10, Code = "P1A-100" },
                new GateAddress { Id = 11, Code = "P2B-200" },
                new GateAddress { Id = 12, Code = "P3C-300" }
            };

            var discovered = service.DiscoverRandomAddress(user, candidates, new Random(42), "Fernaufklärung", Now);

            Assert.NotNull(discovered);
            Assert.Equal(user.Id, discovered.UserId);
            Assert.Equal("Fernaufklärung", discovered.DiscoveryMethod);
            Assert.Contains(discovered.GateAddressId, candidates.Select(c => c.Id));
        }

        private static readonly DateTime Now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static User CreateUser() => new User { Id = 1, Faction = new Faction { ShortName = "SGC" }, ResearchLevels = new ResearchLevels { GateAddressing = 1 } };
        private static PlayerBase CreateBase() => new PlayerBase { Id = 1, Resources = new ResourceStock { Energy = 1000, Supplies = 1000, Personnel = 1000, Intel = 10 }, BuildingLevels = new BuildingLevels { GateControlRoom = 1 } };
        private static GateAddress Address() => new GateAddress { Id = 1, Code = "P4X-219", WorldName = "P4X-219", Description = "verlassene Menschenkolonie", RiskLevel = 3 };
        private static MissionTeam Team(User user) => new MissionTeam { Id = 1, User = user, UserId = user.Id, Name = "SG-Team", Strength = 7, Science = 8, Diplomacy = 7, Stealth = 6, CarryCapacity = 6, Risk = 4, IsAvailable = true };
    }
}
