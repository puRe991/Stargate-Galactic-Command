using System;
using System.Collections.Generic;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;
using Xunit;

namespace StargateGalacticCommand.Tests
{
    public class AllianceAndSpaceCombatServiceTests
    {
        private static User User(int id, string name = "u") => new User { Id = id, UserName = name, Faction = new Faction { ShortName = "SGC" }, ResearchLevels = new ResearchLevels { Hyperdrive = 1, ShieldTechnology = 1 }, CreatedAtUtc = DateTime.UtcNow.AddDays(-10) };
        private static PlayerBase Base(int id, User user, int planet = 1, int sector = 1) => new PlayerBase { Id = id, Name = "B" + id, UserId = user.Id, User = user, Faction = user.Faction, Resources = new ResourceStock { Naquadah = 1000, Trinium = 1000, Supplies = 1000, Energy = 1000, Personnel = 1000 }, BuildingLevels = new BuildingLevels { CommandCenter = 2, HangarLandingZone = 1 }, Ships = new BaseShips { SmallTransporter = 5 }, PlanetSector = new PlanetSector { Number = sector, PlanetId = planet, Planet = new Planet { Id = planet, Name = "P" + planet } } };

        [Fact]
        public void AllianceCanBeCreatedAndApplicantAccepted()
        {
            var service = new AllianceService(); var founder = User(1, "founder"); var applicant = User(2, "joiner"); var now = DateTime.UtcNow;
            var alliance = service.CreateAlliance(founder, "Tau'ri Defense", "TDC", "Core alliance", now);
            var app = service.Apply(alliance, applicant, "Bitte aufnehmen", new List<AllianceMember>(), new List<AllianceApplication>(), now);
            var member = service.Accept(app, founder, alliance.Members, now.AddMinutes(1));
            Assert.Equal("TDC", alliance.Tag); Assert.Equal(AllianceRank.Founder, Assert.Single(alliance.Members).Rank); Assert.Equal(AllianceRank.Member, member.Rank); Assert.True(app.AcceptedAtUtc.HasValue);
        }

        [Fact]
        public void AttackHasFlightTimeAndProtectionValidation()
        {
            var service = new SpaceCombatService(new ShipyardService(new ResourceService()), new FactionModifierService(), new RankingService()); var attacker = User(1); var defender = User(2); var origin = Base(1, attacker, 1, 1); var target = Base(2, defender, 2, 1);
            var protectedStatus = new PlayerProtectionStatus { UserId = defender.Id, ProtectedUntilUtc = DateTime.UtcNow.AddHours(1), Score = 1000 };
            Assert.Throws<InvalidOperationException>(() => service.StartAttack(attacker, origin, target, ShipType.SmallTransporter, 1, new List<SpaceCombatMission>(), protectedStatus, DateTime.UtcNow));
            var mission = service.StartAttack(attacker, origin, target, ShipType.SmallTransporter, 2, new List<SpaceCombatMission>(), null, DateTime.UtcNow);
            Assert.True(mission.ArrivesAtUtc > mission.StartedAtUtc); Assert.Equal(3, origin.Ships.SmallTransporter);
        }

        [Fact]
        public void RetaliationRightBypassesScoreRatioProtection()
        {
            var service = new SpaceCombatService(new ShipyardService(new ResourceService()), new FactionModifierService(), new RankingService()); var attacker = User(1); var defender = User(2); var origin = Base(1, attacker, 1, 1); var target = Base(2, defender, 2, 1);
            var protectedStatus = new PlayerProtectionStatus { UserId = defender.Id, ProtectedUntilUtc = DateTime.UtcNow.AddHours(-1), Score = 1 };
            var now = DateTime.UtcNow;
            Assert.Throws<InvalidOperationException>(() => service.StartAttack(attacker, origin, target, ShipType.SmallTransporter, 1, new List<SpaceCombatMission>(), protectedStatus, now));
            var priorAttackByDefender = new SpaceCombatMission { AttackerUserId = defender.Id, DefenderUserId = attacker.Id, TargetBaseId = origin.Id, CompletedAtUtc = now.AddHours(-2) };
            var mission = service.StartAttack(attacker, origin, target, ShipType.SmallTransporter, 1, new List<SpaceCombatMission>(), protectedStatus, now, new List<SpaceCombatMission> { priorAttackByDefender });
            Assert.NotNull(mission);
        }

        [Fact]
        public void ActivePactBetweenAlliancesBlocksAttack()
        {
            var service = new SpaceCombatService(new ShipyardService(new ResourceService()), new FactionModifierService(), new RankingService()); var attacker = User(1); var defender = User(2); var origin = Base(1, attacker, 1, 1); var target = Base(2, defender, 2, 1);
            Assert.Throws<InvalidOperationException>(() => service.StartAttack(attacker, origin, target, ShipType.SmallTransporter, 1, new List<SpaceCombatMission>(), null, DateTime.UtcNow, null, alliancesUnderPact: true));
        }

        [Fact]
        public void CombatCreatesReportLootLimitAndKeepsBase()
        {
            var service = new SpaceCombatService(new ShipyardService(new ResourceService()), new FactionModifierService(), new RankingService()); var attacker = User(1); var defender = User(2); var origin = Base(1, attacker); origin.Ships.SmallTransporter = 20; var target = Base(2, defender, 1, 4); target.Ships.SmallTransporter = 0;
            var mission = service.StartAttack(attacker, origin, target, ShipType.SmallTransporter, 10, new List<SpaceCombatMission>(), null, DateTime.UtcNow.AddMinutes(-10)); mission.ArrivesAtUtc = DateTime.UtcNow.AddMinutes(-1);
            var report = service.Resolve(mission, attacker, defender, DateTime.UtcNow); var debris = service.CreateDebris(mission, report, DateTime.UtcNow);
            Assert.True(report.Rounds <= 6); Assert.True(report.LootNaquadah <= 300); Assert.NotNull(mission.TargetBase); Assert.True(debris.Naquadah > 0); Assert.True(mission.CompletedAtUtc.HasValue);
        }
    }
}
