using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class LocalCombatService
    {
        // Bewaffnete Sektorangriffe sind ausschließlich auf Planeten mit diesem Status erlaubt.
        // Auf "geteilt" (Startplaneten) und "neutral" bleibt nur friedliche Beanspruchung (LocalSectorService.StartClaim) möglich.
        public const string ContestedPlanetStatus = "umkämpft";
        public const int SectorAttackCooldownHours = 6;
        public const int MissionTravelMinutes = 10;
        public const int MaxRounds = 6;

        // Einzige Quelle für Anfänger-/Startplaneten-Schutz: die persistierte PlayerProtectionStatus,
        // dieselbe die auch SpaceCombatService prüft. Vorher gab es hier eine zweite, abweichende
        // 7-Tage-Regel auf Basis von User.CreatedAtUtc, die nie mit dem Weltraumkampf-Schutz übereinstimmte.
        public void ValidateProtection(User defender, PlayerProtectionStatus defenderProtection, int attackerScore, int defenderScore, DateTime nowUtc)
        {
            if (defender == null) return;
            if (defenderProtection != null && defenderProtection.IsUnderBeginnerProtection(nowUtc)) throw new InvalidOperationException("Spieler unter Startplaneten-Schutz dürfen nicht angegriffen werden.");
            if (attackerScore > 0 && defenderScore > 0 && (attackerScore > defenderScore * 3 || defenderScore > attackerScore * 3)) throw new InvalidOperationException("Punktedifferenz-Schutz verhindert diesen lokalen Konflikt.");
        }

        public LocalCombatMission StartMission(User attacker, PlayerBase attackerBase, PlanetSector sector, LocalCombatObjective objective, GroundUnits units, IEnumerable<LocalCombatMission> sectorMissions, DateTime nowUtc)
        {
            if (attacker == null || attackerBase == null) throw new ArgumentNullException("attacker");
            if (sector == null) throw new ArgumentNullException("sector");
            if (sector.PlayerBase != null || sector.SectorType == SectorType.SettlementSector || sector.SectorType == SectorType.StargateZone) throw new InvalidOperationException("Hauptbasen und Stargate-Zonen sind durch die Lore-Regel geschützt.");
            if (sector.Planet == null || !string.Equals(sector.Planet.Status, ContestedPlanetStatus, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Bewaffnete Sektorangriffe sind nur auf umkämpften Planeten erlaubt; hier ist nur friedliche Beanspruchung möglich.");
            if (units == null || units.Total < 1) throw new InvalidOperationException("Mindestens eine Bodeneinheit muss eingesetzt werden.");
            if (sector.SectorControl != null && sector.SectorControl.UserId == attacker.Id) throw new InvalidOperationException("Eigene Sektoren können nur gehalten, nicht angegriffen werden.");
            if (sectorMissions != null && sectorMissions.Any(m => m.PlanetSectorId == sector.Id && !m.CompletedAtUtc.HasValue)) throw new InvalidOperationException("In diesem Sektor läuft bereits ein lokaler Konflikt.");
            if (sectorMissions != null && sectorMissions.Any(m => m.PlanetSectorId == sector.Id && m.AttackerUserId == attacker.Id && m.CompletedAtUtc.HasValue && m.CompletedAtUtc.Value > nowUtc.AddHours(-SectorAttackCooldownHours))) throw new InvalidOperationException("Cooldown: derselbe Sektor kann noch nicht erneut angegriffen werden.");
            return new LocalCombatMission { AttackerUserId = attacker.Id, DefenderUserId = sector.SectorControl?.UserId, PlanetSectorId = sector.Id, Objective = objective, AttackingUnits = units, DefendingUnits = CreateDefense(sector), StartedAtUtc = nowUtc, ResolvesAtUtc = nowUtc.AddMinutes(MissionTravelMinutes) };
        }

        public SectorBattleReport Resolve(LocalCombatMission mission, User attacker, User defender, PlanetSector sector, DateTime nowUtc)
        {
            if (mission == null) throw new ArgumentNullException("mission");
            if (sector == null) throw new ArgumentNullException("sector");
            if (mission.CompletedAtUtc.HasValue) throw new InvalidOperationException("Dieser Konflikt ist bereits abgeschlossen.");
            if (nowUtc < mission.ResolvesAtUtc) throw new InvalidOperationException("Die Einsatzkräfte sind noch unterwegs.");
            if (sector.PlayerBase != null) throw new InvalidOperationException("Hauptbasen sind nicht zerstörbar und nicht angreifbar.");
            int attackerRemaining = Math.Max(1, mission.AttackingUnits?.Total ?? 0);
            int defenderRemaining = Math.Max(1, mission.DefendingUnits?.Total ?? 0);
            var rounds = new List<LocalCombatRound>();
            for (int i = 1; i <= MaxRounds && attackerRemaining > 0 && defenderRemaining > 0; i++)
            {
                int ap = attackerRemaining * 10 + FactionBonus(attacker?.Faction) + TerrainBonus(sector, true) + TacticBonus(mission.AttackingUnits) + ResearchAttackBonus(attacker);
                int dp = defenderRemaining * 9 + FactionBonus(defender?.Faction) + TerrainBonus(sector, false) + DefenseBonus(mission.DefendingUnits) + ResearchDefenseBonus(defender);
                int dLoss = Math.Min(defenderRemaining, Math.Max(0, ap - dp / 2) / 18 + (ap > dp ? 1 : 0));
                int aLoss = Math.Min(attackerRemaining, Math.Max(0, dp - ap / 2) / 20 + (dp >= ap ? 1 : 0));
                int capA = Math.Max(1, (int)Math.Ceiling(attackerRemaining * 0.35));
                int capD = Math.Max(1, (int)Math.Ceiling(defenderRemaining * 0.35));
                aLoss = Math.Min(aLoss, capA); dLoss = Math.Min(dLoss, capD);
                attackerRemaining -= aLoss; defenderRemaining -= dLoss;
                mission.AttackerLosses += aLoss; mission.DefenderLosses += dLoss;
                var round = new LocalCombatRound { LocalCombatMission = mission, RoundNumber = i, AttackerPower = ap, DefenderPower = dp, AttackerLosses = aLoss, DefenderLosses = dLoss, Summary = "Runde " + i + ": lokale Feuergefechte, Sabotage und Geländegewinne." };
                rounds.Add(round);
                mission.Rounds.Add(round);
            }
            mission.AttackerWon = attackerRemaining > 0 && (defenderRemaining == 0 || attackerRemaining * 11 > defenderRemaining * 10);
            mission.CompletedAtUtc = nowUtc;
            foreach (var r in rounds) { if (mission.Id != 0) r.LocalCombatMissionId = mission.Id; }
            if (mission.AttackerWon)
            {
                if (sector.SectorControl == null) sector.SectorControl = new SectorControl { PlanetSectorId = sector.Id, UserId = mission.AttackerUserId, ControlledAtUtc = nowUtc, LastReinforcedAtUtc = nowUtc };
                else { sector.SectorControl.UserId = mission.AttackerUserId; sector.SectorControl.ControlledAtUtc = nowUtc; sector.SectorControl.LastReinforcedAtUtc = nowUtc; }
            }
            string body = string.Join("\n", rounds.Select(r => $"R{r.RoundNumber}: Angriff {r.AttackerPower}, Verteidigung {r.DefenderPower}, Verluste A/D {r.AttackerLosses}/{r.DefenderLosses}")) + $"\nErgebnis: {(mission.AttackerWon ? "Sektorkontrolle gesichert" : "Angriff abgewehrt")}. Hauptbasen und Lager bleiben geschützt.";
            return new SectorBattleReport { UserId = mission.AttackerUserId, LocalCombatMission = mission, PlanetSectorId = sector.Id, CreatedAtUtc = nowUtc, Title = "Lokaler Sektorkampf", Body = body, AttackerWon = mission.AttackerWon };
        }

        private static DefenseUnits CreateDefense(PlanetSector sector) => new DefenseUnits { BaseGuards = sector.SectorControl == null ? 1 : 2, DefenseRings = sector.SectorControl == null ? 0 : 1, SensorAlarms = sector.SectorType == SectorType.GoauldRuin ? 1 : 0, LocalMilitia = sector.SectorType == SectorType.LocalSettlement ? 2 : 1 };
        private static int TacticBonus(GroundUnits u) => (u?.Saboteurs ?? 0) * 4 + (u?.AgentCells ?? 0) * 3 + (u?.EliteJaffa ?? 0) * 3 + (u?.Marines ?? 0) * 2;
        private static int DefenseBonus(DefenseUnits d) => (d?.DefenseRings ?? 0) * 6 + (d?.SensorAlarms ?? 0) * 4 + (d?.BaseGuards ?? 0) * 2;
        private static int TerrainBonus(PlanetSector s, bool attacker) => s.SectorType == SectorType.GoauldRuin ? (attacker ? 2 : 5) : s.SectorType == SectorType.NaquadahDeposit || s.SectorType == SectorType.TriniumField ? 3 : 0;
        private static int FactionBonus(Faction f) { var s = f?.ShortName ?? ""; if (s == "Jaffa") return 5; if (s == "SGC") return 4; if (s == "Tokra") return 4; if (s == "Lucian") return 3; return 0; }
        private static int ResearchAttackBonus(User u) => u?.ResearchLevels == null ? 0 : (u.ResearchLevels.StaffWeaponDiscipline + u.ResearchLevels.GroundAssaultTactics + u.ResearchLevels.MercenaryContracts) * 3;
        private static int ResearchDefenseBonus(User u) => u?.ResearchLevels == null ? 0 : u.ResearchLevels.FortifiedGarrisons * 3;
    }
}
