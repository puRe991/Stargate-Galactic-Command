using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class ShipyardService
    {
        private readonly ResourceService _resources;

        public ShipyardService(ResourceService resources)
        {
            _resources = resources ?? throw new ArgumentNullException("resources");
        }

        public IList<ShipTypeDefinition> GetAll()
        {
            return new List<ShipTypeDefinition>
            {
                Create(ShipType.SmallTransporter, "Kleiner Transporter", "SGC", true, 180, 90, 140, 40, 12, 900, 500, 80, 2),
                Create(ShipType.SupplyShuttle, "Versorgungsshuttle", "SGC", true, 120, 70, 180, 35, 10, 720, 420, 70, 2),
                Create(ShipType.Teltak, "Tel’tak", "Jaffa", true, 160, 110, 130, 45, 14, 960, 480, 75, 2),
                Create(ShipType.JaffaTransporter, "Jaffa-Transporter", "Jaffa", true, 150, 120, 160, 45, 16, 1020, 520, 70, 2),
                Create(ShipType.CloakedTeltak, "getarntes Tel’tak", "Tok’ra", true, 140, 120, 130, 55, 12, 1080, 460, 65, 3),
                Create(ShipType.AgentTransporter, "Agententransporter", "Tok’ra", true, 110, 90, 100, 50, 8, 780, 380, 90, 2),
                Create(ShipType.SmugglerTransporter, "Schmugglertransporter", "Lucian", true, 130, 80, 150, 40, 10, 840, 450, 85, 2),
                Create(ShipType.F302, "F-302", "SGC", false, 120, 160, 40, 60, 4, 900, 60, 120, 4),
                Create(ShipType.AlkeshLightBomber, "Al’kesh leichter Bomber", "Jaffa", false, 400, 500, 180, 220, 40, 3600, 0, 45, 8),
                Create(ShipType.PirateFighter, "Piratenjäger", "Lucian", false, 100, 140, 45, 55, 5, 840, 50, 110, 4)
            };
        }

        public ShipTypeDefinition Get(ShipType type)
        {
            return GetAll().Single(definition => definition.Type == type);
        }

        public IEnumerable<ShipTypeDefinition> GetAvailableForFaction(Faction faction)
        {
            if (faction == null) throw new ArgumentNullException("faction");
            return GetAll().Where(definition => definition.FactionShortName == faction.ShortName);
        }

        public ShipyardQueueItem StartBuild(PlayerBase playerBase, ShipType shipType, int quantity, DateTime nowUtc, ResearchLevels researchLevels = null)
        {
            Validate(playerBase);
            if (quantity < 1 || quantity > 100) throw new ArgumentOutOfRangeException("quantity");
            if (playerBase.BuildingLevels.HangarLandingZone < 1) throw new InvalidOperationException("Hangar / Landezone Level 1 erforderlich.");

            CompleteFinishedBuilds(playerBase, nowUtc);
            var definition = Get(shipType);
            if (!definition.IsActive) throw new InvalidOperationException("Dieser Schiffstyp ist in Version 0.0.7 gesperrt.");
            if (definition.FactionShortName != playerBase.Faction.ShortName) throw new InvalidOperationException("Schiffstyp gehört nicht zur Fraktion.");

            // Naquadah-Reaktor-Miniaturisierung (SGC) und gestohlene Technologie (Lucian) senken Baukosten, fortgeschrittener Schiffbau (allgemein) senkt die Bauzeit.
            int costReductionLevels = researchLevels == null ? 0 : researchLevels.NaquadahReactorMiniaturization + researchLevels.StolenTechnologyIntegration;
            double costMultiplier = Math.Max(0.5, 1 - costReductionLevels * 0.01);
            double timeMultiplier = Math.Max(0.5, 1 - (researchLevels == null ? 0 : researchLevels.AdvancedShipEngineering) * 0.01);
            _resources.Spend(playerBase.Resources, Scale(definition.Cost, quantity, costMultiplier));
            var item = new ShipyardQueueItem
            {
                PlayerBaseId = playerBase.Id,
                PlayerBase = playerBase,
                ShipType = shipType,
                Quantity = quantity,
                StartedAtUtc = nowUtc,
                CompletesAtUtc = nowUtc.AddSeconds(definition.Cost.Seconds * quantity * timeMultiplier)
            };
            playerBase.ShipyardQueue.Add(item);
            return item;
        }

        public int CompleteFinishedBuilds(PlayerBase playerBase, DateTime nowUtc)
        {
            Validate(playerBase);
            int completed = 0;
            foreach (var item in playerBase.ShipyardQueue.Where(queueItem => queueItem.CompletesAtUtc <= nowUtc).OrderBy(queueItem => queueItem.CompletesAtUtc).ToList())
            {
                playerBase.Ships.Add(item.ShipType, item.Quantity);
                playerBase.ShipyardQueue.Remove(item);
                completed++;
            }
            return completed;
        }

        private static ShipTypeDefinition Create(ShipType type, string name, string faction, bool active, int naquadah, int trinium, int supplies, int energy, int personnel, int seconds, int cargo, int speed, int fuel)
        {
            return new ShipTypeDefinition { Type = type, Name = name, FactionShortName = faction, IsActive = active, Cost = new BuildCost { Naquadah = naquadah, Trinium = trinium, Supplies = supplies, Energy = energy, Personnel = personnel, Seconds = seconds }, CargoCapacity = cargo, Speed = speed, FuelPerDistance = fuel };
        }

        private static BuildCost Scale(BuildCost cost, int quantity, double costMultiplier = 1.0)
        {
            return new BuildCost { Naquadah = (int)Math.Ceiling(cost.Naquadah * quantity * costMultiplier), Trinium = (int)Math.Ceiling(cost.Trinium * quantity * costMultiplier), Supplies = (int)Math.Ceiling(cost.Supplies * quantity * costMultiplier), Energy = (int)Math.Ceiling(cost.Energy * quantity * costMultiplier), Personnel = (int)Math.Ceiling(cost.Personnel * quantity * costMultiplier), Seconds = cost.Seconds * quantity };
        }

        private static void Validate(PlayerBase playerBase)
        {
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            if (playerBase.Resources == null || playerBase.BuildingLevels == null || playerBase.Ships == null || playerBase.ShipyardQueue == null || playerBase.Faction == null) throw new ArgumentException("Basis ist unvollständig geladen.", "playerBase");
        }
    }
}
