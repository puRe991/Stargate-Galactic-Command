using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class ResearchServiceTests
    {
        [Fact]
        public void StartResearch_SpendsResearchCost()
        {
            var service = CreateService();
            var user = CreateUser("SGC");
            var playerBase = CreateBase();
            service.StartResearch(user, playerBase, ResearchType.GateAddressing, Now);
            Assert.Equal(880, playerBase.Resources.Naquadah);
            Assert.Equal(920, playerBase.Resources.Trinium);
            Assert.Equal(920, playerBase.Resources.Supplies);
            Assert.Equal(5, playerBase.Resources.Intel);
        }

        [Fact]
        public void StartResearch_RequiresPrerequisite()
        {
            var service = CreateService();
            var user = CreateUser("SGC");
            var playerBase = CreateBase();
            Assert.Throws<InvalidOperationException>(() => service.StartResearch(user, playerBase, ResearchType.ShieldTechnology, Now));
            Assert.Empty(user.ResearchQueue);
        }

        [Fact]
        public void CompleteFinishedResearch_IncreasesLevelExactlyOnce()
        {
            var service = CreateService();
            var user = CreateUser("SGC");
            var playerBase = CreateBase();
            service.StartResearch(user, playerBase, ResearchType.GateAddressing, Now);
            service.CompleteFinishedResearch(user, Now.AddHours(1));
            service.CompleteFinishedResearch(user, Now.AddHours(2));
            Assert.Equal(1, user.ResearchLevels.GateAddressing);
            Assert.Empty(user.ResearchQueue);
        }

        [Fact]
        public void Catalog_ShowsOnlyOwnFactionResearch()
        {
            var catalog = new ResearchCatalogService();
            var tokra = CreateFaction("Tok’ra");
            var visible = catalog.GetAvailableForFaction(tokra).ToList();
            Assert.Contains(visible, r => r.Type == ResearchType.CovertInfiltration);
            Assert.DoesNotContain(visible, r => r.Type == ResearchType.AsgardDataAnalysis);
        }

        [Fact]
        public void TauriResearchSpeedBonus_ReducesResearchDuration()
        {
            var catalog = new ResearchCatalogService();
            var modifiers = new FactionModifierService();
            int neutral = catalog.CalculateResearchSeconds(ResearchType.GateAddressing, 0, 1, 1.0);
            int tauri = catalog.CalculateResearchSeconds(ResearchType.GateAddressing, 0, 1, modifiers.GetResearchSpeedMultiplier(CreateFaction("SGC")));
            Assert.True(tauri < neutral);
        }

        [Fact]
        public void TokraIntelBonus_IncreasesIntelProduction()
        {
            var economy = new EconomyService();
            var levels = new BuildingLevels { CommandCenter = 1, SensorStation = 10 };
            int neutral = economy.CalculateHourlyProduction(levels, new ResearchLevels(), CreateFaction("SGC")).Intel;
            int tokra = economy.CalculateHourlyProduction(levels, new ResearchLevels(), CreateFaction("Tok’ra")).Intel;
            Assert.Equal(10, neutral);
            Assert.Equal(11, tokra);
        }

        private static readonly DateTime Now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static ResearchQueueService CreateService()
        {
            return new ResearchQueueService(new ResearchCatalogService(), new ResourceService(), new FactionModifierService());
        }

        private static User CreateUser(string factionShortName)
        {
            var faction = CreateFaction(factionShortName);
            return new User { Id = 1, Faction = faction, FactionId = faction.Id, ResearchLevels = new ResearchLevels { UserId = 1 } };
        }

        private static PlayerBase CreateBase()
        {
            return new PlayerBase { Id = 1, Resources = new ResourceStock { Naquadah = 1000, Trinium = 1000, Supplies = 1000, Energy = 1000, Personnel = 1000, Intel = 10 }, BuildingLevels = new BuildingLevels { CommandCenter = 1, ResearchLab = 1 } };
        }

        private static Faction CreateFaction(string shortName)
        {
            return new Faction { Id = shortName.GetHashCode(), Name = shortName, ShortName = shortName };
        }
    }
}
