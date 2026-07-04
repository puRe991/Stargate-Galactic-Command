namespace StargateGalacticCommand.Core.Models
{
    public class BaseShips
    {
        public int Id { get; set; }
        public int PlayerBaseId { get; set; }
        public PlayerBase PlayerBase { get; set; }
        public int F302 { get; set; }
        public int SmallTransporter { get; set; }
        public int SupplyShuttle { get; set; }
        public int Teltak { get; set; }
        public int AlkeshLightBomber { get; set; }
        public int JaffaTransporter { get; set; }
        public int CloakedTeltak { get; set; }
        public int AgentTransporter { get; set; }
        public int SmugglerTransporter { get; set; }
        public int PirateFighter { get; set; }
        public int GetCount(ShipType t){ switch(t){case ShipType.F302:return F302;case ShipType.SmallTransporter:return SmallTransporter;case ShipType.SupplyShuttle:return SupplyShuttle;case ShipType.Teltak:return Teltak;case ShipType.AlkeshLightBomber:return AlkeshLightBomber;case ShipType.JaffaTransporter:return JaffaTransporter;case ShipType.CloakedTeltak:return CloakedTeltak;case ShipType.AgentTransporter:return AgentTransporter;case ShipType.SmugglerTransporter:return SmugglerTransporter;case ShipType.PirateFighter:return PirateFighter;default:throw new System.ArgumentOutOfRangeException("t");}}
        public void Add(ShipType t,int a){ if(a<0)throw new System.ArgumentOutOfRangeException("a"); Set(t,GetCount(t)+a);}
        public void Remove(ShipType t,int a){ if(a<0||GetCount(t)<a)throw new System.InvalidOperationException("Nicht genug Schiffe verfügbar."); Set(t,GetCount(t)-a);}
        private void Set(ShipType t,int v){ switch(t){case ShipType.F302:F302=v;break;case ShipType.SmallTransporter:SmallTransporter=v;break;case ShipType.SupplyShuttle:SupplyShuttle=v;break;case ShipType.Teltak:Teltak=v;break;case ShipType.AlkeshLightBomber:AlkeshLightBomber=v;break;case ShipType.JaffaTransporter:JaffaTransporter=v;break;case ShipType.CloakedTeltak:CloakedTeltak=v;break;case ShipType.AgentTransporter:AgentTransporter=v;break;case ShipType.SmugglerTransporter:SmugglerTransporter=v;break;case ShipType.PirateFighter:PirateFighter=v;break;default:throw new System.ArgumentOutOfRangeException("t");}}
    }
}
