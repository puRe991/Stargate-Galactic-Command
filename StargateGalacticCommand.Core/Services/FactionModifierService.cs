using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class FactionModifierService
    {
        public double GetResearchSpeedMultiplier(Faction faction)
        {
            return IsShortName(faction, "SGC") ? 1.05 : 1.0;
        }

        public double GetIntelProductionMultiplier(Faction faction)
        {
            return IsShortName(faction, "Tok’ra") ? 1.10 : 1.0;
        }

        public double GetSuppliesProductionMultiplier(Faction faction)
        {
            return IsShortName(faction, "Lucian") ? 1.05 : 1.0;
        }

        public double GetDefenseMultiplier(Faction faction)
        {
            return IsShortName(faction, "Jaffa") ? 1.05 : 1.0;
        }

        private static bool IsShortName(Faction faction, string shortName)
        {
            return faction != null && string.Equals(faction.ShortName, shortName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
