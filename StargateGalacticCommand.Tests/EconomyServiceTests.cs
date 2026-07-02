using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class EconomyServiceTests
    {
        [Fact]
        public void CalculateBuildingCost_LevelOne_ReturnsBaseCost()
        {
            EconomyService service = new EconomyService();

            BuildCost cost = service.CalculateBuildingCost(BuildingType.NaquadahMine, 1);

            Assert.Equal(60, cost.Naquadah);
            Assert.Equal(20, cost.Trinium);
            Assert.Equal(0, cost.Deuterium);
            Assert.Equal(20, cost.Supplies);
            Assert.Equal(35, cost.Seconds);
        }

        [Fact]
        public void CalculateBuildingCost_HigherLevel_IncreasesCostAndTime()
        {
            EconomyService service = new EconomyService();

            BuildCost levelOne = service.CalculateBuildingCost(BuildingType.ResearchLab, 1);
            BuildCost levelFive = service.CalculateBuildingCost(BuildingType.ResearchLab, 5);

            Assert.True(levelFive.Naquadah > levelOne.Naquadah);
            Assert.True(levelFive.Trinium > levelOne.Trinium);
            Assert.True(levelFive.Seconds > levelOne.Seconds);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void CalculateBuildingCost_InvalidTargetLevel_Throws(int targetLevel)
        {
            EconomyService service = new EconomyService();

            Assert.Throws<ArgumentOutOfRangeException>(() => service.CalculateBuildingCost(BuildingType.CommandBunker, targetLevel));
        }

        [Fact]
        public void CalculateHourlyProduction_NonProductionBuilding_ReturnsZero()
        {
            EconomyService service = new EconomyService();

            int production = service.CalculateHourlyProduction(BuildingType.GateRoom, 5);

            Assert.Equal(0, production);
        }

        [Fact]
        public void CalculateHourlyProduction_ProductionBuilding_IncreasesWithLevel()
        {
            EconomyService service = new EconomyService();

            int levelOne = service.CalculateHourlyProduction(BuildingType.TriniumExtractor, 1);
            int levelSix = service.CalculateHourlyProduction(BuildingType.TriniumExtractor, 6);

            Assert.True(levelOne > 0);
            Assert.True(levelSix > levelOne);
        }
    }
}
