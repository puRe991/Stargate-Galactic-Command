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

        [Fact]
        public void ArmDecoy_SpendsIntelAndCreatesProfileWithOneCharge()
        {
            var service = new EspionageService(new ResourceService());
            var playerBase = Base("SGC", 1, 100);
            var fakeValues = new ResourceStock { Naquadah = 9999, Trinium = 8888 };

            var profile = service.ArmDecoy(playerBase, null, fakeValues, 500, Now);

            Assert.Equal(100 - EspionageService.DecoyChargeIntelCost, playerBase.Resources.Intel);
            Assert.Equal(1, profile.Charges);
            Assert.True(profile.IsActive);
            Assert.Equal(9999, profile.FakeNaquadah);
            Assert.Equal(8888, profile.FakeTrinium);
            Assert.Equal(500, profile.FakeShipTotal);
        }

        [Fact]
        public void ArmDecoy_CapsChargesAtMaximum()
        {
            var service = new EspionageService(new ResourceService());
            var playerBase = Base("SGC", 1, 1000);
            var existing = new DecoyProfile { PlayerBaseId = playerBase.Id, Charges = EspionageService.MaxDecoyCharges };

            service.ArmDecoy(playerBase, existing, new ResourceStock(), 0, Now);

            Assert.Equal(EspionageService.MaxDecoyCharges, existing.Charges);
        }

        [Fact]
        public void CalculateDeceptionChance_DecreasesAsAttackerSkillIncreasesButHasAFloor()
        {
            var service = new EspionageService(new ResourceService());
            double novice = service.CalculateDeceptionChance(User("SGC", 0, 0));
            double veteran = service.CalculateDeceptionChance(User("SGC", 10, 10));

            Assert.Equal(EspionageService.BaseDeceptionChance, novice);
            Assert.True(veteran < novice);
            Assert.True(veteran >= EspionageService.MinDeceptionChance);
        }

        [Fact]
        public void CreateReport_UsesFakeValuesWhenDecoyDeceivesAttacker()
        {
            var service = new EspionageService(new ResourceService());
            var attacker = User("SGC", 0, 0);
            var target = Base("Lucian", 1, 100);
            target.Resources.Naquadah = 12345;
            var mission = new EspionageMission { UserId = attacker.Id, User = attacker, TargetBase = target, MissionType = EspionageMissionType.SensorRecon, ReportDepth = 3, TargetCounterIntelligenceLevel = CounterIntelligenceLevel.Hardened };
            var decoy = new DecoyProfile { IsActive = true, Charges = 1, FakeNaquadah = 1, FakeTrinium = 1, FakeSupplies = 1, FakeEnergy = 1, FakePersonnel = 1, FakeIntel = 1, FakeShipTotal = 1 };

            var report = service.CreateReport(mission, null, null, null, Now, decoy, new FixedRandom(0.0));

            Assert.True(report.IsSuspectedDecoy);
            Assert.Contains("NQ 1,", report.Body);
            Assert.DoesNotContain("12345", report.Body);
            Assert.Equal(0, decoy.Charges);
        }

        [Fact]
        public void CreateReport_IgnoresDecoyBelowHardenedLevel()
        {
            var service = new EspionageService(new ResourceService());
            var attacker = User("SGC", 0, 0);
            var target = Base("Lucian", 1, 100);
            target.Resources.Naquadah = 12345;
            var mission = new EspionageMission { UserId = attacker.Id, User = attacker, TargetBase = target, MissionType = EspionageMissionType.SensorRecon, ReportDepth = 3, TargetCounterIntelligenceLevel = CounterIntelligenceLevel.Guarded };
            var decoy = new DecoyProfile { IsActive = true, Charges = 1 };

            var report = service.CreateReport(mission, null, null, null, Now, decoy, new FixedRandom(0.0));

            Assert.False(report.IsSuspectedDecoy);
            Assert.Contains("12345", report.Body);
            Assert.Equal(1, decoy.Charges);
        }

        [Fact]
        public void CreateReport_IgnoresDecoyWithNoChargesLeft()
        {
            var service = new EspionageService(new ResourceService());
            var attacker = User("SGC", 0, 0);
            var target = Base("Lucian", 1, 100);
            var mission = new EspionageMission { UserId = attacker.Id, User = attacker, TargetBase = target, MissionType = EspionageMissionType.SensorRecon, ReportDepth = 3, TargetCounterIntelligenceLevel = CounterIntelligenceLevel.Lockdown };
            var decoy = new DecoyProfile { IsActive = true, Charges = 0 };

            var report = service.CreateReport(mission, null, null, null, Now, decoy, new FixedRandom(0.0));

            Assert.False(report.IsSuspectedDecoy);
        }

        [Fact]
        public void CreateReport_DeceptionCanFailOnHighRandomRoll()
        {
            var service = new EspionageService(new ResourceService());
            var attacker = User("SGC", 0, 0);
            var target = Base("Lucian", 1, 100);
            var mission = new EspionageMission { UserId = attacker.Id, User = attacker, TargetBase = target, MissionType = EspionageMissionType.SensorRecon, ReportDepth = 3, TargetCounterIntelligenceLevel = CounterIntelligenceLevel.Hardened };
            var decoy = new DecoyProfile { IsActive = true, Charges = 1 };

            var report = service.CreateReport(mission, null, null, null, Now, decoy, new FixedRandom(0.99));

            Assert.False(report.IsSuspectedDecoy);
            Assert.Equal(1, decoy.Charges);
        }

        [Fact]
        public void CalculateDetectionRisk_TokraAttackResearchLowersRisk()
        {
            var service = new EspionageService(new ResourceService());
            var source = Base("Tok’ra", 1, 500);
            var target = Base("Lucian", 3, 100);
            var novice = User("Tok’ra", 0, 0);
            var veteran = new User { Id = 1, Faction = new Faction { Name = "Tok’ra", ShortName = "Tok’ra" }, ResearchLevels = new ResearchLevels { CovertInfiltration = 5, DeepCoverNetworks = 5, ShadowCouncilInfluence = 5 } };

            int riskWithoutResearch = service.CalculateDetectionRisk(novice, source, target, EspionageMissionType.SensorRecon, 50).DetectionRiskPercent;
            int riskWithResearch = service.CalculateDetectionRisk(veteran, source, target, EspionageMissionType.SensorRecon, 50).DetectionRiskPercent;

            Assert.True(riskWithResearch < riskWithoutResearch);
        }

        [Fact]
        public void CalculateDetectionRisk_CloakFieldCoordinationRaisesDefenderProtection()
        {
            var service = new EspionageService(new ResourceService());
            var attacker = User("SGC", 1, 0);
            var source = Base("SGC", 1, 500);
            var weakTarget = Base("Tok’ra", 3, 100);
            var hardenedTarget = Base("Tok’ra", 3, 100);
            hardenedTarget.User.ResearchLevels.CloakFieldCoordination = 10;

            int riskAgainstWeak = service.CalculateDetectionRisk(attacker, source, weakTarget, EspionageMissionType.SensorRecon, 50).DetectionRiskPercent;
            int riskAgainstHardened = service.CalculateDetectionRisk(attacker, source, hardenedTarget, EspionageMissionType.SensorRecon, 50).DetectionRiskPercent;

            Assert.True(riskAgainstHardened > riskAgainstWeak);
        }

        private class FixedRandom : Random
        {
            private readonly double _value;
            public FixedRandom(double value) { _value = value; }
            public override double NextDouble() => _value;
        }

        private static readonly DateTime Now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static User User(string faction, int sensorics, int stealth) => new User { Id = 1, Faction = new Faction { Name = faction, ShortName = faction }, ResearchLevels = new ResearchLevels { Sensorics = sensorics, StealthTechnology = stealth } };
        private static PlayerBase Base(string faction, int sensorStation, int intel) => new PlayerBase { Id = faction.GetHashCode() ^ sensorStation, Name = faction + " Basis", Faction = new Faction { Name = faction, ShortName = faction }, User = User(faction, 1, 0), Resources = new ResourceStock { Naquadah = 1000, Trinium = 1000, Supplies = 1000, Energy = 1000, Personnel = 1000, Intel = intel }, BuildingLevels = new BuildingLevels { SensorStation = sensorStation, GateControlRoom = 1, DefenseRing = 1 }, Ships = new BaseShips() };
    }
}
