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

        [Theory]
        [InlineData("SGC", GateMissionType.SearchArtifact, 4)]
        [InlineData("Jaffa", GateMissionType.RiskAnalysis, 4)]
        [InlineData("Tok’ra", GateMissionType.AnalyzeAddress, 4)]
        [InlineData("Lucian", GateMissionType.SecureResources, 4)]
        [InlineData("SGC", GateMissionType.SecureResources, 0)]
        [InlineData("Jaffa", GateMissionType.SearchArtifact, 0)]
        public void GetGateMissionScoreBonus_MatchesFactionSpecialtyOnly(string factionShortName, GateMissionType missionType, int expectedBonus)
        {
            var modifiers = new FactionModifierService();
            var faction = new Faction { ShortName = factionShortName };

            Assert.Equal(expectedBonus, modifiers.GetGateMissionScoreBonus(faction, missionType));
        }

        [Fact]
        public void CompleteMission_FactionSpecialtyBonus_TurnsPartialSuccessIntoSuccess()
        {
            var service = new GateMissionService(new ResourceService());
            var user = new User { Id = 1, Faction = new Faction { ShortName = "SGC" }, ResearchLevels = new ResearchLevels { GateAddressing = 1 } };
            var playerBase = new PlayerBase { Id = 1, Faction = user.Faction, Resources = new ResourceStock { Energy = 1000, Supplies = 1000, Personnel = 1000, Intel = 10 }, BuildingLevels = new BuildingLevels { GateControlRoom = 1 } };
            var team = new MissionTeam { Id = 1, User = user, UserId = user.Id, Name = "Grenzfall-Team", Strength = 6, Science = 6, Diplomacy = 6, Stealth = 6, CarryCapacity = 6, Risk = 5, IsAvailable = true };
            var mission = service.StartMission(user, playerBase, Address(), team, GateMissionType.SearchArtifact, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc);

            // Ohne den SGC-Bonus (+4) läge der Score bei 25 (PartialSuccess, factor 0.5); mit Bonus bei 29 (Success, factor 1.0).
            Assert.True(report.ArtifactLeadFound);
            Assert.Equal(6, report.IntelFound);
        }

        [Fact]
        public void CompleteMission_NoFactionBonus_WhenMissionTypeIsNotTheFactionSpecialty()
        {
            var service = new GateMissionService(new ResourceService());
            var user = new User { Id = 1, Faction = new Faction { ShortName = "Lucian" }, ResearchLevels = new ResearchLevels { GateAddressing = 1 } };
            var playerBase = new PlayerBase { Id = 1, Faction = user.Faction, Resources = new ResourceStock { Energy = 1000, Supplies = 1000, Personnel = 1000, Intel = 10 }, BuildingLevels = new BuildingLevels { GateControlRoom = 1 } };
            var team = new MissionTeam { Id = 1, User = user, UserId = user.Id, Name = "Grenzfall-Team", Strength = 6, Science = 6, Diplomacy = 6, Stealth = 6, CarryCapacity = 6, Risk = 5, IsAvailable = true };
            var mission = service.StartMission(user, playerBase, Address(), team, GateMissionType.SearchArtifact, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc);

            Assert.False(report.ArtifactLeadFound);
            Assert.Equal(3, report.IntelFound);
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

        [Fact]
        public void CompleteMission_AnomalyRoll_TriggersOnSuccessAndMarksAddressExhausted()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var playerBase = CreateBase();
            var address = Address();
            var mission = service.StartMission(user, playerBase, address, Team(user), GateMissionType.Explore, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc, new FixedRandom(0.0));

            Assert.Equal(GateAnomalyType.AncientRuin, report.AnomalyType);
            Assert.True(address.AnomalyFound);
            Assert.True(report.IntelFound >= 40);
        }

        [Fact]
        public void CompleteMission_AnomalyRoll_DoesNotTriggerWhenRollMisses()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var playerBase = CreateBase();
            var address = Address();
            var mission = service.StartMission(user, playerBase, address, Team(user), GateMissionType.Explore, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc, new FixedRandom(0.5));

            Assert.Null(report.AnomalyType);
            Assert.False(address.AnomalyFound);
        }

        [Fact]
        public void CompleteMission_AnomalyRoll_NeverTriggersForNonExplorationMissionTypes()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var playerBase = CreateBase();
            var address = Address();
            var mission = service.StartMission(user, playerBase, address, Team(user), GateMissionType.SecureResources, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc, new FixedRandom(0.0));

            Assert.Null(report.AnomalyType);
        }

        [Fact]
        public void CompleteMission_AnomalyRoll_SkipsAlreadyExhaustedAddress()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var playerBase = CreateBase();
            var address = Address();
            address.AnomalyFound = true;
            var mission = service.StartMission(user, playerBase, address, Team(user), GateMissionType.Explore, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc, new FixedRandom(0.0));

            Assert.Null(report.AnomalyType);
        }

        [Fact]
        public void CompleteMission_AnomalyRoll_NeverTriggersOnFailure()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var playerBase = CreateBase();
            var address = Address();
            var weakTeam = new MissionTeam { Id = 2, User = user, UserId = user.Id, Name = "Grenzschutz", Strength = 1, Science = 1, Diplomacy = 1, Stealth = 1, CarryCapacity = 1, Risk = 10, IsAvailable = true };
            var mission = service.StartMission(user, playerBase, address, weakTeam, GateMissionType.Explore, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc, new FixedRandom(0.0));

            Assert.Equal(GateMissionOutcome.WoundedOrLosses, report.Outcome);
            Assert.Null(report.AnomalyType);
        }

        [Fact]
        public void CompleteMission_SeasonFocusAddress_AppliesRewardMultiplier()
        {
            var season = new SeasonService();
            int weekIndex = season.GetWeekIndex(Now);
            var service = new GateMissionService(new ResourceService(), season: season);
            var user = CreateUser();
            var playerBase = CreateBase();
            var focusAddress = new GateAddress { Id = weekIndex, Code = "P5X-FOCUS", WorldName = "P5X-FOCUS", Description = "Fokusadresse", RiskLevel = 3 };
            var mission = service.StartMission(user, playerBase, focusAddress, Team(user), GateMissionType.SecureResources, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc);

            Assert.True(report.IsSeasonFocusBonus);
            Assert.Contains("Fokuswoche", report.Summary);
        }

        [Fact]
        public void CompleteMission_NonFocusAddress_DoesNotApplySeasonBonus()
        {
            var season = new SeasonService();
            int weekIndex = season.GetWeekIndex(Now);
            int otherId = (weekIndex + 1) % SeasonService.FocusBucketModulus;
            var service = new GateMissionService(new ResourceService(), season: season);
            var user = CreateUser();
            var playerBase = CreateBase();
            var otherAddress = new GateAddress { Id = otherId, Code = "P6X-OTHER", WorldName = "P6X-OTHER", Description = "andere Adresse", RiskLevel = 3 };
            var mission = service.StartMission(user, playerBase, otherAddress, Team(user), GateMissionType.SecureResources, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc);

            Assert.False(report.IsSeasonFocusBonus);
        }

        private class FixedRandom : Random
        {
            private readonly double _value;
            public FixedRandom(double value) { _value = value; }
            public override double NextDouble() => _value;
        }

        private static readonly DateTime Now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static User CreateUser() => new User { Id = 1, Faction = new Faction { ShortName = "SGC" }, ResearchLevels = new ResearchLevels { GateAddressing = 1 } };
        private static PlayerBase CreateBase() => new PlayerBase { Id = 1, Resources = new ResourceStock { Energy = 1000, Supplies = 1000, Personnel = 1000, Intel = 10 }, BuildingLevels = new BuildingLevels { GateControlRoom = 1 } };
        private static GateAddress Address() => new GateAddress { Id = 1, Code = "P4X-219", WorldName = "P4X-219", Description = "verlassene Menschenkolonie", RiskLevel = 3 };
        private static MissionTeam Team(User user) => new MissionTeam { Id = 1, User = user, UserId = user.Id, Name = "SG-Team", Strength = 7, Science = 8, Diplomacy = 7, Stealth = 6, CarryCapacity = 6, Risk = 4, IsAvailable = true };
    }
}
