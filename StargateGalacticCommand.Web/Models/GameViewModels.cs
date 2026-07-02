using System.Collections.Generic;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;

namespace StargateGalacticCommand.Web.Models
{
    public class OverviewViewModel { public User User { get; set; } public PlayerBase Base { get; set; } public Planet Planet { get; set; } public ResourceProduction Hourly { get; set; } public IList<PlanetSector> Sectors { get; set; } public IList<Report> Reports { get; set; } }
}
