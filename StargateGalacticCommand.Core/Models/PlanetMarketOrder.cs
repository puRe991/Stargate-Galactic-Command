using System;

namespace StargateGalacticCommand.Core.Models
{
    public class PlanetMarketOrder
    {
        public int Id { get; set; }
        public int PlanetId { get; set; }
        public Planet Planet { get; set; }
        public int SellerUserId { get; set; }
        public User SellerUser { get; set; }
        public int SellerBaseId { get; set; }
        public PlayerBase SellerBase { get; set; }
        public TradeResourceType OfferedResource { get; set; }
        public int OfferedAmount { get; set; }
        public TradeResourceType RequestedResource { get; set; }
        public int RequestedAmount { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public DateTime? CancelledAtUtc { get; set; }
        public bool ReservedReturned { get; set; }
        public bool IsOpen { get { return CompletedAtUtc == null && CancelledAtUtc == null && !ReservedReturned; } }
    }
}
