using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class BuildQueueService
    {
        private readonly BuildingCatalogService _catalog;
        private readonly ResourceService _resources;

        public BuildQueueService(BuildingCatalogService catalog, ResourceService resources)
        {
            _catalog = catalog ?? throw new ArgumentNullException("catalog");
            _resources = resources ?? throw new ArgumentNullException("resources");
        }

        public BuildQueueItem StartBuild(PlayerBase playerBase, BuildingType buildingType, DateTime nowUtc)
        {
            ValidateBase(playerBase);
            CompleteFinishedBuilds(playerBase, nowUtc);
            if (playerBase.BuildQueue.Any()) throw new InvalidOperationException("Es kann pro Basis nur ein Gebäude gleichzeitig gebaut werden.");

            int currentLevel = playerBase.BuildingLevels.GetLevel(buildingType);
            if (currentLevel < 0) throw new InvalidOperationException("Gebäudelevel darf nicht negativ sein.");
            var cost = _catalog.CalculateCost(buildingType, currentLevel);
            _resources.Spend(playerBase.Resources, cost);

            var item = new BuildQueueItem
            {
                PlayerBaseId = playerBase.Id,
                PlayerBase = playerBase,
                BuildingType = buildingType,
                TargetLevel = currentLevel + 1,
                StartedAtUtc = nowUtc,
                CompletesAtUtc = nowUtc.AddSeconds(_catalog.CalculateBuildSeconds(buildingType, currentLevel, playerBase.BuildingLevels.CommandCenter))
            };
            playerBase.BuildQueue.Add(item);
            return item;
        }

        public int CompleteFinishedBuilds(PlayerBase playerBase, DateTime nowUtc)
        {
            ValidateBase(playerBase);
            int completed = 0;
            foreach (var item in playerBase.BuildQueue.Where(q => q.CompletesAtUtc <= nowUtc).OrderBy(q => q.CompletesAtUtc).ToList())
            {
                int current = playerBase.BuildingLevels.GetLevel(item.BuildingType);
                if (item.TargetLevel > current) playerBase.BuildingLevels.SetLevel(item.BuildingType, item.TargetLevel);
                playerBase.BuildQueue.Remove(item);
                completed++;
            }
            return completed;
        }

        private static void ValidateBase(PlayerBase playerBase)
        {
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            if (playerBase.Resources == null) throw new ArgumentException("Base has no resources.", "playerBase");
            if (playerBase.BuildingLevels == null) throw new ArgumentException("Base has no building levels.", "playerBase");
            if (playerBase.BuildQueue == null) throw new ArgumentException("Base has no build queue.", "playerBase");
        }
    }
}
