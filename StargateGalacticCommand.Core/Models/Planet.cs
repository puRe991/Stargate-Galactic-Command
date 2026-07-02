using System.Collections.Generic;

namespace StargateGalacticCommand.Core.Models
{
    public class Planet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GateAddress { get; set; }
        public bool IsCanonicalRestricted { get; set; }
        public ICollection<BaseSector> BaseSectors { get; set; }

        public Planet()
        {
            BaseSectors = new List<BaseSector>();
        }
    }
}
