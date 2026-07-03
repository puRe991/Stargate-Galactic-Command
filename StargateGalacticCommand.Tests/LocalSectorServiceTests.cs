using System;
using System.Collections.Generic;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class LocalSectorServiceTests
    {
        [Fact]
        public void StartClaim_CreatesTimedClaimForNeutralResourceSector()
        {
            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var sector = new PlanetSector { Id = 7, SectorType = SectorType.TriniumField };
            var claim = new LocalSectorService().StartClaim(new User { Id = 3 }, sector, new List<SectorClaim>(), now);
            Assert.Equal(3, claim.UserId);
            Assert.Equal(7, claim.PlanetSectorId);
            Assert.Equal(now.AddMinutes(LocalSectorService.ClaimDurationMinutes), claim.CompletesAtUtc);
        }

        [Fact]
        public void StartClaim_PreventsDuplicateClaim()
        {
            var sector = new PlanetSector { Id = 9, SectorType = SectorType.NaquadahDeposit };
            Assert.Throws<InvalidOperationException>(() => new LocalSectorService().StartClaim(new User { Id = 1 }, sector, new[] { new SectorClaim { PlanetSectorId = 9 } }, DateTime.UtcNow));
        }

        [Fact]
        public void CalculateBonus_AppliesControlledSectorBonuses()
        {
            var bonus = new LocalSectorService().CalculateBonus(new[] { new PlanetSector { SectorType = SectorType.NaquadahDeposit }, new PlanetSector { SectorType = SectorType.TradingPost } });
            Assert.Equal(0.10, bonus.NaquadahMultiplier);
            Assert.Equal(0.05, bonus.SuppliesMultiplier);
        }

        [Fact]
        public void CalculateInfluence_CombinesBaseSectorsResearchAndActions()
        {
            var playerBase = new PlayerBase { BuildingLevels = new BuildingLevels { CommandCenter = 2, NaquadahRefinery = 3 } };
            var user = new User { ResearchLevels = new ResearchLevels { GateAddressing = 1 } };
            int score = new LocalSectorService().CalculateInfluence(playerBase, user, new[] { new PlanetSector(), new PlanetSector() }, new[] { new SectorClaim() });
            Assert.Equal(57, score);
        }

        [Fact]
        public void CompleteClaim_AssignsControlAndReportAfterDuration()
        {
            var now = new DateTime(2026, 1, 1, 1, 0, 0, DateTimeKind.Utc);
            var claim = new SectorClaim { UserId = 4, PlanetSectorId = 8, CompletesAtUtc = now.AddMinutes(-1), PlanetSector = new PlanetSector { Id = 8, Name = "Ruine", SectorType = SectorType.GoauldRuin } };
            var report = new LocalSectorService().CompleteClaim(claim, now);
            Assert.True(claim.IsCompleted);
            Assert.NotNull(claim.PlanetSector.SectorControl);
            Assert.Equal(4, claim.PlanetSector.SectorControl.UserId);
            Assert.Equal("Sektor gesichert", report.Title);
        }
    }
}
