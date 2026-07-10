using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Web.Models
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        public string Password { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;
    }

    public class AdminServerListEntry
    {
        public GameServer Server { get; set; }
        public int PlayerCount { get; set; }
    }

    public class AdminServerListViewModel
    {
        public IList<AdminServerListEntry> Servers { get; set; } = new List<AdminServerListEntry>();
    }
}
