using System;

namespace StargateGalacticCommand.Core.Models
{
    public class LoginAttempt
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public string IpHash { get; set; }
        public string UsernameKey { get; set; }
        public bool Succeeded { get; set; }
        public DateTime AttemptedAtUtc { get; set; }
    }
}
