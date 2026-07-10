using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class MentorService
    {
        public const int MentorshipWindowDays = 14;
        public static readonly BuildCost GateMissionMilestoneReward = new BuildCost { Naquadah = 200, Trinium = 100, Supplies = 100 };
        public static readonly BuildCost SectorMilestoneReward = new BuildCost { Naquadah = 300, Trinium = 150, Supplies = 150 };

        public void AssignMentor(AllianceMember mentee, AllianceMember mentor, DateTime nowUtc)
        {
            if (mentee == null) throw new ArgumentNullException("mentee");
            if (mentor == null) throw new ArgumentNullException("mentor");
            if (mentee.AllianceId != mentor.AllianceId) throw new InvalidOperationException("Mentor muss Mitglied derselben Allianz sein.");
            if (mentee.UserId == mentor.UserId) throw new InvalidOperationException("Du kannst nicht dein eigener Mentor sein.");
            if (mentee.MentorUserId.HasValue) throw new InvalidOperationException("Es wurde bereits ein Mentor gewählt.");
            if (!IsWithinMentorshipWindow(mentee, nowUtc)) throw new InvalidOperationException("Ein Mentor kann nur innerhalb der ersten " + MentorshipWindowDays + " Tage nach Beitritt gewählt werden.");
            mentee.MentorUserId = mentor.UserId;
        }

        public bool IsWithinMentorshipWindow(AllianceMember mentee, DateTime nowUtc)
        {
            if (mentee == null) throw new ArgumentNullException("mentee");
            return nowUtc <= mentee.JoinedAtUtc.AddDays(MentorshipWindowDays);
        }

        // Belohnung ist an echten Fortschritt des Schützlings gekoppelt (siehe GAMEPLAY_IDEAS 3.2 Balancing-Hinweis), nicht an bloßen Beitritt.
        public bool TryGrantGateMissionMilestone(AllianceMember mentee, PlayerBase mentorBase, bool menteeHasCompletedGateMission, DateTime nowUtc)
        {
            if (mentee == null) throw new ArgumentNullException("mentee");
            if (mentorBase == null) throw new ArgumentNullException("mentorBase");
            if (!mentee.MentorUserId.HasValue || mentee.MentorMissionRewardGrantedAtUtc.HasValue) return false;
            if (!menteeHasCompletedGateMission || !IsWithinMentorshipWindow(mentee, nowUtc)) return false;

            Grant(mentorBase, GateMissionMilestoneReward);
            mentee.MentorMissionRewardGrantedAtUtc = nowUtc;
            return true;
        }

        public bool TryGrantSectorMilestone(AllianceMember mentee, PlayerBase mentorBase, bool menteeControlsSector, DateTime nowUtc)
        {
            if (mentee == null) throw new ArgumentNullException("mentee");
            if (mentorBase == null) throw new ArgumentNullException("mentorBase");
            if (!mentee.MentorUserId.HasValue || mentee.MentorSectorRewardGrantedAtUtc.HasValue) return false;
            if (!menteeControlsSector || !IsWithinMentorshipWindow(mentee, nowUtc)) return false;

            Grant(mentorBase, SectorMilestoneReward);
            mentee.MentorSectorRewardGrantedAtUtc = nowUtc;
            return true;
        }

        private static void Grant(PlayerBase playerBase, BuildCost reward)
        {
            playerBase.Resources.Naquadah += reward.Naquadah;
            playerBase.Resources.Trinium += reward.Trinium;
            playerBase.Resources.Supplies += reward.Supplies;
            playerBase.Resources.Energy += reward.Energy;
            playerBase.Resources.Personnel += reward.Personnel;
            playerBase.Resources.Intel += reward.Intel;
        }
    }
}
