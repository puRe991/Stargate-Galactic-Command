using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class QuestlineServiceTests
    {
        private static readonly DateTime Now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [Theory]
        [InlineData("SGC")]
        [InlineData("Jaffa")]
        [InlineData("Tok’ra")]
        [InlineData("Lucian")]
        public void GetStepsForFaction_ReturnsFourOrderedStepsPerStartFaction(string factionShortName)
        {
            var service = new QuestlineService();
            var steps = service.GetStepsForFaction(factionShortName);

            Assert.Equal(4, steps.Count);
            Assert.Equal(new[] { 1, 2, 3, 4 }, steps.Select(s => s.Order).ToArray());
            Assert.True(steps.Zip(steps.Skip(1), (a, b) => b.RequiredSuccessfulCount > a.RequiredSuccessfulCount).All(x => x));
        }

        [Fact]
        public void GetStepsForFaction_UnknownFactionReturnsEmpty()
        {
            var service = new QuestlineService();
            Assert.Empty(service.GetStepsForFaction("Unknown"));
        }

        [Fact]
        public void DifferentFactions_HaveDistinctNarrativeText()
        {
            var service = new QuestlineService();
            var sgcNarratives = service.GetStepsForFaction("SGC").Select(s => s.Narrative);
            var lucianNarratives = service.GetStepsForFaction("Lucian").Select(s => s.Narrative);
            Assert.Empty(sgcNarratives.Intersect(lucianNarratives));
        }

        [Fact]
        public void IsStepUnlocked_FirstStepIsAlwaysUnlocked()
        {
            var service = new QuestlineService();
            var steps = service.GetStepsForFaction("SGC");
            Assert.True(service.IsStepUnlocked(steps[0], steps, new HashSet<string>()));
        }

        [Fact]
        public void IsStepUnlocked_LaterStepRequiresPreviousCompleted()
        {
            var service = new QuestlineService();
            var steps = service.GetStepsForFaction("SGC");
            Assert.False(service.IsStepUnlocked(steps[1], steps, new HashSet<string>()));
            Assert.True(service.IsStepUnlocked(steps[1], steps, new HashSet<string> { steps[0].Key }));
        }

        [Fact]
        public void IsStepComplete_ComparesAgainstRequiredCount()
        {
            var service = new QuestlineService();
            var step = service.GetStepsForFaction("SGC")[0];
            Assert.False(service.IsStepComplete(step, step.RequiredSuccessfulCount - 1));
            Assert.True(service.IsStepComplete(step, step.RequiredSuccessfulCount));
        }

        [Fact]
        public void TryCompleteStep_ReturnsNullWhenLocked()
        {
            var service = new QuestlineService();
            var step = service.GetStepsForFaction("SGC")[1];
            var user = new User { Id = 1 };
            Assert.Null(service.TryCompleteStep(user, step, false, true, new HashSet<string>(), Now));
        }

        [Fact]
        public void TryCompleteStep_ReturnsNullWhenNotComplete()
        {
            var service = new QuestlineService();
            var step = service.GetStepsForFaction("SGC")[0];
            var user = new User { Id = 1 };
            Assert.Null(service.TryCompleteStep(user, step, true, false, new HashSet<string>(), Now));
        }

        [Fact]
        public void TryCompleteStep_ReturnsNullWhenAlreadyCompleted()
        {
            var service = new QuestlineService();
            var step = service.GetStepsForFaction("SGC")[0];
            var user = new User { Id = 1 };
            Assert.Null(service.TryCompleteStep(user, step, true, true, new HashSet<string> { step.Key }, Now));
        }

        [Fact]
        public void TryCompleteStep_ReturnsProgressWhenUnlockedAndComplete()
        {
            var service = new QuestlineService();
            var step = service.GetStepsForFaction("SGC")[0];
            var user = new User { Id = 7 };

            var progress = service.TryCompleteStep(user, step, true, true, new HashSet<string>(), Now);

            Assert.NotNull(progress);
            Assert.Equal(7, progress.UserId);
            Assert.Equal(step.Key, progress.StepKey);
            Assert.Equal(Now, progress.CompletedAtUtc);
        }
    }
}
