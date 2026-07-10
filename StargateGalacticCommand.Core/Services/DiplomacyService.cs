using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class DiplomacyService
    {
        public const int PactBreakCooldownDays = 7;
        public const double PactMarketFeeReduction = 0.10;

        public (int LowId, int HighId) OrderPair(int allianceAId, int allianceBId)
        {
            return allianceAId <= allianceBId ? (allianceAId, allianceBId) : (allianceBId, allianceAId);
        }

        public AllianceDiplomacyStatus ProposePact(Alliance proposer, Alliance target, AllianceDiplomacyStatus existing, DateTime nowUtc)
        {
            if (proposer == null) throw new ArgumentNullException("proposer");
            if (target == null) throw new ArgumentNullException("target");
            if (proposer.Id == target.Id) throw new InvalidOperationException("Eine Allianz kann sich nicht selbst einen Pakt vorschlagen.");

            if (existing != null)
            {
                if (existing.Status == AllianceDiplomacyStatusType.Pact) throw new InvalidOperationException("Es besteht bereits ein Pakt zwischen diesen Allianzen.");
                if (existing.Status == AllianceDiplomacyStatusType.Proposed) throw new InvalidOperationException("Es liegt bereits ein Paktvorschlag vor.");
                if (existing.BrokenByAllianceId == proposer.Id && existing.LastBrokenAtUtc.HasValue && nowUtc < existing.LastBrokenAtUtc.Value.AddDays(PactBreakCooldownDays))
                    throw new InvalidOperationException("Nach einem Paktbruch muss diese Allianz " + PactBreakCooldownDays + " Tage warten, bevor sie derselben Allianz erneut einen Pakt vorschlagen kann.");

                existing.Status = AllianceDiplomacyStatusType.Proposed;
                existing.ProposedByAllianceId = proposer.Id;
                existing.SinceUtc = nowUtc;
                return existing;
            }

            var (lowId, highId) = OrderPair(proposer.Id, target.Id);
            return new AllianceDiplomacyStatus { AllianceAId = lowId, AllianceBId = highId, Status = AllianceDiplomacyStatusType.Proposed, ProposedByAllianceId = proposer.Id, SinceUtc = nowUtc };
        }

        public void AcceptPact(AllianceDiplomacyStatus status, Alliance accepter, DateTime nowUtc)
        {
            if (status == null) throw new ArgumentNullException("status");
            if (accepter == null) throw new ArgumentNullException("accepter");
            if (status.Status != AllianceDiplomacyStatusType.Proposed) throw new InvalidOperationException("Es liegt kein offener Paktvorschlag vor.");
            if (accepter.Id != status.AllianceAId && accepter.Id != status.AllianceBId) throw new InvalidOperationException("Diese Allianz ist an diesem Vorschlag nicht beteiligt.");
            if (accepter.Id == status.ProposedByAllianceId) throw new InvalidOperationException("Der Vorschlag kann nicht von der vorschlagenden Allianz selbst angenommen werden.");

            status.Status = AllianceDiplomacyStatusType.Pact;
            status.SinceUtc = nowUtc;
        }

        public AllianceDiplomacyStatus DeclareWar(Alliance declarer, Alliance target, AllianceDiplomacyStatus existing, DateTime nowUtc)
        {
            if (declarer == null) throw new ArgumentNullException("declarer");
            if (target == null) throw new ArgumentNullException("target");
            if (declarer.Id == target.Id) throw new InvalidOperationException("Eine Allianz kann sich nicht selbst den Krieg erklären.");

            if (existing == null)
            {
                var (lowId, highId) = OrderPair(declarer.Id, target.Id);
                return new AllianceDiplomacyStatus { AllianceAId = lowId, AllianceBId = highId, Status = AllianceDiplomacyStatusType.War, ProposedByAllianceId = declarer.Id, SinceUtc = nowUtc };
            }

            if (existing.Status == AllianceDiplomacyStatusType.War) throw new InvalidOperationException("Es besteht bereits Krieg zwischen diesen Allianzen.");
            if (existing.Status == AllianceDiplomacyStatusType.Pact)
            {
                existing.BrokenByAllianceId = declarer.Id;
                existing.LastBrokenAtUtc = nowUtc;
            }
            existing.Status = AllianceDiplomacyStatusType.War;
            existing.SinceUtc = nowUtc;
            return existing;
        }

        public bool IsPactActive(AllianceDiplomacyStatus status)
        {
            return status != null && status.Status == AllianceDiplomacyStatusType.Pact;
        }

        public double GetMarketFeeReduction(AllianceDiplomacyStatus status)
        {
            return IsPactActive(status) ? PactMarketFeeReduction : 0.0;
        }
    }
}
