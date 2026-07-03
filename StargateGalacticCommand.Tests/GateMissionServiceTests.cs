using System;
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

        private static readonly DateTime Now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static User CreateUser() => new User { Id = 1, Faction = new Faction { ShortName = "SGC" }, ResearchLevels = new ResearchLevels { GateAddressing = 1 } };
        private static PlayerBase CreateBase() => new PlayerBase { Id = 1, Resources = new ResourceStock { Energy = 1000, Supplies = 1000, Personnel = 1000, Intel = 10 }, BuildingLevels = new BuildingLevels { GateControlRoom = 1 } };
        private static GateAddress Address() => new GateAddress { Id = 1, Code = "P4X-219", WorldName = "P4X-219", Description = "verlassene Menschenkolonie", RiskLevel = 3 };
        private static MissionTeam Team(User user) => new MissionTeam { Id = 1, User = user, UserId = user.Id, Name = "SG-Team", Strength = 7, Science = 8, Diplomacy = 7, Stealth = 6, CarryCapacity = 6, Risk = 4, IsAvailable = true };
    }
}
