using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Benutzername ist erforderlich.")]
        [MinLength(3, ErrorMessage = "Der Benutzername muss mindestens 3 Zeichen lang sein.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-Mail ist erforderlich.")]
        [EmailAddress(ErrorMessage = "Bitte gib eine gültige E-Mail-Adresse ein.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        [MinLength(8, ErrorMessage = "Das Passwort muss mindestens 8 Zeichen lang sein.")]
        public string Password { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Bitte wähle eine Fraktion.")]
        public int FactionId { get; set; }

        public string Error { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public IList<Faction> Factions { get; set; } = new List<Faction>();
    }
}
