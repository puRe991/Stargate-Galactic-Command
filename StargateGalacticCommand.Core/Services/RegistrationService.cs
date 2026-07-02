using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class RegistrationService
    {
        private readonly EconomyService _economy;
        private readonly PasswordService _passwords;
        public RegistrationService(EconomyService economy, PasswordService passwords) { _economy = economy; _passwords = passwords; }

        public User CreateUserWithStartBase(IQueryable<User> users, IQueryable<Faction> factions, IQueryable<Planet> planets, string userName, string email, string password, int factionId)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Benutzername fehlt.");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@")) throw new ArgumentException("E-Mail ist ungültig.");
            if (users.Any(u => u.UserName == userName || u.Email == email)) throw new InvalidOperationException("Benutzername oder E-Mail ist bereits vergeben.");
            Faction faction = factions.SingleOrDefault(f => f.Id == factionId);
            if (faction == null) throw new ArgumentException("Unbekannte Fraktion.");
            Planet planet = planets.Single(p => p.Name == "P3X-742");
            PlanetSector sector = planet.Sectors.OrderBy(s => s.Number).FirstOrDefault(s => s.IsSettlementSector && s.PlayerBase == null);
            if (sector == null) throw new InvalidOperationException("Alle Startsektoren auf P3X-742 sind belegt.");
            string hash, salt; _passwords.CreateHash(password, out hash, out salt);
            var user = new User { UserName = userName.Trim(), Email = email.Trim().ToLowerInvariant(), PasswordHash = hash, PasswordSalt = salt, FactionId = faction.Id, CreatedAtUtc = DateTime.UtcNow };
            user.Bases.Add(new PlayerBase { Name = user.UserName + " Hauptbasis", FactionId = faction.Id, PlanetSector = sector, LastResourceUpdateUtc = DateTime.UtcNow, Resources = _economy.CreateStartingResources(), BuildingLevels = _economy.CreateStartingBuildings() });
            user.Reports.Add(new Report { Title = "Willkommen auf P3X-742", Body = "Deine Hauptbasis wurde in einem freien Siedlungssektor eingerichtet. Kein PvP, keine Flotten und keine Gate-Missionen sind in Version 0.0.1 aktiv.", CreatedAtUtc = DateTime.UtcNow });
            return user;
        }
    }
}
