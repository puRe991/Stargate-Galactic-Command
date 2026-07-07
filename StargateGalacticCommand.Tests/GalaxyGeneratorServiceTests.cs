using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class GalaxyGeneratorServiceTests
    {
        [Fact]
        public void GenerateWorlds_ReturnsRequestedCountWithUniqueCodes()
        {
            var service = new GalaxyGeneratorService();
            var worlds = service.GenerateWorlds(320, new List<string>(), seed: 1);

            Assert.Equal(320, worlds.Count);
            Assert.Equal(320, worlds.Select(w => w.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count());
            Assert.All(worlds, w => Assert.True(w.IsNeutralPve));
            Assert.All(worlds, w => Assert.Null(w.PlanetId));
            Assert.All(worlds, w => Assert.InRange(w.RiskLevel, 1, 10));
            Assert.All(worlds, w => Assert.False(string.IsNullOrWhiteSpace(w.Description)));
        }

        [Fact]
        public void GenerateWorlds_AvoidsCollisionsWithExistingCodes()
        {
            var service = new GalaxyGeneratorService();
            var firstBatch = service.GenerateWorlds(50, new List<string>(), seed: 7);
            var existingCodes = firstBatch.Select(w => w.Code).ToList();

            var secondBatch = service.GenerateWorlds(50, existingCodes, seed: 7);

            Assert.Empty(secondBatch.Select(w => w.Code).Intersect(existingCodes, StringComparer.OrdinalIgnoreCase));
        }

        [Fact]
        public void GenerateWorlds_IsDeterministicForSameSeedAndInputs()
        {
            var service = new GalaxyGeneratorService();
            var firstRun = service.GenerateWorlds(30, new List<string>(), seed: 99);
            var secondRun = service.GenerateWorlds(30, new List<string>(), seed: 99);

            Assert.Equal(firstRun.Select(w => w.Code), secondRun.Select(w => w.Code));
            Assert.Equal(firstRun.Select(w => w.Description), secondRun.Select(w => w.Description));
        }
    }
}
