using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class TradeRouteServiceTests
    {
        private static PlayerBase Base(int id) => new PlayerBase { Id = id, Name = "Base " + id };
        private static readonly DateTime Now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void CreateRoute_ThrowsWhenOriginAndTargetAreTheSame()
        {
            var service = new TradeRouteService();
            var b = Base(1);
            Assert.Throws<InvalidOperationException>(() => service.CreateRoute(new User { Id = 1 }, b, b, ShipType.SmallTransporter, 1, new ResourceStock { Naquadah = 10 }, 6, 0, Now));
        }

        [Fact]
        public void CreateRoute_ThrowsWhenIntervalOutOfRange()
        {
            var service = new TradeRouteService();
            Assert.Throws<ArgumentOutOfRangeException>(() => service.CreateRoute(new User { Id = 1 }, Base(1), Base(2), ShipType.SmallTransporter, 1, new ResourceStock { Naquadah = 10 }, TradeRouteService.MinIntervalHours - 1, 0, Now));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.CreateRoute(new User { Id = 1 }, Base(1), Base(2), ShipType.SmallTransporter, 1, new ResourceStock { Naquadah = 10 }, TradeRouteService.MaxIntervalHours + 1, 0, Now));
        }

        [Fact]
        public void CreateRoute_ThrowsWhenAtMaxActiveRoutes()
        {
            var service = new TradeRouteService();
            Assert.Throws<InvalidOperationException>(() => service.CreateRoute(new User { Id = 1 }, Base(1), Base(2), ShipType.SmallTransporter, 1, new ResourceStock { Naquadah = 10 }, 6, TradeRouteService.MaxActiveRoutesPerUser, Now));
        }

        [Fact]
        public void CreateRoute_ThrowsWhenCargoIsEmpty()
        {
            var service = new TradeRouteService();
            Assert.Throws<InvalidOperationException>(() => service.CreateRoute(new User { Id = 1 }, Base(1), Base(2), ShipType.SmallTransporter, 1, new ResourceStock(), 6, 0, Now));
        }

        [Fact]
        public void CreateRoute_IsDueImmediatelyAfterCreation()
        {
            var service = new TradeRouteService();
            var route = service.CreateRoute(new User { Id = 1 }, Base(1), Base(2), ShipType.SmallTransporter, 2, new ResourceStock { Naquadah = 100 }, 8, 0, Now);

            Assert.True(service.IsDue(route, Now));
            Assert.Equal(8, route.IntervalHours);
            Assert.True(route.IsActive);
        }

        [Fact]
        public void MarkExecuted_AdvancesNextDueByInterval()
        {
            var service = new TradeRouteService();
            var route = service.CreateRoute(new User { Id = 1 }, Base(1), Base(2), ShipType.SmallTransporter, 1, new ResourceStock { Naquadah = 10 }, 6, 0, Now);

            service.MarkExecuted(route, Now);

            Assert.Equal(Now, route.LastExecutedAtUtc);
            Assert.Equal(Now.AddHours(6), route.NextDueAtUtc);
            Assert.False(service.IsDue(route, Now));
        }

        [Fact]
        public void Pause_MakesRouteNotDueRegardlessOfTime()
        {
            var service = new TradeRouteService();
            var route = service.CreateRoute(new User { Id = 1 }, Base(1), Base(2), ShipType.SmallTransporter, 1, new ResourceStock { Naquadah = 10 }, 6, 0, Now);

            service.Pause(route);

            Assert.False(service.IsDue(route, Now.AddDays(1)));
        }

        [Fact]
        public void Resume_MakesRouteDueImmediately()
        {
            var service = new TradeRouteService();
            var route = service.CreateRoute(new User { Id = 1 }, Base(1), Base(2), ShipType.SmallTransporter, 1, new ResourceStock { Naquadah = 10 }, 6, 0, Now);
            service.Pause(route);

            service.Resume(route, Now.AddHours(2));

            Assert.True(service.IsDue(route, Now.AddHours(2)));
        }
    }
}
