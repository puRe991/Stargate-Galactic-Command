using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class SeasonServiceTests
    {
        [Fact]
        public void GetWeekIndex_IsStableWithinTheSameCalendarWeek()
        {
            var service = new SeasonService();
            var monday = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
            var sundayLater = new DateTime(2026, 1, 11, 23, 0, 0, DateTimeKind.Utc);

            Assert.Equal(service.GetWeekIndex(monday), service.GetWeekIndex(sundayLater));
        }

        [Fact]
        public void GetWeekIndex_ChangesForTheFollowingWeek()
        {
            var service = new SeasonService();
            var week1 = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
            var week2 = week1.AddDays(7);

            int index1 = service.GetWeekIndex(week1);
            int index2 = service.GetWeekIndex(week2);

            Assert.NotEqual(index1, index2);
            Assert.Equal((index1 + 1) % SeasonService.FocusBucketModulus, index2);
        }

        [Fact]
        public void GetWeekIndex_IsAlwaysWithinBucketRange()
        {
            var service = new SeasonService();
            var random = new Random(7);
            for (int i = 0; i < 50; i++)
            {
                var date = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(random.Next(0, 4000));
                int index = service.GetWeekIndex(date);
                Assert.InRange(index, 0, SeasonService.FocusBucketModulus - 1);
            }
        }

        [Fact]
        public void IsFocusAddress_OnlyMatchesTheAddressWhoseIdModuloEqualsTheWeekIndex()
        {
            var service = new SeasonService();
            Assert.True(service.IsFocusAddress(5, 5));
            Assert.True(service.IsFocusAddress(65, 5));
            Assert.False(service.IsFocusAddress(6, 5));
        }

        [Fact]
        public void GetRewardMultiplier_ReturnsBonusOnlyForFocusAddress()
        {
            var service = new SeasonService();
            var focus = new GateAddress { Id = 10 };
            var other = new GateAddress { Id = 11 };

            Assert.Equal(SeasonService.FocusRewardMultiplier, service.GetRewardMultiplier(focus, 10));
            Assert.Equal(1.0, service.GetRewardMultiplier(other, 10));
            Assert.Equal(1.0, service.GetRewardMultiplier(null, 10));
        }

        [Fact]
        public void GetSeasonLabel_ContainsOneBasedWeekAndModulus()
        {
            var service = new SeasonService();
            Assert.Equal("Fokuswoche 6/60", service.GetSeasonLabel(5));
        }
    }
}
