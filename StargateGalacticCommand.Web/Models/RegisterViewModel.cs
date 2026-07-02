using System.Collections.Generic;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Web.Models
{
    public class RegisterViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int FactionId { get; set; }
        public string Error { get; set; }
        public IList<Faction> Factions { get; set; }
    }
}
