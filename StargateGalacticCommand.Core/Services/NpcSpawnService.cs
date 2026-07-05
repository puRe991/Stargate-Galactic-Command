using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class NpcSpawnService
    {
        private const string NpcNamePrefix = "Goa'uld Kriegsherr #";

        public int EnsureNpcPresence(DbContext db, DateTime nowUtc, int targetNpcCount)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (targetNpcCount < 0) throw new ArgumentOutOfRangeException(nameof(targetNpcCount), "Target NPC count must not be negative.");
            if (targetNpcCount == 0) return 0;

            var users = db.Set<User>();
            var bases = db.Set<PlayerBase>();
            var sectors = db.Set<PlanetSector>();

            int activeHumanPlayers = users.Count(u => !u.IsNpc && u.Bases.Any());
            if (activeHumanPlayers >= targetNpcCount) return 0;

            int existingNpcBases = bases.Count(b => b.User.IsNpc);
            int settlementSectorCount = sectors.Count(s => s.IsSettlementSector);
            int maxNpcBases = CalculateNpcBaseLimit(settlementSectorCount, targetNpcCount);
            int desiredNpcBases = Math.Min(targetNpcCount - activeHumanPlayers, maxNpcBases);
            int missingNpcBases = Math.Max(0, desiredNpcBases - existingNpcBases);
            if (missingNpcBases == 0) return 0;

            var freeSettlementSectors = sectors
                .Include(s => s.PlayerBase)
                .Where(s => s.IsSettlementSector && s.PlayerBase == null)
                .OrderBy(s => s.PlanetId)
                .ThenBy(s => s.Number)
                .Take(missingNpcBases)
                .ToList();

            if (freeSettlementSectors.Count == 0) return 0;

            var faction = db.Set<Faction>().OrderBy(f => f.Id).FirstOrDefault(f => f.Name.Contains("Goa"))
                ?? db.Set<Faction>().OrderBy(f => f.Id).FirstOrDefault()
                ?? throw new InvalidOperationException("NPC spawning requires at least one faction.");

            var usedNames = users.Where(u => u.IsNpc).Select(u => u.UserName).ToList().ToHashSet(StringComparer.OrdinalIgnoreCase);
            int created = 0;
            foreach (var sector in freeSettlementSectors)
            {
                var profile = CreateProfileForIndex(existingNpcBases + created);
                var npcName = CreateUniqueNpcName(usedNames);
                var user = new User
                {
                    UserName = npcName,
                    Email = CreateNpcEmail(npcName),
                    PasswordHash = "NPC_ACCOUNT_DISABLED",
                    PasswordSalt = Guid.NewGuid().ToString("N"),
                    IsNpc = true,
                    Faction = faction,
                    CreatedAtUtc = nowUtc,
                    ResearchLevels = profile.ResearchLevels
                };

                user.Bases.Add(new PlayerBase
                {
                    Name = npcName + " Basis",
                    User = user,
                    Faction = faction,
                    PlanetSector = sector,
                    Resources = profile.Resources,
                    BuildingLevels = profile.BuildingLevels,
                    Ships = profile.Ships,
                    LastResourceUpdateUtc = nowUtc
                });

                users.Add(user);
                usedNames.Add(npcName);
                created++;
            }

            db.SaveChanges();
            return created;
        }

        private static int CalculateNpcBaseLimit(int settlementSectorCount, int targetNpcCount)
        {
            if (settlementSectorCount <= 0 || targetNpcCount <= 0) return 0;
            return Math.Min(targetNpcCount, Math.Max(1, settlementSectorCount / 3));
        }

        private static NpcDifficultyProfile CreateProfileForIndex(int index)
        {
            if (index % 5 == 4) return CreateSystemLordProfile();
            if (index % 3 == 2) return CreateWarlordProfile();
            return CreateRaiderProfile();
        }

        private static NpcDifficultyProfile CreateRaiderProfile()
        {
            return new NpcDifficultyProfile(
                new ResearchLevels { GateAddressing = 1, NaquadahEnergyTechnology = 1, ShieldTechnology = 1, Hyperdrive = 1, StaffWeaponDiscipline = 1 },
                new BuildingLevels { CommandCenter = 2, NaquadahRefinery = 2, TriniumMine = 1, SupplyDepot = 1, EnergyGenerator = 2, ResearchLab = 1, GateControlRoom = 1, DefenseRing = 1, HangarLandingZone = 1 },
                new ResourceStock { Naquadah = 1500, Trinium = 800, Supplies = 1000, Energy = 1200, Personnel = 300 },
                new BaseShips { Teltak = 2, JaffaTransporter = 1, PirateFighter = 2 });
        }

        private static NpcDifficultyProfile CreateWarlordProfile()
        {
            return new NpcDifficultyProfile(
                new ResearchLevels { GateAddressing = 2, NaquadahEnergyTechnology = 2, ShieldTechnology = 2, Hyperdrive = 2, Sensorics = 1, StaffWeaponDiscipline = 2, HatakCommandStructure = 1, JaffaWarriorCode = 1 },
                new BuildingLevels { CommandCenter = 3, NaquadahRefinery = 3, TriniumMine = 2, SupplyDepot = 2, EnergyGenerator = 3, ResearchLab = 2, GateControlRoom = 2, SensorStation = 1, DefenseRing = 2, HangarLandingZone = 2 },
                new ResourceStock { Naquadah = 3500, Trinium = 2200, Supplies = 2500, Energy = 3000, Personnel = 650 },
                new BaseShips { Teltak = 4, AlkeshLightBomber = 1, JaffaTransporter = 3, PirateFighter = 4 });
        }

        private static NpcDifficultyProfile CreateSystemLordProfile()
        {
            return new NpcDifficultyProfile(
                new ResearchLevels { GateAddressing = 3, NaquadahEnergyTechnology = 3, ShieldTechnology = 3, Hyperdrive = 3, Sensorics = 2, StaffWeaponDiscipline = 3, HatakCommandStructure = 2, JaffaWarriorCode = 2, GoauldSabotage = 1 },
                new BuildingLevels { CommandCenter = 4, NaquadahRefinery = 4, TriniumMine = 3, SupplyDepot = 3, EnergyGenerator = 4, ResearchLab = 3, GateControlRoom = 3, SensorStation = 2, DefenseRing = 3, HangarLandingZone = 3 },
                new ResourceStock { Naquadah = 7000, Trinium = 4500, Supplies = 5000, Energy = 6500, Personnel = 1200 },
                new BaseShips { Teltak = 8, AlkeshLightBomber = 3, JaffaTransporter = 5, PirateFighter = 8 });
        }

        private static string CreateUniqueNpcName(ISet<string> usedNames)
        {
            for (int i = 1; i <= 10000; i++)
            {
                var candidate = NpcNamePrefix + i;
                if (!usedNames.Contains(candidate)) return candidate;
            }

            throw new InvalidOperationException("No unique NPC name is available.");
        }

        private static string CreateNpcEmail(string npcName)
        {
            var suffix = npcName.Substring(NpcNamePrefix.Length);
            return "npc-" + suffix + "@stargate.local";
        }

        private sealed class NpcDifficultyProfile
        {
            public NpcDifficultyProfile(ResearchLevels researchLevels, BuildingLevels buildingLevels, ResourceStock resources, BaseShips ships)
            {
                ResearchLevels = researchLevels;
                BuildingLevels = buildingLevels;
                Resources = resources;
                Ships = ships;
            }

            public ResearchLevels ResearchLevels { get; }
            public BuildingLevels BuildingLevels { get; }
            public ResourceStock Resources { get; }
            public BaseShips Ships { get; }
        }
    }
}
