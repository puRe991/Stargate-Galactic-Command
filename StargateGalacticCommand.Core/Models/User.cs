using System;
using System.Collections.Generic;

namespace StargateGalacticCommand.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public int FactionId { get; set; }
        public Faction Faction { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public bool IsNpc { get; set; }
        public DateTime? LastSeenAtUtc { get; set; }
        public ICollection<PlayerBase> Bases { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ResearchLevels ResearchLevels { get; set; }
        public ICollection<ResearchQueueItem> ResearchQueue { get; set; }
        public ICollection<KnownGateAddress> KnownGateAddresses { get; set; }
        public ICollection<MissionTeam> MissionTeams { get; set; }
        public ICollection<GateMission> GateMissions { get; set; }
        public ICollection<GateMissionReport> GateMissionReports { get; set; }
        public User() { Bases = new List<PlayerBase>(); Reports = new List<Report>(); ResearchQueue = new List<ResearchQueueItem>(); KnownGateAddresses = new List<KnownGateAddress>(); MissionTeams = new List<MissionTeam>(); GateMissions = new List<GateMission>(); GateMissionReports = new List<GateMissionReport>(); }
    }
}
