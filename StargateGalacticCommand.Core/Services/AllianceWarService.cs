using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class AllianceWarService
    {
        public const int RequiredHours = 24;
        public const double RequiredSectorShareOfMembers = 0.5;
        public const int MinRequiredSectors = 2;

        public static readonly BuildCost VictoryRewardPerMember = new BuildCost { Naquadah = 300, Trinium = 200, Supplies = 150, Energy = 100, Personnel = 20, Intel = 10 };

        public int CalculateRequiredSectors(int allianceMemberCount)
        {
            return Math.Max(MinRequiredSectors, (int)Math.Ceiling(allianceMemberCount * RequiredSectorShareOfMembers));
        }

        public AllianceWarGoal Declare(Alliance alliance, Planet planet, int memberCount, bool hasActiveGoal, DateTime nowUtc)
        {
            if (alliance == null) throw new ArgumentNullException("alliance");
            if (planet == null) throw new ArgumentNullException("planet");
            if (hasActiveGoal) throw new InvalidOperationException("Diese Allianz verfolgt bereits ein aktives Kriegsziel.");
            return new AllianceWarGoal { AllianceId = alliance.Id, Alliance = alliance, PlanetId = planet.Id, Planet = planet, RequiredSectors = CalculateRequiredSectors(memberCount), RequiredHours = RequiredHours, StartedAtUtc = nowUtc, Status = AllianceWarGoalStatus.Active };
        }

        public void Abandon(AllianceWarGoal goal, DateTime nowUtc)
        {
            if (goal == null) throw new ArgumentNullException("goal");
            if (goal.Status != AllianceWarGoalStatus.Active) throw new InvalidOperationException("Nur aktive Kriegsziele können aufgegeben werden.");
            goal.Status = AllianceWarGoalStatus.Abandoned;
            goal.EndedAtUtc = nowUtc;
        }

        // Returns true exactly once, the moment continuous control crosses RequiredHours - callers use this to trigger the one-time reward.
        public bool EvaluateProgress(AllianceWarGoal goal, int currentControlledSectors, DateTime nowUtc)
        {
            if (goal == null) throw new ArgumentNullException("goal");
            if (goal.Status != AllianceWarGoalStatus.Active) return false;

            if (currentControlledSectors < goal.RequiredSectors)
            {
                goal.HoldStreakStartedAtUtc = null;
                return false;
            }

            if (!goal.HoldStreakStartedAtUtc.HasValue) goal.HoldStreakStartedAtUtc = nowUtc;

            if ((nowUtc - goal.HoldStreakStartedAtUtc.Value).TotalHours < goal.RequiredHours) return false;

            goal.Status = AllianceWarGoalStatus.Achieved;
            goal.AchievedAtUtc = nowUtc;
            return true;
        }

        public void ApplyVictoryReward(ResourceStock resources)
        {
            if (resources == null) throw new ArgumentNullException("resources");
            resources.Naquadah += VictoryRewardPerMember.Naquadah;
            resources.Trinium += VictoryRewardPerMember.Trinium;
            resources.Supplies += VictoryRewardPerMember.Supplies;
            resources.Energy += VictoryRewardPerMember.Energy;
            resources.Personnel += VictoryRewardPerMember.Personnel;
            resources.Intel += VictoryRewardPerMember.Intel;
        }
    }
}
