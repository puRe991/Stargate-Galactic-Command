using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class PlanetMarketServiceTests
    {
        private static readonly DateTime Now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void CreateOrder_ReservesOfferedResources()
        {
            var seller = User(1, "SGC"); var sellerBase = Base(1, seller, 1);
            var order = Service().CreateOrder(seller, sellerBase, TradeResourceType.Naquadah, 100, TradeResourceType.Trinium, 50, Now.AddHours(1), Now);
            Assert.Equal(400, sellerBase.Resources.Naquadah);
            Assert.Equal(1, order.PlanetId);
            Assert.True(order.IsOpen);
        }

        [Fact]
        public void BuyOrder_TransfersResourcesAndMarketFee()
        {
            var seller = User(1, "SGC"); var buyer = User(2, "Jaffa"); var sellerBase = Base(1, seller, 1); var buyerBase = Base(2, buyer, 1);
            var order = Service().CreateOrder(seller, sellerBase, TradeResourceType.Naquadah, 100, TradeResourceType.Trinium, 50, Now.AddHours(1), Now);
            order.Id = 99; order.SellerUser = seller;
            var transaction = Service().BuyOrder(order, buyer, buyerBase, sellerBase, Array.Empty<PlanetSector>(), Now.AddMinutes(1));
            Assert.Equal(600, buyerBase.Resources.Naquadah);
            Assert.Equal(450, buyerBase.Resources.Trinium);
            Assert.Equal(549, sellerBase.Resources.Trinium);
            Assert.Equal(1, transaction.FeeAmount);
            Assert.NotNull(order.CompletedAtUtc);
        }

        [Fact]
        public void CancelOrder_ReturnsReservedResources()
        {
            var seller = User(1, "SGC"); var sellerBase = Base(1, seller, 1);
            var order = Service().CreateOrder(seller, sellerBase, TradeResourceType.Supplies, 75, TradeResourceType.Naquadah, 30, Now.AddHours(1), Now);
            Service().CancelOrder(order, seller.Id, sellerBase.Resources, Now.AddMinutes(2));
            Assert.Equal(500, sellerBase.Resources.Supplies);
            Assert.True(order.ReservedReturned);
            Assert.NotNull(order.CancelledAtUtc);
        }

        [Fact]
        public void ExpireOrder_ReturnsReservedResourcesAndBlocksPurchase()
        {
            var seller = User(1, "SGC"); var buyer = User(2, "Jaffa"); var sellerBase = Base(1, seller, 1); var buyerBase = Base(2, buyer, 1);
            var order = Service().CreateOrder(seller, sellerBase, TradeResourceType.Naquadah, 100, TradeResourceType.Trinium, 50, Now.AddMinutes(5), Now);
            Assert.True(Service().ExpireOrder(order, sellerBase.Resources, Now.AddMinutes(6)));
            Assert.Equal(500, sellerBase.Resources.Naquadah);
            Assert.Throws<InvalidOperationException>(() => Service().BuyOrder(order, buyer, buyerBase, sellerBase, null, Now.AddMinutes(7)));
        }

        [Fact]
        public void CalculateFee_UsesLucianBonus()
        {
            var fee = Service().CalculateFee(100, Service().CalculateFeeRate(new Faction { ShortName = "Lucian" }, Array.Empty<PlanetSector>()));
            Assert.Equal(2, fee); // 1.5 rounded up, no zero-fee trades.
        }

        [Fact]
        public void BuyOrder_RequiresSamePlanetAndPreventsDupe()
        {
            var seller = User(1, "SGC"); var buyer = User(2, "Jaffa"); var sellerBase = Base(1, seller, 1); var buyerBase = Base(2, buyer, 1);
            var order = Service().CreateOrder(seller, sellerBase, TradeResourceType.Naquadah, 100, TradeResourceType.Trinium, 50, Now.AddHours(1), Now);
            Service().BuyOrder(order, buyer, buyerBase, sellerBase, null, Now.AddMinutes(1));
            Assert.Throws<InvalidOperationException>(() => Service().BuyOrder(order, buyer, buyerBase, sellerBase, null, Now.AddMinutes(2)));
            buyerBase.PlanetSector.PlanetId = 2;
            var second = Service().CreateOrder(seller, sellerBase, TradeResourceType.Naquadah, 10, TradeResourceType.Trinium, 10, Now.AddHours(1), Now);
            Assert.Throws<InvalidOperationException>(() => Service().BuyOrder(second, buyer, buyerBase, sellerBase, null, Now.AddMinutes(3)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreateOrder_RejectsInvalidAmounts(int amount)
        {
            var seller = User(1, "SGC"); var sellerBase = Base(1, seller, 1);
            Assert.Throws<InvalidOperationException>(() => Service().CreateOrder(seller, sellerBase, TradeResourceType.Naquadah, amount, TradeResourceType.Trinium, 50, Now.AddHours(1), Now));
        }

        private static PlanetMarketService Service() => new PlanetMarketService();
        private static User User(int id, string faction) => new User { Id = id, UserName = "u" + id, Faction = new Faction { ShortName = faction } };
        private static PlayerBase Base(int id, User user, int planetId) => new PlayerBase { Id = id, UserId = user.Id, User = user, Faction = user.Faction, Resources = new ResourceStock { Naquadah = 500, Trinium = 500, Supplies = 500, Intel = 20 }, PlanetSector = new PlanetSector { PlanetId = planetId } };
    }
}
