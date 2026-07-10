using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class DiplomacyServiceTests
    {
        private static readonly DateTime Now = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        private static Alliance A(int id) => new Alliance { Id = id, Name = "Alliance " + id, Tag = "A" + id };

        [Fact]
        public void ProposePact_ThrowsForSelfProposal()
        {
            var service = new DiplomacyService();
            var alliance = A(1);
            Assert.Throws<InvalidOperationException>(() => service.ProposePact(alliance, alliance, null, Now));
        }

        [Fact]
        public void ProposePact_CreatesProposedStatusWithOrderedIds()
        {
            var service = new DiplomacyService();
            var status = service.ProposePact(A(5), A(2), null, Now);

            Assert.Equal(2, status.AllianceAId);
            Assert.Equal(5, status.AllianceBId);
            Assert.Equal(AllianceDiplomacyStatusType.Proposed, status.Status);
            Assert.Equal(5, status.ProposedByAllianceId);
        }

        [Fact]
        public void ProposePact_ThrowsWhenAlreadyPact()
        {
            var service = new DiplomacyService();
            var existing = new AllianceDiplomacyStatus { AllianceAId = 1, AllianceBId = 2, Status = AllianceDiplomacyStatusType.Pact };
            Assert.Throws<InvalidOperationException>(() => service.ProposePact(A(1), A(2), existing, Now));
        }

        [Fact]
        public void ProposePact_ThrowsWhenAlreadyProposed()
        {
            var service = new DiplomacyService();
            var existing = new AllianceDiplomacyStatus { AllianceAId = 1, AllianceBId = 2, Status = AllianceDiplomacyStatusType.Proposed };
            Assert.Throws<InvalidOperationException>(() => service.ProposePact(A(1), A(2), existing, Now));
        }

        [Fact]
        public void ProposePact_ThrowsDuringBreakCooldownForTheBreaker()
        {
            var service = new DiplomacyService();
            var existing = new AllianceDiplomacyStatus { AllianceAId = 1, AllianceBId = 2, Status = AllianceDiplomacyStatusType.War, BrokenByAllianceId = 1, LastBrokenAtUtc = Now.AddDays(-1) };
            Assert.Throws<InvalidOperationException>(() => service.ProposePact(A(1), A(2), existing, Now));
        }

        [Fact]
        public void ProposePact_AllowedAfterCooldownElapsed()
        {
            var service = new DiplomacyService();
            var existing = new AllianceDiplomacyStatus { AllianceAId = 1, AllianceBId = 2, Status = AllianceDiplomacyStatusType.War, BrokenByAllianceId = 1, LastBrokenAtUtc = Now.AddDays(-(DiplomacyService.PactBreakCooldownDays + 1)) };
            var status = service.ProposePact(A(1), A(2), existing, Now);
            Assert.Equal(AllianceDiplomacyStatusType.Proposed, status.Status);
        }

        [Fact]
        public void ProposePact_AllowedForTheNonBreakingAllianceDuringCooldown()
        {
            var service = new DiplomacyService();
            var existing = new AllianceDiplomacyStatus { AllianceAId = 1, AllianceBId = 2, Status = AllianceDiplomacyStatusType.War, BrokenByAllianceId = 1, LastBrokenAtUtc = Now.AddDays(-1) };
            var status = service.ProposePact(A(2), A(1), existing, Now);
            Assert.Equal(AllianceDiplomacyStatusType.Proposed, status.Status);
        }

        [Fact]
        public void AcceptPact_ThrowsWhenNotProposed()
        {
            var service = new DiplomacyService();
            var status = new AllianceDiplomacyStatus { AllianceAId = 1, AllianceBId = 2, Status = AllianceDiplomacyStatusType.Pact };
            Assert.Throws<InvalidOperationException>(() => service.AcceptPact(status, A(2), Now));
        }

        [Fact]
        public void AcceptPact_ThrowsWhenProposerAcceptsOwnProposal()
        {
            var service = new DiplomacyService();
            var status = new AllianceDiplomacyStatus { AllianceAId = 1, AllianceBId = 2, Status = AllianceDiplomacyStatusType.Proposed, ProposedByAllianceId = 1 };
            Assert.Throws<InvalidOperationException>(() => service.AcceptPact(status, A(1), Now));
        }

        [Fact]
        public void AcceptPact_ThrowsWhenAccepterNotInvolved()
        {
            var service = new DiplomacyService();
            var status = new AllianceDiplomacyStatus { AllianceAId = 1, AllianceBId = 2, Status = AllianceDiplomacyStatusType.Proposed, ProposedByAllianceId = 1 };
            Assert.Throws<InvalidOperationException>(() => service.AcceptPact(status, A(99), Now));
        }

        [Fact]
        public void AcceptPact_SetsStatusToPact()
        {
            var service = new DiplomacyService();
            var status = new AllianceDiplomacyStatus { AllianceAId = 1, AllianceBId = 2, Status = AllianceDiplomacyStatusType.Proposed, ProposedByAllianceId = 1 };
            service.AcceptPact(status, A(2), Now);
            Assert.Equal(AllianceDiplomacyStatusType.Pact, status.Status);
        }

        [Fact]
        public void DeclareWar_FromPact_MarksBreaker()
        {
            var service = new DiplomacyService();
            var existing = new AllianceDiplomacyStatus { AllianceAId = 1, AllianceBId = 2, Status = AllianceDiplomacyStatusType.Pact };
            var status = service.DeclareWar(A(1), A(2), existing, Now);
            Assert.Equal(AllianceDiplomacyStatusType.War, status.Status);
            Assert.Equal(1, status.BrokenByAllianceId);
            Assert.Equal(Now, status.LastBrokenAtUtc);
        }

        [Fact]
        public void DeclareWar_FromNeutral_DoesNotMarkBreaker()
        {
            var service = new DiplomacyService();
            var status = service.DeclareWar(A(1), A(2), null, Now);
            Assert.Equal(AllianceDiplomacyStatusType.War, status.Status);
            Assert.Null(status.BrokenByAllianceId);
        }

        [Fact]
        public void DeclareWar_ThrowsWhenAlreadyAtWar()
        {
            var service = new DiplomacyService();
            var existing = new AllianceDiplomacyStatus { AllianceAId = 1, AllianceBId = 2, Status = AllianceDiplomacyStatusType.War };
            Assert.Throws<InvalidOperationException>(() => service.DeclareWar(A(1), A(2), existing, Now));
        }

        [Fact]
        public void IsPactActive_TrueOnlyForPactStatus()
        {
            var service = new DiplomacyService();
            Assert.True(service.IsPactActive(new AllianceDiplomacyStatus { Status = AllianceDiplomacyStatusType.Pact }));
            Assert.False(service.IsPactActive(new AllianceDiplomacyStatus { Status = AllianceDiplomacyStatusType.War }));
            Assert.False(service.IsPactActive(null));
        }

        [Fact]
        public void GetMarketFeeReduction_ReturnsReductionOnlyDuringPact()
        {
            var service = new DiplomacyService();
            Assert.Equal(DiplomacyService.PactMarketFeeReduction, service.GetMarketFeeReduction(new AllianceDiplomacyStatus { Status = AllianceDiplomacyStatusType.Pact }));
            Assert.Equal(0.0, service.GetMarketFeeReduction(new AllianceDiplomacyStatus { Status = AllianceDiplomacyStatusType.Proposed }));
        }
    }
}
