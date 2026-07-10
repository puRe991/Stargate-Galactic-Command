using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class AscensionService
    {
        public const int MinScoreToAscend = 15000;
        public const int MinHoursBetweenAscensions = 24;
        public const double BonusPerAscension = 0.03;
        public const int MaxBonusAscensions = 10;

        public double CalculateProductionBonus(int ascensionCount)
        {
            return Math.Min(MaxBonusAscensions, Math.Max(0, ascensionCount)) * BonusPerAscension;
        }

        public void ValidateCanAscend(int currentScore, DateTime? lastAscendedAtUtc, DateTime nowUtc)
        {
            if (currentScore < MinScoreToAscend) throw new InvalidOperationException("Für die Erleuchtung wird ein Basis-Score von mindestens " + MinScoreToAscend + " benötigt (aktuell " + currentScore + ").");
            if (lastAscendedAtUtc.HasValue)
            {
                double hoursSinceLast = (nowUtc - lastAscendedAtUtc.Value).TotalHours;
                if (hoursSinceLast < MinHoursBetweenAscensions) throw new InvalidOperationException("Die nächste Erleuchtung ist erst in " + Math.Ceiling(MinHoursBetweenAscensions - hoursSinceLast) + "h möglich.");
            }
        }

        // Resets power (buildings, research, resources, ships) in exchange for a small permanent production bonus; history (reports, achievements, contracts, alliance membership, known addresses) is deliberately untouched so ascending doesn't feel like data loss.
        public void Ascend(User user, PlayerBase playerBase, ResourceStock startingResources, BuildingLevels startingBuildings, DateTime nowUtc)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            if (playerBase.Resources == null || playerBase.BuildingLevels == null || playerBase.Ships == null || user.ResearchLevels == null) throw new ArgumentException("Basis oder Nutzer ist unvollständig geladen.");
            if (startingResources == null) throw new ArgumentNullException("startingResources");
            if (startingBuildings == null) throw new ArgumentNullException("startingBuildings");

            user.AscensionCount++;
            user.LastAscendedAtUtc = nowUtc;

            foreach (ResearchType type in Enum.GetValues(typeof(ResearchType))) user.ResearchLevels.SetLevel(type, 0);
            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType))) playerBase.BuildingLevels.SetLevel(type, startingBuildings.GetLevel(type));
            foreach (ShipType type in Enum.GetValues(typeof(ShipType)))
            {
                int count = playerBase.Ships.GetCount(type);
                if (count > 0) playerBase.Ships.Remove(type, count);
            }

            playerBase.Resources.Naquadah = startingResources.Naquadah;
            playerBase.Resources.Trinium = startingResources.Trinium;
            playerBase.Resources.Supplies = startingResources.Supplies;
            playerBase.Resources.Energy = startingResources.Energy;
            playerBase.Resources.Personnel = startingResources.Personnel;
            playerBase.Resources.Intel = startingResources.Intel;
            playerBase.LastResourceUpdateUtc = nowUtc;
        }
    }
}
