using System;

namespace StargateGalacticCommand.Core.Models
{
    public class TradeReport
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? PlanetMarketOrderId { get; set; }
        public PlanetMarketOrder PlanetMarketOrder { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
