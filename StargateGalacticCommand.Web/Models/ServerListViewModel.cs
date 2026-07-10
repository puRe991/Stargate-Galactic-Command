using System.Collections.Generic;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Web.Models
{
    public class ServerListEntry
    {
        public GameServer Server { get; set; }
        public int PlayerCount { get; set; }
    }

    public class ServerListViewModel
    {
        public IList<ServerListEntry> Servers { get; set; } = new List<ServerListEntry>();
    }
}
