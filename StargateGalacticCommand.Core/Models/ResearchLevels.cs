using System;

namespace StargateGalacticCommand.Core.Models
{
    public class ResearchLevels
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int GateAddressing { get; set; }
        public int NaquadahEnergyTechnology { get; set; }
        public int ShieldTechnology { get; set; }
        public int Hyperdrive { get; set; }
        public int Sensorics { get; set; }
        public int Medicine { get; set; }
        public int StealthTechnology { get; set; }
        public int Diplomacy { get; set; }
        public int Logistics { get; set; }
        public int AsgardDataAnalysis { get; set; }
        public int Bc304Tactics { get; set; }
        public int IrisSecurityProtocols { get; set; }
        public int StaffWeaponDiscipline { get; set; }
        public int HatakCommandStructure { get; set; }
        public int JaffaWarriorCode { get; set; }
        public int CovertInfiltration { get; set; }
        public int GoauldSabotage { get; set; }
        public int CloakFieldCoordination { get; set; }
        public int SmugglingRoutes { get; set; }
        public int BlackMarketLogistics { get; set; }
        public int MercenaryContracts { get; set; }

        public int GetLevel(ResearchType type)
        {
            switch (type)
            {
                case ResearchType.GateAddressing: return GateAddressing;
                case ResearchType.NaquadahEnergyTechnology: return NaquadahEnergyTechnology;
                case ResearchType.ShieldTechnology: return ShieldTechnology;
                case ResearchType.Hyperdrive: return Hyperdrive;
                case ResearchType.Sensorics: return Sensorics;
                case ResearchType.Medicine: return Medicine;
                case ResearchType.StealthTechnology: return StealthTechnology;
                case ResearchType.Diplomacy: return Diplomacy;
                case ResearchType.Logistics: return Logistics;
                case ResearchType.AsgardDataAnalysis: return AsgardDataAnalysis;
                case ResearchType.Bc304Tactics: return Bc304Tactics;
                case ResearchType.IrisSecurityProtocols: return IrisSecurityProtocols;
                case ResearchType.StaffWeaponDiscipline: return StaffWeaponDiscipline;
                case ResearchType.HatakCommandStructure: return HatakCommandStructure;
                case ResearchType.JaffaWarriorCode: return JaffaWarriorCode;
                case ResearchType.CovertInfiltration: return CovertInfiltration;
                case ResearchType.GoauldSabotage: return GoauldSabotage;
                case ResearchType.CloakFieldCoordination: return CloakFieldCoordination;
                case ResearchType.SmugglingRoutes: return SmugglingRoutes;
                case ResearchType.BlackMarketLogistics: return BlackMarketLogistics;
                case ResearchType.MercenaryContracts: return MercenaryContracts;
                default: throw new ArgumentOutOfRangeException("type");
            }
        }

        public void SetLevel(ResearchType type, int level)
        {
            if (level < 0) throw new ArgumentOutOfRangeException("level", "Research level must not be negative.");
            switch (type)
            {
                case ResearchType.GateAddressing: GateAddressing = level; break;
                case ResearchType.NaquadahEnergyTechnology: NaquadahEnergyTechnology = level; break;
                case ResearchType.ShieldTechnology: ShieldTechnology = level; break;
                case ResearchType.Hyperdrive: Hyperdrive = level; break;
                case ResearchType.Sensorics: Sensorics = level; break;
                case ResearchType.Medicine: Medicine = level; break;
                case ResearchType.StealthTechnology: StealthTechnology = level; break;
                case ResearchType.Diplomacy: Diplomacy = level; break;
                case ResearchType.Logistics: Logistics = level; break;
                case ResearchType.AsgardDataAnalysis: AsgardDataAnalysis = level; break;
                case ResearchType.Bc304Tactics: Bc304Tactics = level; break;
                case ResearchType.IrisSecurityProtocols: IrisSecurityProtocols = level; break;
                case ResearchType.StaffWeaponDiscipline: StaffWeaponDiscipline = level; break;
                case ResearchType.HatakCommandStructure: HatakCommandStructure = level; break;
                case ResearchType.JaffaWarriorCode: JaffaWarriorCode = level; break;
                case ResearchType.CovertInfiltration: CovertInfiltration = level; break;
                case ResearchType.GoauldSabotage: GoauldSabotage = level; break;
                case ResearchType.CloakFieldCoordination: CloakFieldCoordination = level; break;
                case ResearchType.SmugglingRoutes: SmugglingRoutes = level; break;
                case ResearchType.BlackMarketLogistics: BlackMarketLogistics = level; break;
                case ResearchType.MercenaryContracts: MercenaryContracts = level; break;
                default: throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
