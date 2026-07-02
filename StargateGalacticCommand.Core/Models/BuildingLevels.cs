using System;

namespace StargateGalacticCommand.Core.Models
{
    public class BuildingLevels
    {
        public int Id { get; set; }
        public int PlayerBaseId { get; set; }
        public int CommandCenter { get; set; }
        public int NaquadahRefinery { get; set; }
        public int TriniumMine { get; set; }
        public int SupplyDepot { get; set; }
        public int EnergyGenerator { get; set; }
        public int ResearchLab { get; set; }
        public int GateControlRoom { get; set; }
        public int SensorStation { get; set; }
        public int DefenseRing { get; set; }
        public int HangarLandingZone { get; set; }

        public int GetLevel(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.CommandCenter: return CommandCenter;
                case BuildingType.NaquadahRefinery: return NaquadahRefinery;
                case BuildingType.TriniumMine: return TriniumMine;
                case BuildingType.SupplyDepot: return SupplyDepot;
                case BuildingType.EnergyGenerator: return EnergyGenerator;
                case BuildingType.ResearchLab: return ResearchLab;
                case BuildingType.GateControlRoom: return GateControlRoom;
                case BuildingType.SensorStation: return SensorStation;
                case BuildingType.DefenseRing: return DefenseRing;
                case BuildingType.HangarLandingZone: return HangarLandingZone;
                default: throw new ArgumentOutOfRangeException("type");
            }
        }

        public void SetLevel(BuildingType type, int level)
        {
            if (level < 0) throw new ArgumentOutOfRangeException("level", "Building level must not be negative.");
            switch (type)
            {
                case BuildingType.CommandCenter: CommandCenter = level; break;
                case BuildingType.NaquadahRefinery: NaquadahRefinery = level; break;
                case BuildingType.TriniumMine: TriniumMine = level; break;
                case BuildingType.SupplyDepot: SupplyDepot = level; break;
                case BuildingType.EnergyGenerator: EnergyGenerator = level; break;
                case BuildingType.ResearchLab: ResearchLab = level; break;
                case BuildingType.GateControlRoom: GateControlRoom = level; break;
                case BuildingType.SensorStation: SensorStation = level; break;
                case BuildingType.DefenseRing: DefenseRing = level; break;
                case BuildingType.HangarLandingZone: HangarLandingZone = level; break;
                default: throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
