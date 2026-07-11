using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class SpecialResourceCatalogServiceTests
    {
        [Fact]
        public void GetAll_HasOneUniqueDefinitionPerEnumValue()
        {
            var catalog = new SpecialResourceCatalogService();
            var allTypes = Enum.GetValues(typeof(SpecialResourceType)).Cast<SpecialResourceType>().ToList();
            var definitions = catalog.GetAll();

            Assert.Equal(allTypes.Count, definitions.Count);
            Assert.Equal(allTypes.OrderBy(t => t), definitions.Select(d => d.Type).OrderBy(t => t));
            Assert.Equal(definitions.Select(d => d.Type).Distinct().Count(), definitions.Count);
        }

        [Fact]
        public void GetAll_EveryDefinitionHasNameAndDescription()
        {
            var catalog = new SpecialResourceCatalogService();
            foreach (var definition in catalog.GetAll())
            {
                Assert.False(string.IsNullOrWhiteSpace(definition.Name));
                Assert.False(string.IsNullOrWhiteSpace(definition.Description));
            }
        }

        [Fact]
        public void Get_ReturnsMatchingDefinition()
        {
            var catalog = new SpecialResourceCatalogService();
            var definition = catalog.Get(SpecialResourceType.ZeroPointModule);
            Assert.Equal("ZPM (Zero Point Module)", definition.Name);
            Assert.Equal(SpecialResourceCategory.EnergySource, definition.Category);
        }

        [Fact]
        public void GetByCategory_OnlyReturnsMatchingCategory()
        {
            var catalog = new SpecialResourceCatalogService();
            var crystals = catalog.GetByCategory(SpecialResourceCategory.Crystal).ToList();
            Assert.NotEmpty(crystals);
            Assert.All(crystals, d => Assert.Equal(SpecialResourceCategory.Crystal, d.Category));
        }

        [Fact]
        public void PickFromCategory_IsDeterministicForSameSeed()
        {
            var catalog = new SpecialResourceCatalogService();
            var first = catalog.PickFromCategory(SpecialResourceCategory.Artifact, 3);
            var second = catalog.PickFromCategory(SpecialResourceCategory.Artifact, 3);
            Assert.Equal(first, second);
        }

        [Fact]
        public void PickFromCategory_HandlesNegativeSeeds()
        {
            var catalog = new SpecialResourceCatalogService();
            var picked = catalog.PickFromCategory(SpecialResourceCategory.RawMaterial, -7);
            Assert.Equal(SpecialResourceCategory.RawMaterial, catalog.Get(picked).Category);
        }
    }

    public class SpecialResourceServiceTests
    {
        [Fact]
        public void Add_CreatesNewEntryWhenNoneExists()
        {
            var service = new SpecialResourceService();
            var playerBase = new PlayerBase { Id = 1 };

            service.Add(playerBase, SpecialResourceType.AncientCrystals, 5);

            Assert.Equal(5, service.GetQuantity(playerBase, SpecialResourceType.AncientCrystals));
            Assert.Single(playerBase.SpecialResources);
        }

        [Fact]
        public void Add_AccumulatesOnExistingEntry()
        {
            var service = new SpecialResourceService();
            var playerBase = new PlayerBase { Id = 1 };

            service.Add(playerBase, SpecialResourceType.AncientCrystals, 5);
            service.Add(playerBase, SpecialResourceType.AncientCrystals, 3);

            Assert.Equal(8, service.GetQuantity(playerBase, SpecialResourceType.AncientCrystals));
            Assert.Single(playerBase.SpecialResources);
        }

        [Fact]
        public void GetQuantity_ReturnsZeroWhenNotOwned()
        {
            var service = new SpecialResourceService();
            var playerBase = new PlayerBase { Id = 1 };
            Assert.Equal(0, service.GetQuantity(playerBase, SpecialResourceType.WraithCrystals));
        }

        [Fact]
        public void Spend_ReducesQuantityWhenEnoughIsAvailable()
        {
            var service = new SpecialResourceService();
            var playerBase = new PlayerBase { Id = 1 };
            service.Add(playerBase, SpecialResourceType.Kassa, 10);

            service.Spend(playerBase, SpecialResourceType.Kassa, 4);

            Assert.Equal(6, service.GetQuantity(playerBase, SpecialResourceType.Kassa));
        }

        [Fact]
        public void Spend_ThrowsWhenNotEnough()
        {
            var service = new SpecialResourceService();
            var playerBase = new PlayerBase { Id = 1 };
            service.Add(playerBase, SpecialResourceType.Kassa, 2);

            Assert.Throws<InvalidOperationException>(() => service.Spend(playerBase, SpecialResourceType.Kassa, 5));
        }

        [Fact]
        public void HasEnough_ReflectsCurrentQuantity()
        {
            var service = new SpecialResourceService();
            var playerBase = new PlayerBase { Id = 1 };
            service.Add(playerBase, SpecialResourceType.Kassa, 2);

            Assert.True(service.HasEnough(playerBase, SpecialResourceType.Kassa, 2));
            Assert.False(service.HasEnough(playerBase, SpecialResourceType.Kassa, 3));
        }
    }

    public class GateMissionSpecialResourceRewardTests
    {
        [Fact]
        public void CompleteMission_AnalyzeAddress_AlwaysGrantsStargateAddressesOnSuccess()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var playerBase = CreateBase();
            var mission = service.StartMission(user, playerBase, Address(), Team(user), GateMissionType.AnalyzeAddress, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc);

            Assert.Equal(SpecialResourceType.StargateAddresses, report.SpecialResourceFound);
            Assert.Equal(1, report.SpecialResourceAmount);
            Assert.Equal(1, new SpecialResourceService().GetQuantity(playerBase, SpecialResourceType.StargateAddresses));
        }

        [Fact]
        public void CompleteMission_FoundColony_GrantsPlanetaryInfrastructure()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var playerBase = CreateBase();
            var mission = service.StartMission(user, playerBase, Address(), Team(user), GateMissionType.FoundColony, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc);

            Assert.Equal(SpecialResourceType.PlanetaryInfrastructure, report.SpecialResourceFound);
            Assert.Equal(1, report.SpecialResourceAmount);
        }

        [Fact]
        public void CompleteMission_SecureResources_GrantsRawMaterialCategoryReward()
        {
            var service = new GateMissionService(new ResourceService());
            var catalog = new SpecialResourceCatalogService();
            var user = CreateUser();
            var playerBase = CreateBase();
            var mission = service.StartMission(user, playerBase, Address(), Team(user), GateMissionType.SecureResources, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc);

            Assert.NotNull(report.SpecialResourceFound);
            Assert.Equal(SpecialResourceCategory.RawMaterial, catalog.Get(report.SpecialResourceFound.Value).Category);
            Assert.True(report.SpecialResourceAmount > 0);
        }

        [Fact]
        public void CompleteMission_SearchArtifact_OnlyGrantsArtifactOnFullSuccess()
        {
            var service = new GateMissionService(new ResourceService());
            var catalog = new SpecialResourceCatalogService();
            var user = new User { Id = 1, Faction = new Faction { ShortName = "SGC" }, ResearchLevels = new ResearchLevels { GateAddressing = 1 } };
            var playerBase = new PlayerBase { Id = 1, Faction = user.Faction, Resources = new ResourceStock { Energy = 1000, Supplies = 1000, Personnel = 1000, Intel = 10 }, BuildingLevels = new BuildingLevels { GateControlRoom = 1 } };
            var team = new MissionTeam { Id = 1, User = user, UserId = user.Id, Name = "Grenzfall-Team", Strength = 6, Science = 6, Diplomacy = 6, Stealth = 6, CarryCapacity = 6, Risk = 5, IsAvailable = true };
            var mission = service.StartMission(user, playerBase, Address(), team, GateMissionType.SearchArtifact, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc);

            Assert.True(report.ArtifactLeadFound);
            Assert.NotNull(report.SpecialResourceFound);
            Assert.Equal(SpecialResourceCategory.Artifact, catalog.Get(report.SpecialResourceFound.Value).Category);
        }

        [Fact]
        public void CompleteMission_SearchArtifact_GrantsNothingOnPartialSuccess()
        {
            var service = new GateMissionService(new ResourceService());
            var user = new User { Id = 1, Faction = new Faction { ShortName = "Lucian" }, ResearchLevels = new ResearchLevels { GateAddressing = 1 } };
            var playerBase = new PlayerBase { Id = 1, Faction = user.Faction, Resources = new ResourceStock { Energy = 1000, Supplies = 1000, Personnel = 1000, Intel = 10 }, BuildingLevels = new BuildingLevels { GateControlRoom = 1 } };
            var team = new MissionTeam { Id = 1, User = user, UserId = user.Id, Name = "Grenzfall-Team", Strength = 6, Science = 6, Diplomacy = 6, Stealth = 6, CarryCapacity = 6, Risk = 5, IsAvailable = true };
            var mission = service.StartMission(user, playerBase, Address(), team, GateMissionType.SearchArtifact, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc);

            Assert.False(report.ArtifactLeadFound);
            Assert.Null(report.SpecialResourceFound);
        }

        [Fact]
        public void CompleteMission_Failure_GrantsNoSpecialResource()
        {
            var service = new GateMissionService(new ResourceService());
            var user = CreateUser();
            var playerBase = CreateBase();
            var weakTeam = new MissionTeam { Id = 2, User = user, UserId = user.Id, Name = "Grenzschutz", Strength = 1, Science = 1, Diplomacy = 1, Stealth = 1, CarryCapacity = 1, Risk = 10, IsAvailable = true };
            var mission = service.StartMission(user, playerBase, Address(), weakTeam, GateMissionType.Explore, Now);

            var report = service.CompleteMission(mission, playerBase, null, mission.CompletesAtUtc);

            Assert.Equal(GateMissionOutcome.WoundedOrLosses, report.Outcome);
            Assert.Null(report.SpecialResourceFound);
        }

        private static readonly DateTime Now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static User CreateUser() => new User { Id = 1, Faction = new Faction { ShortName = "SGC" }, ResearchLevels = new ResearchLevels { GateAddressing = 1 } };
        private static PlayerBase CreateBase() => new PlayerBase { Id = 1, Resources = new ResourceStock { Energy = 1000, Supplies = 1000, Personnel = 1000, Intel = 10 }, BuildingLevels = new BuildingLevels { GateControlRoom = 1 } };
        private static GateAddress Address() => new GateAddress { Id = 1, Code = "P4X-219", WorldName = "P4X-219", Description = "verlassene Menschenkolonie", RiskLevel = 3 };
        private static MissionTeam Team(User user) => new MissionTeam { Id = 1, User = user, UserId = user.Id, Name = "SG-Team", Strength = 7, Science = 8, Diplomacy = 7, Stealth = 6, CarryCapacity = 6, Risk = 4, IsAvailable = true };
    }
}
