using System;
using System.Collections.Generic;

namespace StargateGalacticCommand.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public int FactionId { get; set; }
        public Faction Faction { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public ICollection<PlayerBase> Bases { get; set; }
        public ICollection<Report> Reports { get; set; }
        public User() { Bases = new List<PlayerBase>(); Reports = new List<Report>(); }
    }
}
