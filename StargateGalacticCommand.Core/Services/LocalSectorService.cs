using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class LocalSectorService
    {
        public const int ClaimDurationMinutes = 30;

        public bool CanClaim(PlanetSector sector)
        {
            return sector != null && sector.PlayerBase == null && sector.SectorControl == null && sector.SectorType != SectorType.SettlementSector && sector.SectorType != SectorType.StargateZone;
        }

        public SectorClaim StartClaim(User user, PlanetSector sector, IEnumerable<SectorClaim> activeClaims, DateTime nowUtc)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (!CanClaim(sector)) throw new InvalidOperationException("Dieser Sektor kann in Version 0.0.5 nicht beansprucht werden.");
            if (activeClaims != null && activeClaims.Any(c => c.PlanetSectorId == sector.Id && !c.IsCompleted)) throw new InvalidOperationException("Dieser Sektor wird bereits beansprucht.");
            return new SectorClaim { UserId = user.Id, PlanetSectorId = sector.Id, StartedAtUtc = nowUtc, CompletesAtUtc = nowUtc.AddMinutes(ClaimDurationMinutes), IsCompleted = false };
        }

        public LocalActionReport CompleteClaim(SectorClaim claim, DateTime nowUtc)
        {
            if (claim == null) throw new ArgumentNullException("claim");
            if (claim.IsCompleted) throw new InvalidOperationException("Diese Beanspruchung ist bereits abgeschlossen.");
            if (nowUtc < claim.CompletesAtUtc) throw new InvalidOperationException("Die Beanspruchung läuft noch.");
            if (claim.PlanetSector == null) throw new ArgumentException("Beanspruchung hat keinen Sektor.", "claim");
            if (claim.PlanetSector.SectorControl != null) throw new InvalidOperationException("Dieser Sektor ist bereits kontrolliert.");
            claim.IsCompleted = true;
            claim.PlanetSector.SectorControl = new SectorControl { PlanetSectorId = claim.PlanetSectorId, UserId = claim.UserId, ControlledAtUtc = nowUtc };
            return new LocalActionReport { UserId = claim.UserId, PlanetSectorId = claim.PlanetSectorId, CreatedAtUtc = nowUtc, Title = "Sektor gesichert", Body = claim.PlanetSector.Name + " steht jetzt unter deiner Kontrolle." };
        }

        public SectorBonus CalculateBonus(IEnumerable<PlanetSector> controlledSectors)
        {
            var bonus = new SectorBonus();
            if (controlledSectors == null) return bonus;
            foreach (var sector in controlledSectors)
            {
                if (sector == null) continue;
                if (sector.SectorType == SectorType.NaquadahDeposit) bonus.NaquadahMultiplier += 0.10;
                else if (sector.SectorType == SectorType.TriniumField) bonus.TriniumMultiplier += 0.10;
                else if (sector.SectorType == SectorType.LocalSettlement) bonus.PersonnelMultiplier += 0.05;
                else if (sector.SectorType == SectorType.TradingPost) bonus.SuppliesMultiplier += 0.05;
                else if (sector.SectorType == SectorType.GoauldRuin) bonus.IntelMultiplier += 0.05;
            }
            return bonus;
        }

        public int CalculateInfluence(PlayerBase playerBase, User user, IEnumerable<PlanetSector> controlledSectors, IEnumerable<SectorClaim> runningClaims)
        {
            int score = Math.Max(0, playerBase == null || playerBase.BuildingLevels == null ? 0 : playerBase.BuildingLevels.CommandCenter) * 10;
            if (playerBase != null && playerBase.BuildingLevels != null) score += playerBase.BuildingLevels.NaquadahRefinery + playerBase.BuildingLevels.TriniumMine + playerBase.BuildingLevels.SupplyDepot + playerBase.BuildingLevels.EnergyGenerator + playerBase.BuildingLevels.ResearchLab + playerBase.BuildingLevels.GateControlRoom + playerBase.BuildingLevels.SensorStation;
            if (user != null && user.ResearchLevels != null) score += user.ResearchLevels.NaquadahEnergyTechnology + user.ResearchLevels.Sensorics + user.ResearchLevels.GateAddressing;
            score += (controlledSectors == null ? 0 : controlledSectors.Count()) * 15;
            score += (runningClaims == null ? 0 : runningClaims.Count(c => !c.IsCompleted)) * 3;
            return score;
        }
    }
}
