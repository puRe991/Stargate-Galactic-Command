using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(GameDbContext context)
        {
            if (context == null) return;
            context.Database.EnsureCreated();
            SeedFactions(context);
            SeedStartPlanet(context);
            context.SaveChanges();
        }
        private static void SeedFactions(GameDbContext context)
        {
            if (context.Factions.Any()) return;
            context.Factions.AddRange(
                new Faction { Id = 1, Name = "Tau’ri / Stargate Command", ShortName = "SGC" },
                new Faction { Id = 2, Name = "Freie Jaffa", ShortName = "Jaffa" },
                new Faction { Id = 3, Name = "Tok’ra", ShortName = "Tok’ra" },
                new Faction { Id = 4, Name = "Lucian Alliance", ShortName = "Lucian" });
        }
        private static void SeedStartPlanet(GameDbContext context)
        {
            if (context.Planets.Any(p => p.Name == "P3X-742")) return;
            var planet = new Planet { Name = "P3X-742", Galaxy = "Milchstraße", Type = "Grenzwelt", StargateActive = true, Status = "geteilt" };
            string[] names = { "Stargate-Lichtung", "lokale Siedlung", "Siedlungssektor 3", "Siedlungssektor 4", "Siedlungssektor 5", "Siedlungssektor 6", "Triniumfeld", "alte Goa’uld-Ruine", "Naquadah-Vorkommen", "Orbitalkorridor" };
            for (int i = 0; i < names.Length; i++) planet.Sectors.Add(new PlanetSector { Number = i + 1, Name = names[i], IsSettlementSector = i + 1 >= 2 && i + 1 <= 6 });
            context.Planets.Add(planet);
        }
    }
}
