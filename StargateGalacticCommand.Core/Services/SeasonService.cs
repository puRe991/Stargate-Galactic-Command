using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class SeasonService
    {
        public const int FocusBucketModulus = 60;
        public const double FocusRewardMultiplier = 1.5;
        private static readonly DateTime Epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // A rotating "focus" bucket of addresses gets a reward bonus for the week, instead of gating the other 300+ addresses behind a schedule (see balancing note: no exclusive power advantage, no permanent FOMO).
        public int GetWeekIndex(DateTime nowUtc)
        {
            int daysSinceMonday = ((int)nowUtc.DayOfWeek + 6) % 7;
            var weekStart = nowUtc.Date.AddDays(-daysSinceMonday);
            long weeksSinceEpoch = (long)(weekStart - Epoch).TotalDays / 7;
            return (int)(((weeksSinceEpoch % FocusBucketModulus) + FocusBucketModulus) % FocusBucketModulus);
        }

        public bool IsFocusAddress(int gateAddressId, int weekIndex)
        {
            return ((gateAddressId % FocusBucketModulus) + FocusBucketModulus) % FocusBucketModulus == weekIndex;
        }

        public string GetSeasonLabel(int weekIndex)
        {
            return "Fokuswoche " + (weekIndex + 1) + "/" + FocusBucketModulus;
        }

        public double GetRewardMultiplier(GateAddress address, int weekIndex)
        {
            return address != null && IsFocusAddress(address.Id, weekIndex) ? FocusRewardMultiplier : 1.0;
        }
    }
}
