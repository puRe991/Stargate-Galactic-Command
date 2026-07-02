using System.Collections.Generic;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Web.Models
{
    public class HomeIndexViewModel
    {
        public IList<Planet> OpenPlanets { get; set; }
        public IList<BaseSector> BaseSectors { get; set; }
        public int RestrictedWorldCount { get; set; }

        public HomeIndexViewModel()
        {
            OpenPlanets = new List<Planet>();
            BaseSectors = new List<BaseSector>();
        }
    }
}
