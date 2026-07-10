using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class RankingService
    {
        public int CalculateBaseScore(PlayerBase playerBase)
        {
            if (playerBase == null) return 0;
            int resourceScore = playerBase.Resources.Naquadah + playerBase.Resources.Trinium + playerBase.Resources.Supplies + playerBase.Resources.Energy + playerBase.Resources.Personnel;
            var s = playerBase.Ships;
            int shipScore = s.F302 * 100 + s.SmallTransporter * 50 + s.SupplyShuttle * 45 + s.Teltak * 60 + s.AlkeshLightBomber * 90 + s.JaffaTransporter * 60 + s.CloakedTeltak * 70 + s.AgentTransporter * 45 + s.SmugglerTransporter * 50 + s.PirateFighter * 80;
            var b = playerBase.BuildingLevels;
            int buildingScore = (b.CommandCenter + b.NaquadahRefinery + b.TriniumMine + b.SupplyDepot + b.EnergyGenerator + b.ResearchLab + b.GateControlRoom + b.SensorStation + b.DefenseRing + b.HangarLandingZone) * 150;
            return resourceScore + shipScore + buildingScore;
        }

        public int CalculateUserScore(IEnumerable<PlayerBase> userBases)
        {
            return userBases?.Sum(CalculateBaseScore) ?? 0;
        }

        public IList<PlayerRankingEntry> BuildPlayerRankings(IEnumerable<PlayerBase> allBases, IDictionary<int, string> allianceTagByUserId, TimeSpan onlineWindow, DateTime now)
        {
            if (allBases == null) return new List<PlayerRankingEntry>();
            return allBases
                .GroupBy(b => b.UserId)
                .Select(g => new PlayerRankingEntry
                {
                    UserId = g.Key,
                    UserName = g.First().User.UserName,
                    FactionShortName = g.First().User.Faction?.ShortName,
                    AllianceTag = allianceTagByUserId != null && allianceTagByUserId.TryGetValue(g.Key, out var tag) ? tag : null,
                    BaseCount = g.Count(),
                    Score = g.Sum(CalculateBaseScore),
                    IsOnline = g.First().User.LastSeenAtUtc.HasValue && g.First().User.LastSeenAtUtc.Value >= now - onlineWindow,
                    AscensionCount = g.First().User.AscensionCount
                })
                .OrderByDescending(e => e.Score)
                .ToList();
        }

        public IList<AllianceRankingEntry> BuildAllianceRankings(IEnumerable<AllianceMember> members, IDictionary<int, int> scoreByUserId)
        {
            if (members == null) return new List<AllianceRankingEntry>();
            return members
                .GroupBy(m => m.AllianceId)
                .Select(g => new AllianceRankingEntry
                {
                    AllianceId = g.Key,
                    Name = g.First().Alliance.Name,
                    Tag = g.First().Alliance.Tag,
                    MemberCount = g.Count(),
                    TotalScore = g.Sum(m => scoreByUserId != null && scoreByUserId.TryGetValue(m.UserId, out var s) ? s : 0)
                })
                .OrderByDescending(a => a.TotalScore)
                .ToList();
        }
    }
}
