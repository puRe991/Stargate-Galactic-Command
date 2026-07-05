using System.ComponentModel.DataAnnotations;

namespace StargateGalacticCommand.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Benutzername oder E-Mail ist erforderlich.")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        [MinLength(8, ErrorMessage = "Das Passwort muss mindestens 8 Zeichen lang sein.")]
        public string Password { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;
    }
}
