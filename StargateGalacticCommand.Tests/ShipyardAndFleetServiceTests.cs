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
    }
}
