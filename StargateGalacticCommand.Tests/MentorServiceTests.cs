using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class MentorServiceTests
    {
        private static readonly DateTime Now = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        private static AllianceMember Member(int userId, int allianceId, DateTime joinedAtUtc)
        {
            return new AllianceMember { Id = userId, UserId = userId, AllianceId = allianceId, JoinedAtUtc = joinedAtUtc };
        }

        private static PlayerBase Base()
        {
            return new PlayerBase { Id = 1, Resources = new ResourceStock() };
        }

        [Fact]
        public void AssignMentor_ThrowsWhenDifferentAlliances()
        {
            var service = new MentorService();
            var mentee = Member(1, 1, Now);
            var mentor = Member(2, 2, Now.AddDays(-30));
            Assert.Throws<InvalidOperationException>(() => service.AssignMentor(mentee, mentor, Now));
        }

        [Fact]
        public void AssignMentor_ThrowsWhenSelfMentoring()
        {
            var service = new MentorService();
            var mentee = Member(1, 1, Now);
            Assert.Throws<InvalidOperationException>(() => service.AssignMentor(mentee, mentee, Now));
        }

        [Fact]
        public void AssignMentor_ThrowsWhenAlreadyHasMentor()
        {
            var service = new MentorService();
            var mentee = Member(1, 1, Now);
            mentee.MentorUserId = 99;
            var mentor = Member(2, 1, Now.AddDays(-30));
            Assert.Throws<InvalidOperationException>(() => service.AssignMentor(mentee, mentor, Now));
        }

        [Fact]
        public void AssignMentor_ThrowsOutsideMentorshipWindow()
        {
            var service = new MentorService();
            var mentee = Member(1, 1, Now.AddDays(-(MentorService.MentorshipWindowDays + 1)));
            var mentor = Member(2, 1, Now.AddDays(-60));
            Assert.Throws<InvalidOperationException>(() => service.AssignMentor(mentee, mentor, Now));
        }

        [Fact]
        public void AssignMentor_SetsMentorUserIdWhenValid()
        {
            var service = new MentorService();
            var mentee = Member(1, 1, Now);
            var mentor = Member(2, 1, Now.AddDays(-60));

            service.AssignMentor(mentee, mentor, Now);

            Assert.Equal(2, mentee.MentorUserId);
        }

        [Fact]
        public void TryGrantGateMissionMilestone_ReturnsFalseWithoutMentor()
        {
            var service = new MentorService();
            var mentee = Member(1, 1, Now);
            Assert.False(service.TryGrantGateMissionMilestone(mentee, Base(), true, Now));
        }

        [Fact]
        public void TryGrantGateMissionMilestone_ReturnsFalseWhenMilestoneNotReached()
        {
            var service = new MentorService();
            var mentee = Member(1, 1, Now);
            mentee.MentorUserId = 2;
            Assert.False(service.TryGrantGateMissionMilestone(mentee, Base(), false, Now));
        }

        [Fact]
        public void TryGrantGateMissionMilestone_ReturnsFalseOutsideWindow()
        {
            var service = new MentorService();
            var mentee = Member(1, 1, Now.AddDays(-(MentorService.MentorshipWindowDays + 1)));
            mentee.MentorUserId = 2;
            Assert.False(service.TryGrantGateMissionMilestone(mentee, Base(), true, Now));
        }

        [Fact]
        public void TryGrantGateMissionMilestone_GrantsRewardOnceAndMarksTimestamp()
        {
            var service = new MentorService();
            var mentee = Member(1, 1, Now);
            mentee.MentorUserId = 2;
            var mentorBase = Base();

            bool granted = service.TryGrantGateMissionMilestone(mentee, mentorBase, true, Now);

            Assert.True(granted);
            Assert.Equal(MentorService.GateMissionMilestoneReward.Naquadah, mentorBase.Resources.Naquadah);
            Assert.NotNull(mentee.MentorMissionRewardGrantedAtUtc);

            bool grantedAgain = service.TryGrantGateMissionMilestone(mentee, mentorBase, true, Now.AddHours(1));
            Assert.False(grantedAgain);
            Assert.Equal(MentorService.GateMissionMilestoneReward.Naquadah, mentorBase.Resources.Naquadah);
        }

        [Fact]
        public void TryGrantSectorMilestone_GrantsRewardOnceAndMarksTimestamp()
        {
            var service = new MentorService();
            var mentee = Member(1, 1, Now);
            mentee.MentorUserId = 2;
            var mentorBase = Base();

            bool granted = service.TryGrantSectorMilestone(mentee, mentorBase, true, Now);

            Assert.True(granted);
            Assert.Equal(MentorService.SectorMilestoneReward.Naquadah, mentorBase.Resources.Naquadah);
            Assert.NotNull(mentee.MentorSectorRewardGrantedAtUtc);

            bool grantedAgain = service.TryGrantSectorMilestone(mentee, mentorBase, true, Now.AddHours(1));
            Assert.False(grantedAgain);
        }

        [Fact]
        public void BothMilestones_CanBeGrantedIndependently()
        {
            var service = new MentorService();
            var mentee = Member(1, 1, Now);
            mentee.MentorUserId = 2;
            var mentorBase = Base();

            service.TryGrantGateMissionMilestone(mentee, mentorBase, true, Now);
            service.TryGrantSectorMilestone(mentee, mentorBase, true, Now);

            Assert.Equal(MentorService.GateMissionMilestoneReward.Naquadah + MentorService.SectorMilestoneReward.Naquadah, mentorBase.Resources.Naquadah);
        }
    }
}
