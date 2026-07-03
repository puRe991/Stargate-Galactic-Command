using System;

namespace StargateGalacticCommand.Core.Models
{
    public class GateMission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int GateAddressId { get; set; }
        public GateAddress GateAddress { get; set; }
        public int MissionTeamId { get; set; }
        public MissionTeam MissionTeam { get; set; }
        public GateMissionType MissionType { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime CompletesAtUtc { get; set; }
        public bool IsCompleted { get; set; }
    }
}
