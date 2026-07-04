using System;

namespace StargateGalacticCommand.Core.Models
{
    public class PlanetMarketTransaction
    {
        public int Id { get; set; }
        public int PlanetMarketOrderId { get; set; }
        public PlanetMarketOrder PlanetMarketOrder { get; set; }
        public int PlanetId { get; set; }
        public Planet Planet { get; set; }
        public int SellerUserId { get; set; }
        public User SellerUser { get; set; }
        public int BuyerUserId { get; set; }
        public User BuyerUser { get; set; }
        public TradeResourceType OfferedResource { get; set; }
        public int OfferedAmount { get; set; }
        public TradeResourceType RequestedResource { get; set; }
        public int RequestedAmount { get; set; }
        public int FeeAmount { get; set; }
        public double FeeRate { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
