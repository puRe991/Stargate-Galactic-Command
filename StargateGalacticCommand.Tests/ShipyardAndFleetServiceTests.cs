using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class ShipyardAndFleetServiceTests
    {
        private static PlayerBase Base(int id, int userId = 1, int sector = 1)
        {
            var faction = new Faction { ShortName = "SGC" };
            return new PlayerBase { Id = id, UserId = userId, Faction = faction, Resources = new ResourceStock { Naquadah = 5000, Trinium = 5000, Supplies = 5000, Energy = 5000, Personnel = 5000 }, BuildingLevels = new BuildingLevels { HangarLandingZone = 1 }, Ships = new BaseShips(), PlanetSector = new PlanetSector { Number = sector, PlanetId = 1, Planet = new Planet { Id = 1, Name = "P3X-742" } } };
        }

        [Fact]
        public void ShipBuildCostsResourcesAndCompletes()
        {
            var service = new ShipyardService(new ResourceService());
            var b = Base(1);
            var startEnergy = b.Resources.Energy;
            service.StartBuild(b, ShipType.SmallTransporter, 1, DateTime.UtcNow);
            Assert.True(b.Resources.Energy < startEnergy);
            service.CompleteFinishedBuilds(b, DateTime.UtcNow.AddHours(1));
            Assert.Equal(1, b.Ships.SmallTransporter);
        }

        [Fact]
        public void TransportDeductsAndDeliversResourcesThenReturns()
        {
            var shipyard = new ShipyardService(new ResourceService());
            var service = new FleetService(shipyard);
            var origin = Base(1, 1, 1); var target = Base(2, 1, 4); origin.Ships.SmallTransporter = 1;
            var fleet = service.Start(origin, target, FleetMissionType.Transport, ShipType.SmallTransporter, 1, new ResourceStock { Naquadah = 50 }, DateTime.UtcNow);
            Assert.Equal(4950, origin.Resources.Naquadah);
            Assert.True(fleet.FuelCost > 0);
            service.Complete(fleet, fleet.ArrivesAtUtc.AddSeconds(1));
            Assert.Equal(5050, target.Resources.Naquadah);
            Assert.Equal(FleetMovementStatus.Returning, fleet.Status);
            service.Complete(fleet, fleet.ArrivesAtUtc.AddSeconds(1));
            Assert.Equal(1, origin.Ships.SmallTransporter);
        }

        [Fact]
        public void InactiveCombatShipCannotBeBuiltOrSentThroughGate()
        {
            var service = new ShipyardService(new ResourceService());
            Assert.Throws<InvalidOperationException>(() => service.StartBuild(Base(1), ShipType.F302, 1, DateTime.UtcNow));
        }

        [Fact]
        public void StartExploration_HasNoFixedTargetAndReturnsToOrigin()
        {
            var shipyard = new ShipyardService(new ResourceService());
            var service = new FleetService(shipyard);
            var origin = Base(1); origin.Ships.SmallTransporter = 2;
            var startEnergy = origin.Resources.Energy;

            var fleet = service.StartExploration(origin, ShipType.SmallTransporter, 2, 100, DateTime.UtcNow);

            Assert.Equal(origin.Id, fleet.TargetBaseId);
            Assert.Equal(FleetMissionType.Exploration, fleet.MissionType);
            Assert.Equal(0, origin.Ships.SmallTransporter);
            Assert.True(origin.Resources.Energy < startEnergy);
            Assert.True(fleet.FuelCost > 0);
        }

        [Fact]
        public void StartExploration_ThrowsWhenNotEnoughShips()
        {
            var shipyard = new ShipyardService(new ResourceService());
            var service = new FleetService(shipyard);
            var origin = Base(1);
            Assert.Throws<InvalidOperationException>(() => service.StartExploration(origin, ShipType.SmallTransporter, 1, 100, DateTime.UtcNow));
        }

        [Fact]
        public void StartExploration_ThenComplete_ReturnsShipsToOriginBase()
        {
            var shipyard = new ShipyardService(new ResourceService());
            var service = new FleetService(shipyard);
            var origin = Base(1); origin.Ships.SmallTransporter = 3;

            var fleet = service.StartExploration(origin, ShipType.SmallTransporter, 3, 100, DateTime.UtcNow);
            fleet.TargetBase = origin;
            service.Complete(fleet, fleet.ArrivesAtUtc.AddSeconds(1));
            Assert.Equal(FleetMovementStatus.Returning, fleet.Status);

            service.Complete(fleet, fleet.ArrivesAtUtc.AddSeconds(1));
            Assert.Equal(FleetMovementStatus.Completed, fleet.Status);
            Assert.Equal(3, origin.Ships.SmallTransporter);
        }

        [Fact]
        public void StartRecycle_ThrowsWhenFieldAlreadyRecycled()
        {
            var shipyard = new ShipyardService(new ResourceService());
            var service = new FleetService(shipyard);
            var origin = Base(1, 1, 1); origin.Ships.SmallTransporter = 1;
            var owner = Base(2, 2, 4);
            var field = new DebrisField { Id = 1, PlayerBaseId = owner.Id, PlayerBase = owner, Naquadah = 100, Trinium = 50, IsRecycled = true };

            Assert.Throws<InvalidOperationException>(() => service.StartRecycle(origin, field, ShipType.SmallTransporter, 1, DateTime.UtcNow));
        }

        [Fact]
        public void Recycle_FullyCollectsSmallFieldAndMarksItRecycled()
        {
            var shipyard = new ShipyardService(new ResourceService());
            var service = new FleetService(shipyard);
            var origin = Base(1, 1, 1); origin.Ships.SmallTransporter = 1;
            var owner = Base(2, 2, 4);
            var field = new DebrisField { Id = 1, PlayerBaseId = owner.Id, PlayerBase = owner, Naquadah = 300, Trinium = 200 };

            var fleet = service.StartRecycle(origin, field, ShipType.SmallTransporter, 1, DateTime.UtcNow);
            Assert.Equal(0, origin.Ships.SmallTransporter);
            Assert.Equal(FleetMissionType.Recycle, fleet.MissionType);

            service.Complete(fleet, fleet.ArrivesAtUtc.AddSeconds(1));
            Assert.Equal(FleetMovementStatus.Returning, fleet.Status);
            Assert.Equal(300, fleet.Naquadah);
            Assert.Equal(200, fleet.Trinium);
            Assert.True(field.IsRecycled);
            Assert.Equal(0, field.Naquadah);
            Assert.Equal(0, field.Trinium);

            var originNaquadahBefore = origin.Resources.Naquadah;
            service.Complete(fleet, fleet.ArrivesAtUtc.AddSeconds(1));
            Assert.Equal(FleetMovementStatus.Completed, fleet.Status);
            Assert.Equal(1, origin.Ships.SmallTransporter);
            Assert.Equal(originNaquadahBefore + 300, origin.Resources.Naquadah);
            Assert.Equal(200, fleet.Trinium);
        }

        [Fact]
        public void Recycle_CapsCollectionAtCargoCapacityAndLeavesFieldPartiallyRecycled()
        {
            var shipyard = new ShipyardService(new ResourceService());
            var service = new FleetService(shipyard);
            var origin = Base(1, 1, 1); origin.Ships.SmallTransporter = 1;
            var owner = Base(2, 2, 4);
            var field = new DebrisField { Id = 1, PlayerBaseId = owner.Id, PlayerBase = owner, Naquadah = 600, Trinium = 600 };

            var fleet = service.StartRecycle(origin, field, ShipType.SmallTransporter, 1, DateTime.UtcNow);
            service.Complete(fleet, fleet.ArrivesAtUtc.AddSeconds(1));

            Assert.Equal(250, fleet.Naquadah);
            Assert.Equal(250, fleet.Trinium);
            Assert.Equal(350, field.Naquadah);
            Assert.Equal(350, field.Trinium);
            Assert.False(field.IsRecycled);
        }
    }
}
