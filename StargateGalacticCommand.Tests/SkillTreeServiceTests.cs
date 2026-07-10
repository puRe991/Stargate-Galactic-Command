using System;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class SkillTreeServiceTests
    {
        [Fact]
        public void GetOrCreate_ReturnsExistingWhenPresent()
        {
            var service = new SkillTreeService();
            var user = new User { Id = 1 };
            var existing = new CharacterSkills { UserId = 1, MilitaryLevel = 3 };

            var result = service.GetOrCreate(existing, user);

            Assert.Same(existing, result);
        }

        [Fact]
        public void GetOrCreate_CreatesNewForUserWhenMissing()
        {
            var service = new SkillTreeService();
            var user = new User { Id = 5 };

            var result = service.GetOrCreate(null, user);

            Assert.Equal(5, result.UserId);
            Assert.Equal(0, result.UnspentPoints);
        }

        [Fact]
        public void AwardMissionPoint_IncrementsUnspentPoints()
        {
            var service = new SkillTreeService();
            var skills = new CharacterSkills { UserId = 1, UnspentPoints = 2 };

            service.AwardMissionPoint(skills);

            Assert.Equal(3, skills.UnspentPoints);
        }

        [Fact]
        public void InvestPoint_ThrowsWhenNoUnspentPoints()
        {
            var service = new SkillTreeService();
            var skills = new CharacterSkills { UserId = 1, UnspentPoints = 0 };

            Assert.Throws<InvalidOperationException>(() => service.InvestPoint(skills, SkillTrack.Military));
        }

        [Fact]
        public void InvestPoint_ThrowsWhenTrackIsAtMaxLevel()
        {
            var service = new SkillTreeService();
            var skills = new CharacterSkills { UserId = 1, UnspentPoints = 1, MilitaryLevel = SkillTreeService.MaxLevelPerTrack };

            Assert.Throws<InvalidOperationException>(() => service.InvestPoint(skills, SkillTrack.Military));
        }

        [Theory]
        [InlineData(SkillTrack.Military)]
        [InlineData(SkillTrack.Science)]
        [InlineData(SkillTrack.Diplomacy)]
        public void InvestPoint_IncreasesTheChosenTrackAndConsumesAPoint(SkillTrack track)
        {
            var service = new SkillTreeService();
            var skills = new CharacterSkills { UserId = 1, UnspentPoints = 2 };

            service.InvestPoint(skills, track);

            Assert.Equal(1, skills.UnspentPoints);
            Assert.Equal(1, service.GetLevel(skills, track));
        }

        [Fact]
        public void GetGateMissionScoreBonus_ReturnsZeroWhenSkillsIsNull()
        {
            var service = new SkillTreeService();
            Assert.Equal(0, service.GetGateMissionScoreBonus(null, GateMissionType.RiskAnalysis));
        }

        [Theory]
        [InlineData(GateMissionType.RiskAnalysis, 3, 0, 0, 3)]
        [InlineData(GateMissionType.SearchArtifact, 0, 4, 0, 4)]
        [InlineData(GateMissionType.DiplomaticContact, 0, 0, 5, 5)]
        [InlineData(GateMissionType.SecureResources, 3, 4, 5, 0)]
        public void GetGateMissionScoreBonus_MatchesTheCorrespondingTrackOnly(GateMissionType type, int military, int science, int diplomacy, int expected)
        {
            var service = new SkillTreeService();
            var skills = new CharacterSkills { MilitaryLevel = military, ScienceLevel = science, DiplomacyLevel = diplomacy };

            Assert.Equal(expected, service.GetGateMissionScoreBonus(skills, type));
        }

        [Fact]
        public void GetResearchSpeedMultiplier_ReturnsOneWhenSkillsIsNull()
        {
            var service = new SkillTreeService();
            Assert.Equal(1.0, service.GetResearchSpeedMultiplier(null));
        }

        [Fact]
        public void GetResearchSpeedMultiplier_ScalesWithScienceLevel()
        {
            var service = new SkillTreeService();
            var skills = new CharacterSkills { ScienceLevel = 10 };

            Assert.Equal(1.0 + 10 * SkillTreeService.ResearchSpeedBonusPerLevel, service.GetResearchSpeedMultiplier(skills));
        }
    }
}
