using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class SkillTreeService
    {
        public const int MaxLevelPerTrack = 10;
        public const int GateMissionScoreBonusPerLevel = 1;
        public const double ResearchSpeedBonusPerLevel = 0.03;

        public CharacterSkills GetOrCreate(CharacterSkills existing, User user)
        {
            return existing ?? new CharacterSkills { UserId = user.Id, User = user };
        }

        public void AwardMissionPoint(CharacterSkills skills)
        {
            if (skills == null) throw new ArgumentNullException("skills");
            skills.UnspentPoints++;
        }

        public void InvestPoint(CharacterSkills skills, SkillTrack track)
        {
            if (skills == null) throw new ArgumentNullException("skills");
            if (skills.UnspentPoints < 1) throw new InvalidOperationException("Keine freien Skillpunkte verfügbar.");
            int currentLevel = GetLevel(skills, track);
            if (currentLevel >= MaxLevelPerTrack) throw new InvalidOperationException("Diese Fertigkeit hat bereits die Höchststufe erreicht.");

            switch (track)
            {
                case SkillTrack.Military: skills.MilitaryLevel++; break;
                case SkillTrack.Science: skills.ScienceLevel++; break;
                case SkillTrack.Diplomacy: skills.DiplomacyLevel++; break;
            }
            skills.UnspentPoints--;
        }

        public int GetLevel(CharacterSkills skills, SkillTrack track)
        {
            if (skills == null) return 0;
            switch (track)
            {
                case SkillTrack.Military: return skills.MilitaryLevel;
                case SkillTrack.Science: return skills.ScienceLevel;
                case SkillTrack.Diplomacy: return skills.DiplomacyLevel;
                default: return 0;
            }
        }

        // Militär -> Risikoanalyse, Wissenschaft -> Artefaktsuche, Diplomatie -> diplomatischer Kontakt (siehe GAMEPLAY_IDEAS 2.5).
        public int GetGateMissionScoreBonus(CharacterSkills skills, GateMissionType missionType)
        {
            if (skills == null) return 0;
            if (missionType == GateMissionType.RiskAnalysis) return skills.MilitaryLevel * GateMissionScoreBonusPerLevel;
            if (missionType == GateMissionType.SearchArtifact) return skills.ScienceLevel * GateMissionScoreBonusPerLevel;
            if (missionType == GateMissionType.DiplomaticContact) return skills.DiplomacyLevel * GateMissionScoreBonusPerLevel;
            return 0;
        }

        public double GetResearchSpeedMultiplier(CharacterSkills skills)
        {
            if (skills == null) return 1.0;
            return 1.0 + skills.ScienceLevel * ResearchSpeedBonusPerLevel;
        }
    }
}
