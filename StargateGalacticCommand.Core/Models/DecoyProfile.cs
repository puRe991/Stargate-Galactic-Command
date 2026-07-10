using System;

namespace StargateGalacticCommand.Core.Models
{
    public class DecoyProfile
    {
        public int Id { get; set; }
        public int PlayerBaseId { get; set; }
        public PlayerBase PlayerBase { get; set; }
        public bool IsActive { get; set; }
        public int Charges { get; set; }
        public int FakeNaquadah { get; set; }
        public int FakeTrinium { get; set; }
        public int FakeSupplies { get; set; }
        public int FakeEnergy { get; set; }
        public int FakePersonnel { get; set; }
        public int FakeIntel { get; set; }
        public int FakeShipTotal { get; set; }
        public DateTime? LastArmedAtUtc { get; set; }
    }
}
