using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class PlanetMarketService
    {
        public const double BaseMarketFeeRate = 0.02;
        public const double LucianAllianceFeeReduction = 0.25;
        public const double TradingPostFeeReduction = 0.05;
        public const int MaxIntelTradeAmount = 25;

        public TradeTaxRule CreateDefaultTaxRule()
        {
            return new TradeTaxRule { BaseFeeRate = BaseMarketFeeRate, LucianAllianceReduction = LucianAllianceFeeReduction, TradingPostReduction = TradingPostFeeReduction, MaxIntelAmount = MaxIntelTradeAmount };
        }

        public PlanetMarketOrder CreateOrder(User seller, PlayerBase sellerBase, TradeResourceType offeredResource, int offeredAmount, TradeResourceType requestedResource, int requestedAmount, DateTime expiresAtUtc, DateTime nowUtc)
        {
            if (seller == null) throw new ArgumentNullException("seller");
            if (sellerBase == null) throw new ArgumentNullException("sellerBase");
            if (sellerBase.Resources == null) throw new ArgumentException("Basis hat keinen Ressourcenbestand.", "sellerBase");
            if (sellerBase.PlanetSector == null) throw new ArgumentException("Basis hat keinen Planetensektor.", "sellerBase");
            ValidateTradable(offeredResource, offeredAmount, "Angebot");
            ValidateTradable(requestedResource, requestedAmount, "Gegenforderung");
            if (expiresAtUtc <= nowUtc) throw new InvalidOperationException("Ablaufzeit muss in der Zukunft liegen.");
            if (!HasEnough(sellerBase.Resources, offeredResource, offeredAmount)) throw new InvalidOperationException("Nicht genug Ressourcen für dieses Angebot.");

            Add(sellerBase.Resources, offeredResource, -offeredAmount);
            return new PlanetMarketOrder
            {
                PlanetId = sellerBase.PlanetSector.PlanetId,
                SellerUserId = seller.Id,
                SellerBaseId = sellerBase.Id,
                OfferedResource = offeredResource,
                OfferedAmount = offeredAmount,
                RequestedResource = requestedResource,
                RequestedAmount = requestedAmount,
                CreatedAtUtc = nowUtc,
                ExpiresAtUtc = expiresAtUtc,
                ReservedReturned = false
            };
        }

        public PlanetMarketTransaction BuyOrder(PlanetMarketOrder order, User buyer, PlayerBase buyerBase, PlayerBase sellerBase, IEnumerable<PlanetSector> sellerControlledSectors, DateTime nowUtc, double alliancePactFeeReduction = 0.0, double sellerResearchFeeReduction = 0.0)
        {
            if (order == null) throw new ArgumentNullException("order");
            if (buyer == null) throw new ArgumentNullException("buyer");
            if (buyerBase == null) throw new ArgumentNullException("buyerBase");
            if (sellerBase == null) throw new ArgumentNullException("sellerBase");
            if (buyerBase.Resources == null || sellerBase.Resources == null) throw new ArgumentException("Handel benötigt Ressourcenbestände.");
            if (buyerBase.PlanetSector == null || sellerBase.PlanetSector == null) throw new ArgumentException("Handel benötigt Planetensektoren.");
            if (!order.IsOpen) throw new InvalidOperationException("Dieses Angebot ist nicht mehr aktiv.");
            if (nowUtc >= order.ExpiresAtUtc) throw new InvalidOperationException("Dieses Angebot ist abgelaufen.");
            if (order.SellerUserId == buyer.Id) throw new InvalidOperationException("Eigene Angebote können nicht gekauft werden.");
            if (buyerBase.PlanetSector.PlanetId != order.PlanetId || sellerBase.PlanetSector.PlanetId != order.PlanetId) throw new InvalidOperationException("Handel ist nur auf demselben Planeten möglich.");
            if (!HasEnough(buyerBase.Resources, order.RequestedResource, order.RequestedAmount)) throw new InvalidOperationException("Nicht genug Ressourcen für den Kauf.");

            var feeRate = CalculateFeeRate(order.SellerUser == null ? sellerBase.Faction : order.SellerUser.Faction, sellerControlledSectors, alliancePactFeeReduction + sellerResearchFeeReduction);
            var fee = CalculateFee(order.RequestedAmount, feeRate);

            Add(buyerBase.Resources, order.RequestedResource, -order.RequestedAmount);
            Add(buyerBase.Resources, order.OfferedResource, order.OfferedAmount);
            Add(sellerBase.Resources, order.RequestedResource, order.RequestedAmount - fee);
            order.CompletedAtUtc = nowUtc;

            return new PlanetMarketTransaction
            {
                PlanetMarketOrderId = order.Id,
                PlanetId = order.PlanetId,
                SellerUserId = order.SellerUserId,
                BuyerUserId = buyer.Id,
                OfferedResource = order.OfferedResource,
                OfferedAmount = order.OfferedAmount,
                RequestedResource = order.RequestedResource,
                RequestedAmount = order.RequestedAmount,
                FeeAmount = fee,
                FeeRate = feeRate,
                CreatedAtUtc = nowUtc
            };
        }

        public void CancelOrder(PlanetMarketOrder order, int userId, ResourceStock sellerResources, DateTime nowUtc)
        {
            if (order == null) throw new ArgumentNullException("order");
            if (sellerResources == null) throw new ArgumentNullException("sellerResources");
            if (order.SellerUserId != userId) throw new InvalidOperationException("Nur eigene Angebote dürfen storniert werden.");
            if (!order.IsOpen) throw new InvalidOperationException("Dieses Angebot ist nicht mehr aktiv.");
            ReturnReserved(order, sellerResources);
            order.CancelledAtUtc = nowUtc;
        }

        public bool ExpireOrder(PlanetMarketOrder order, ResourceStock sellerResources, DateTime nowUtc)
        {
            if (order == null) throw new ArgumentNullException("order");
            if (sellerResources == null) throw new ArgumentNullException("sellerResources");
            if (!order.IsOpen || nowUtc < order.ExpiresAtUtc) return false;
            ReturnReserved(order, sellerResources);
            return true;
        }

        public double CalculateFeeRate(Faction sellerFaction, IEnumerable<PlanetSector> sellerControlledSectors, double alliancePactFeeReduction = 0.0)
        {
            var rate = BaseMarketFeeRate;
            if (sellerFaction != null && sellerFaction.ShortName == "Lucian") rate *= 1 - LucianAllianceFeeReduction;
            if (sellerControlledSectors != null && sellerControlledSectors.Any(s => s != null && s.SectorType == SectorType.TradingPost)) rate *= 1 - TradingPostFeeReduction;
            if (alliancePactFeeReduction > 0) rate *= 1 - Math.Min(1, alliancePactFeeReduction);
            return Math.Max(0, rate);
        }

        public int CalculateFee(int amount, double feeRate)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException("amount");
            return Math.Max(1, (int)Math.Ceiling(amount * feeRate));
        }

        private void ReturnReserved(PlanetMarketOrder order, ResourceStock sellerResources)
        {
            if (order.ReservedReturned) return;
            Add(sellerResources, order.OfferedResource, order.OfferedAmount);
            order.ReservedReturned = true;
        }

        private static void ValidateTradable(TradeResourceType resource, int amount, string label)
        {
            if (amount <= 0) throw new InvalidOperationException(label + " muss größer als 0 sein.");
            if (resource == TradeResourceType.Intel && amount > MaxIntelTradeAmount) throw new InvalidOperationException("Intel ist pro Angebot auf " + MaxIntelTradeAmount + " begrenzt.");
            if (!Enum.IsDefined(typeof(TradeResourceType), resource)) throw new InvalidOperationException("Diese Ressource ist nicht handelbar.");
        }

        private static bool HasEnough(ResourceStock stock, TradeResourceType type, int amount) { return Get(stock, type) >= amount; }
        private static int Get(ResourceStock stock, TradeResourceType type)
        {
            switch (type)
            {
                case TradeResourceType.Naquadah: return stock.Naquadah;
                case TradeResourceType.Trinium: return stock.Trinium;
                case TradeResourceType.Supplies: return stock.Supplies;
                case TradeResourceType.Intel: return stock.Intel;
                default: throw new InvalidOperationException("Diese Ressource ist nicht handelbar.");
            }
        }
        private static void Add(ResourceStock stock, TradeResourceType type, int amount)
        {
            switch (type)
            {
                case TradeResourceType.Naquadah: stock.Naquadah += amount; break;
                case TradeResourceType.Trinium: stock.Trinium += amount; break;
                case TradeResourceType.Supplies: stock.Supplies += amount; break;
                case TradeResourceType.Intel: stock.Intel += amount; break;
                default: throw new InvalidOperationException("Diese Ressource ist nicht handelbar.");
            }
        }
    }
}
