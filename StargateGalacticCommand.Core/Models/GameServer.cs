using System;

namespace StargateGalacticCommand.Core.Models
{
    public class GameServer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public ServerStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int GalaxySeed { get; set; }
        public bool IsDefault { get; set; }
    }
}
