using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class EspionageServiceTests
    {
        [Fact]
        public void StartMission_SpendsIntel()
        {
            var service = new EspionageService(new ResourceService());
            var source = Base("SGC", 1, 100);
            var mission = service.StartMission(User("SGC", 1, 0), source, Base("Jaffa", 2, 100), EspionageMissionType.SensorRecon, 25, Now);
            Assert.Equal(75, source.Resources.Intel);
            Assert.Equal(25, mission.IntelSpent);
        }

        [Fact]
        public void CalculateReportDepth_RespondsToIntelAndDefense()
        {
            var service = new EspionageService(new ResourceService());
            var user = User("SGC", 2, 0);
            var source = Base("SGC", 1, 500);
            var weakTarget = Base("Lucian", 1, 100);
            var hardTarget = Base("SGC", 5, 100);
            Assert.True(service.CalculateReportDepth(user, source, weakTarget, EspionageMissionType.SensorRecon, 250) > service.CalculateReportDepth(user, source, hardTarget, EspionageMissionType.SensorRecon, 5));
        }

        [Fact]
        public void CalculateDetectionRisk_IncreasesWithSensorStationDefense()
        {
            var service = new EspionageService(new ResourceService());
            var user = User("SGC", 1, 0);
            var source = Base("SGC", 1, 500);
            Assert.True(service.CalculateDetectionRisk(user, source, Base("Lucian", 5, 100), EspionageMissionType.SensorRecon, 50).DetectionRiskPercent > service.CalculateDetectionRisk(user, source, Base("Lucian", 1, 100), EspionageMissionType.SensorRecon, 50).DetectionRiskPercent);
        }

        [Fact]
        public void Tokra_HaveClearInfiltrationAdvantage()
        {
            var service = new EspionageService(new ResourceService());
            var target = Base("SGC", 3, 100);
            var sgcDepth = service.CalculateReportDepth(User("SGC", 1, 1), Base("SGC", 1, 500), target, EspionageMissionType.AgentInfiltration, 100);
            var tokraDepth = service.CalculateReportDepth(User("Tok’ra", 1, 1), Base("Tok’ra", 1, 500), target, EspionageMissionType.AgentInfiltration, 100);
            var sgcRisk = service.CalculateDetectionRisk(User("SGC", 1, 1), Base("SGC", 1, 500), target, EspionageMissionType.AgentInfiltration, 100).DetectionRiskPercent;
            var tokraRisk = service.CalculateDetectionRisk(User("Tok’ra", 1, 1), Base("Tok’ra", 1, 500), target, EspionageMissionType.AgentInfiltration, 100).DetectionRiskPercent;
            Assert.True(tokraDepth > sgcDepth);
            Assert.True(tokraRisk < sgcRisk);
        }

        [Fact]
        public void StartMission_RequiresStealthForCovertTypes()
        {
            var service = new EspionageService(new ResourceService());
            Assert.Throws<InvalidOperationException>(() => service.StartMission(User("SGC", 1, 0), Base("SGC", 1, 100), Base("Jaffa", 1, 100), EspionageMissionType.AgentInfiltration, 25, Now));
        }

        private static readonly DateTime Now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static User User(string faction, int sensorics, int stealth) => new User { Id = 1, Faction = new Faction { Name = faction, ShortName = faction }, ResearchLevels = new ResearchLevels { Sensorics = sensorics, StealthTechnology = stealth } };
        private static PlayerBase Base(string faction, int sensorStation, int intel) => new PlayerBase { Id = faction.GetHashCode() ^ sensorStation, Name = faction + " Basis", Faction = new Faction { Name = faction, ShortName = faction }, User = User(faction, 1, 0), Resources = new ResourceStock { Naquadah = 1000, Trinium = 1000, Supplies = 1000, Energy = 1000, Personnel = 1000, Intel = intel }, BuildingLevels = new BuildingLevels { SensorStation = sensorStation, GateControlRoom = 1, DefenseRing = 1 }, Ships = new BaseShips() };
    }
}
