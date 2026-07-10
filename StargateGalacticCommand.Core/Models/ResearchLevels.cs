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
        public int AdvancedNaquadahRefining { get; set; }
        public int AutomatedTriniumExtraction { get; set; }
        public int StructuralEngineering { get; set; }
        public int AdvancedShipEngineering { get; set; }
        public int XenoArchaeology { get; set; }
        public int AsgardDataAnalysis { get; set; }
        public int Bc304Tactics { get; set; }
        public int IrisSecurityProtocols { get; set; }
        public int NaquadahReactorMiniaturization { get; set; }
        public int AncientOutpostTechnology { get; set; }
        public int PrometheusEngineering { get; set; }
        public int StargateNetworkMapping { get; set; }
        public int AsgardBeamingTechnology { get; set; }
        public int ZeroPointModuleTheory { get; set; }
        public int StaffWeaponDiscipline { get; set; }
        public int HatakCommandStructure { get; set; }
        public int JaffaWarriorCode { get; set; }
        public int SymbioteEfficiency { get; set; }
        public int GroundAssaultTactics { get; set; }
        public int FortifiedGarrisons { get; set; }
        public int KelNoReemTraining { get; set; }
        public int HonorGuardProtocols { get; set; }
        public int FreeJaffaNationLogistics { get; set; }
        public int CovertInfiltration { get; set; }
        public int GoauldSabotage { get; set; }
        public int CloakFieldCoordination { get; set; }
        public int SymbioteHealing { get; set; }
        public int DeepCoverNetworks { get; set; }
        public int HostBondingTechnology { get; set; }
        public int IntelligenceNetworkExpansion { get; set; }
        public int SystemLordDossiers { get; set; }
        public int ShadowCouncilInfluence { get; set; }
        public int SmugglingRoutes { get; set; }
        public int BlackMarketLogistics { get; set; }
        public int MercenaryContracts { get; set; }
        public int PirateNetworkConnections { get; set; }
        public int RuthlessNegotiation { get; set; }
        public int StolenTechnologyIntegration { get; set; }
        public int HiddenCaches { get; set; }
        public int ExtortionNetworks { get; set; }
        public int WarlordAmbitions { get; set; }

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
                case ResearchType.AdvancedNaquadahRefining: return AdvancedNaquadahRefining;
                case ResearchType.AutomatedTriniumExtraction: return AutomatedTriniumExtraction;
                case ResearchType.StructuralEngineering: return StructuralEngineering;
                case ResearchType.AdvancedShipEngineering: return AdvancedShipEngineering;
                case ResearchType.XenoArchaeology: return XenoArchaeology;
                case ResearchType.AsgardDataAnalysis: return AsgardDataAnalysis;
                case ResearchType.Bc304Tactics: return Bc304Tactics;
                case ResearchType.IrisSecurityProtocols: return IrisSecurityProtocols;
                case ResearchType.NaquadahReactorMiniaturization: return NaquadahReactorMiniaturization;
                case ResearchType.AncientOutpostTechnology: return AncientOutpostTechnology;
                case ResearchType.PrometheusEngineering: return PrometheusEngineering;
                case ResearchType.StargateNetworkMapping: return StargateNetworkMapping;
                case ResearchType.AsgardBeamingTechnology: return AsgardBeamingTechnology;
                case ResearchType.ZeroPointModuleTheory: return ZeroPointModuleTheory;
                case ResearchType.StaffWeaponDiscipline: return StaffWeaponDiscipline;
                case ResearchType.HatakCommandStructure: return HatakCommandStructure;
                case ResearchType.JaffaWarriorCode: return JaffaWarriorCode;
                case ResearchType.SymbioteEfficiency: return SymbioteEfficiency;
                case ResearchType.GroundAssaultTactics: return GroundAssaultTactics;
                case ResearchType.FortifiedGarrisons: return FortifiedGarrisons;
                case ResearchType.KelNoReemTraining: return KelNoReemTraining;
                case ResearchType.HonorGuardProtocols: return HonorGuardProtocols;
                case ResearchType.FreeJaffaNationLogistics: return FreeJaffaNationLogistics;
                case ResearchType.CovertInfiltration: return CovertInfiltration;
                case ResearchType.GoauldSabotage: return GoauldSabotage;
                case ResearchType.CloakFieldCoordination: return CloakFieldCoordination;
                case ResearchType.SymbioteHealing: return SymbioteHealing;
                case ResearchType.DeepCoverNetworks: return DeepCoverNetworks;
                case ResearchType.HostBondingTechnology: return HostBondingTechnology;
                case ResearchType.IntelligenceNetworkExpansion: return IntelligenceNetworkExpansion;
                case ResearchType.SystemLordDossiers: return SystemLordDossiers;
                case ResearchType.ShadowCouncilInfluence: return ShadowCouncilInfluence;
                case ResearchType.SmugglingRoutes: return SmugglingRoutes;
                case ResearchType.BlackMarketLogistics: return BlackMarketLogistics;
                case ResearchType.MercenaryContracts: return MercenaryContracts;
                case ResearchType.PirateNetworkConnections: return PirateNetworkConnections;
                case ResearchType.RuthlessNegotiation: return RuthlessNegotiation;
                case ResearchType.StolenTechnologyIntegration: return StolenTechnologyIntegration;
                case ResearchType.HiddenCaches: return HiddenCaches;
                case ResearchType.ExtortionNetworks: return ExtortionNetworks;
                case ResearchType.WarlordAmbitions: return WarlordAmbitions;
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
                case ResearchType.AdvancedNaquadahRefining: AdvancedNaquadahRefining = level; break;
                case ResearchType.AutomatedTriniumExtraction: AutomatedTriniumExtraction = level; break;
                case ResearchType.StructuralEngineering: StructuralEngineering = level; break;
                case ResearchType.AdvancedShipEngineering: AdvancedShipEngineering = level; break;
                case ResearchType.XenoArchaeology: XenoArchaeology = level; break;
                case ResearchType.AsgardDataAnalysis: AsgardDataAnalysis = level; break;
                case ResearchType.Bc304Tactics: Bc304Tactics = level; break;
                case ResearchType.IrisSecurityProtocols: IrisSecurityProtocols = level; break;
                case ResearchType.NaquadahReactorMiniaturization: NaquadahReactorMiniaturization = level; break;
                case ResearchType.AncientOutpostTechnology: AncientOutpostTechnology = level; break;
                case ResearchType.PrometheusEngineering: PrometheusEngineering = level; break;
                case ResearchType.StargateNetworkMapping: StargateNetworkMapping = level; break;
                case ResearchType.AsgardBeamingTechnology: AsgardBeamingTechnology = level; break;
                case ResearchType.ZeroPointModuleTheory: ZeroPointModuleTheory = level; break;
                case ResearchType.StaffWeaponDiscipline: StaffWeaponDiscipline = level; break;
                case ResearchType.HatakCommandStructure: HatakCommandStructure = level; break;
                case ResearchType.JaffaWarriorCode: JaffaWarriorCode = level; break;
                case ResearchType.SymbioteEfficiency: SymbioteEfficiency = level; break;
                case ResearchType.GroundAssaultTactics: GroundAssaultTactics = level; break;
                case ResearchType.FortifiedGarrisons: FortifiedGarrisons = level; break;
                case ResearchType.KelNoReemTraining: KelNoReemTraining = level; break;
                case ResearchType.HonorGuardProtocols: HonorGuardProtocols = level; break;
                case ResearchType.FreeJaffaNationLogistics: FreeJaffaNationLogistics = level; break;
                case ResearchType.CovertInfiltration: CovertInfiltration = level; break;
                case ResearchType.GoauldSabotage: GoauldSabotage = level; break;
                case ResearchType.CloakFieldCoordination: CloakFieldCoordination = level; break;
                case ResearchType.SymbioteHealing: SymbioteHealing = level; break;
                case ResearchType.DeepCoverNetworks: DeepCoverNetworks = level; break;
                case ResearchType.HostBondingTechnology: HostBondingTechnology = level; break;
                case ResearchType.IntelligenceNetworkExpansion: IntelligenceNetworkExpansion = level; break;
                case ResearchType.SystemLordDossiers: SystemLordDossiers = level; break;
                case ResearchType.ShadowCouncilInfluence: ShadowCouncilInfluence = level; break;
                case ResearchType.SmugglingRoutes: SmugglingRoutes = level; break;
                case ResearchType.BlackMarketLogistics: BlackMarketLogistics = level; break;
                case ResearchType.MercenaryContracts: MercenaryContracts = level; break;
                case ResearchType.PirateNetworkConnections: PirateNetworkConnections = level; break;
                case ResearchType.RuthlessNegotiation: RuthlessNegotiation = level; break;
                case ResearchType.StolenTechnologyIntegration: StolenTechnologyIntegration = level; break;
                case ResearchType.HiddenCaches: HiddenCaches = level; break;
                case ResearchType.ExtortionNetworks: ExtortionNetworks = level; break;
                case ResearchType.WarlordAmbitions: WarlordAmbitions = level; break;
                default: throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
