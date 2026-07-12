using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;
using StargateGalacticCommand.Core.Services;

namespace StargateGalacticCommand.Data
{
    // Besetzt leere Siedlungssektoren auf Planeten, auf denen bereits mindestens
    // ein menschlicher Spieler eine Basis hat, mit statischen NSC-Basen, damit
    // gemeinsame Planeten nicht dauerhaft halbleer wirken. NSC-Basen sind über
    // User.IsNpc von Galaxieliste, Rangliste, Postfach und Login ausgeschlossen
    // (siehe GameController.BuildGalaxyEntries, RankingService, MessageService,
    // AccountController) und daher rein kosmetisch, solange es keine PvP-Regeln
    // gegen Basen gibt (siehe README "Offene TODOs").
    public class NpcSpawnService
    {
        public const int SettlementSectorsPerNpc = 3;

        private readonly GameDbContext _db;
        private readonly EconomyService _economy;

        public NpcSpawnService(GameDbContext db, EconomyService economy)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
        }

        public int EnsureNpcPresence(Planet planet, DateTime nowUtc)
        {
            if (planet == null) throw new ArgumentNullException(nameof(planet));

            var settlementSectors = planet.Sectors.Where(s => s.IsSettlementSector).OrderBy(s => s.Number).ToList();
            var emptySectors = settlementSectors.Where(s => s.PlayerBase == null).ToList();
            if (emptySectors.Count == 0) return 0;

            bool hasHumanPresence = _db.PlayerBases.Any(b => b.PlanetSector.PlanetId == planet.Id && !b.User.IsNpc);
            if (!hasHumanPresence) return 0;

            int existingNpcBases = _db.PlayerBases.Count(b => b.PlanetSector.PlanetId == planet.Id && b.User.IsNpc);
            int maxNpcBases = Math.Max(1, settlementSectors.Count / SettlementSectorsPerNpc);
            int missing = maxNpcBases - existingNpcBases;
            if (missing <= 0) return 0;

            var factions = _db.Factions.OrderBy(f => f.Id).ToList();
            if (factions.Count == 0) return 0;

            int created = 0;
            foreach (var sector in emptySectors.Take(missing))
            {
                int npcIndex = existingNpcBases + created;
                var faction = factions[npcIndex % factions.Count];
                var user = new User
                {
                    ServerId = planet.ServerId,
                    UserName = "NSC-Wache #" + sector.Id,
                    Email = "npc-sector-" + sector.Id + "@stargate.local",
                    PasswordHash = string.Empty,
                    PasswordSalt = string.Empty,
                    IsNpc = true,
                    FactionId = faction.Id,
                    Faction = faction,
                    CreatedAtUtc = nowUtc,
                    ResearchLevels = new ResearchLevels()
                };
                user.Bases.Add(BuildNpcBase(sector, faction, npcIndex, nowUtc));
                _db.Users.Add(user);
                created++;
            }

            _db.SaveChanges();
            return created;
        }

        private PlayerBase BuildNpcBase(PlanetSector sector, Faction faction, int npcIndex, DateTime nowUtc)
        {
            var buildings = _economy.CreateStartingBuildings();
            var resources = _economy.CreateStartingResources();
            var ships = new BaseShips();

            switch (npcIndex % 3)
            {
                case 0: // Wachposten
                    buildings.DefenseRing = 1;
                    ships.PirateFighter = 2;
                    break;
                case 1: // Außenposten
                    buildings.CommandCenter = 2;
                    buildings.DefenseRing = 2;
                    resources.Naquadah *= 2;
                    resources.Trinium *= 2;
                    ships.PirateFighter = 4;
                    ships.JaffaTransporter = 1;
                    break;
                default: // Stützpunkt
                    buildings.CommandCenter = 3;
                    buildings.DefenseRing = 3;
                    resources.Naquadah *= 3;
                    resources.Trinium *= 3;
                    ships.PirateFighter = 6;
                    ships.JaffaTransporter = 2;
                    ships.AlkeshLightBomber = 1;
                    break;
            }

            return new PlayerBase
            {
                Name = faction.ShortName + "-Außenposten",
                FactionId = faction.Id,
                Faction = faction,
                PlanetSector = sector,
                LastResourceUpdateUtc = nowUtc,
                Resources = resources,
                BuildingLevels = buildings,
                Ships = ships
            };
        }
    }
}
