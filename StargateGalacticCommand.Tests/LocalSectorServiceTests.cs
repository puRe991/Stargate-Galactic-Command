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
            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var playerBase = new PlayerBase { BuildingLevels = new BuildingLevels { CommandCenter = 2, NaquadahRefinery = 3 } };
            var user = new User { ResearchLevels = new ResearchLevels { GateAddressing = 1 } };
            var sectors = new[] { new PlanetSector { SectorControl = new SectorControl { LastReinforcedAtUtc = now } }, new PlanetSector { SectorControl = new SectorControl { LastReinforcedAtUtc = now } } };
            int score = new LocalSectorService().CalculateInfluence(playerBase, user, sectors, new[] { new SectorClaim() }, now);
            Assert.Equal(57, score);
        }

        [Fact]
        public void CalculateSectorInfluenceWeight_IsFullWithinGracePeriod()
        {
            var now = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
            var control = new SectorControl { LastReinforcedAtUtc = now.AddHours(-LocalSectorService.DecayGracePeriodHours) };
            Assert.Equal(1.0, new LocalSectorService().CalculateSectorInfluenceWeight(control, now));
        }

        [Fact]
        public void CalculateSectorInfluenceWeight_DecaysLinearlyBetweenGraceAndRelease()
        {
            var now = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
            int midpointHours = (LocalSectorService.DecayGracePeriodHours + LocalSectorService.DecayReleaseAfterHours) / 2;
            var control = new SectorControl { LastReinforcedAtUtc = now.AddHours(-midpointHours) };
            Assert.Equal(0.5, new LocalSectorService().CalculateSectorInfluenceWeight(control, now), 2);
        }

        [Fact]
        public void CalculateSectorInfluenceWeight_IsZeroAtOrAfterRelease()
        {
            var now = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
            var control = new SectorControl { LastReinforcedAtUtc = now.AddHours(-LocalSectorService.DecayReleaseAfterHours) };
            Assert.Equal(0.0, new LocalSectorService().CalculateSectorInfluenceWeight(control, now));
        }

        [Fact]
        public void IsExpired_TrueOnlyAtOrAfterReleaseThreshold()
        {
            var now = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
            var service = new LocalSectorService();
            var stillHeld = new SectorControl { LastReinforcedAtUtc = now.AddHours(-(LocalSectorService.DecayReleaseAfterHours - 1)) };
            var expired = new SectorControl { LastReinforcedAtUtc = now.AddHours(-LocalSectorService.DecayReleaseAfterHours) };

            Assert.False(service.IsExpired(stillHeld, now));
            Assert.True(service.IsExpired(expired, now));
        }

        [Fact]
        public void Reinforce_UpdatesTimestampForControllingUserOnly()
        {
            var now = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
            var control = new SectorControl { UserId = 1, LastReinforcedAtUtc = now.AddDays(-2) };
            var service = new LocalSectorService();

            Assert.Throws<InvalidOperationException>(() => service.Reinforce(control, 2, now));

            service.Reinforce(control, 1, now);
            Assert.Equal(now, control.LastReinforcedAtUtc);
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

        [Fact]
        public void LocalCombat_PreventsMainBaseAttack()
        {
            var service = new LocalCombatService();
            var sector = new PlanetSector { Id = 1, SectorType = SectorType.SettlementSector, PlayerBase = new PlayerBase() };
            Assert.Throws<InvalidOperationException>(() => service.StartMission(new User { Id = 1 }, new PlayerBase(), sector, LocalCombatObjective.SecureNeutralResourceZone, new GroundUnits { Marines = 1 }, Array.Empty<LocalCombatMission>(), DateTime.UtcNow));
        }

        [Fact]
        public void LocalCombat_PreventsNewPlayerAttack()
        {
            var now = new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc);
            Assert.Throws<InvalidOperationException>(() => new LocalCombatService().ValidateProtection(new User { Id = 1 }, new User { Id = 2, CreatedAtUtc = now.AddDays(-2) }, 100, 100, now));
        }

        [Fact]
        public void LocalCombat_ChangesControlOnlyOnValidWin()
        {
            var now = new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc);
            var sector = new PlanetSector { Id = 5, Name = "Naquadah-Ader", SectorType = SectorType.NaquadahDeposit };
            var mission = new LocalCombatMission { AttackerUserId = 1, PlanetSectorId = 5, AttackingUnits = new GroundUnits { Marines = 8 }, DefendingUnits = new DefenseUnits { LocalMilitia = 1 }, ResolvesAtUtc = now.AddMinutes(-1) };
            var report = new LocalCombatService().Resolve(mission, new User { Id = 1, Faction = new Faction { ShortName = "SGC" } }, null, sector, now);
            Assert.True(report.AttackerWon);
            Assert.NotNull(sector.SectorControl);
            Assert.Equal(1, sector.SectorControl.UserId);
        }

        [Fact]
        public void LocalCombat_CooldownBlocksRepeatedSectorAttack()
        {
            var now = new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc);
            var old = new LocalCombatMission { AttackerUserId = 1, PlanetSectorId = 3, CompletedAtUtc = now.AddHours(-1) };
            Assert.Throws<InvalidOperationException>(() => new LocalCombatService().StartMission(new User { Id = 1 }, new PlayerBase(), new PlanetSector { Id = 3, SectorType = SectorType.TriniumField }, LocalCombatObjective.SecureNeutralResourceZone, new GroundUnits { Marines = 1 }, new[] { old }, now));
        }

        [Fact]
        public void LocalCombat_FactionBonusInfluencesPower()
        {
            var now = new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc);
            var sector = new PlanetSector { Id = 6, Name = "Ruine", SectorType = SectorType.GoauldRuin };
            var mission = new LocalCombatMission { AttackerUserId = 1, PlanetSectorId = 6, AttackingUnits = new GroundUnits { EliteJaffa = 2 }, DefendingUnits = new DefenseUnits { LocalMilitia = 1 }, ResolvesAtUtc = now.AddMinutes(-1) };
            var report = new LocalCombatService().Resolve(mission, new User { Id = 1, Faction = new Faction { ShortName = "Jaffa" } }, null, sector, now);
            Assert.Contains("Angriff", report.Body);
        }
    }
}
