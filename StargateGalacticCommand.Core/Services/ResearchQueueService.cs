using System;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class ResearchQueueService
    {
        private readonly ResearchCatalogService _catalog;
        private readonly ResourceService _resources;
        private readonly FactionModifierService _modifiers;
        private readonly SkillTreeService _skillTree;

        public ResearchQueueService(ResearchCatalogService catalog, ResourceService resources, FactionModifierService modifiers, SkillTreeService skillTree = null)
        {
            _catalog = catalog ?? throw new ArgumentNullException("catalog");
            _resources = resources ?? throw new ArgumentNullException("resources");
            _modifiers = modifiers ?? throw new ArgumentNullException("modifiers");
            _skillTree = skillTree ?? new SkillTreeService();
        }

        public ResearchQueueItem StartResearch(User user, PlayerBase playerBase, ResearchType type, DateTime nowUtc, CharacterSkills skills = null)
        {
            Validate(user, playerBase);
            CompleteFinishedResearch(user, nowUtc);
            if (user.ResearchQueue.Any()) throw new InvalidOperationException("Es kann pro Spieler nur eine Forschung gleichzeitig laufen.");
            if (playerBase.BuildingLevels.ResearchLab < 1) throw new InvalidOperationException("Forschung benötigt Forschungslabor Level 1.");

            var definition = _catalog.Get(type);
            if (definition.FactionShortName != null && (user.Faction == null || !string.Equals(definition.FactionShortName, user.Faction.ShortName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Diese Forschung ist für die gewählte Fraktion nicht verfügbar.");
            if (definition.Prerequisite.HasValue && user.ResearchLevels.GetLevel(definition.Prerequisite.Value) < 1)
                throw new InvalidOperationException("Voraussetzung nicht erfüllt.");

            int currentLevel = user.ResearchLevels.GetLevel(type);
            var cost = _catalog.CalculateCost(type, currentLevel);
            _resources.Spend(playerBase.Resources, cost);
            double speedMultiplier = _modifiers.GetResearchSpeedMultiplier(user.Faction) * _skillTree.GetResearchSpeedMultiplier(skills);
            int seconds = _catalog.CalculateResearchSeconds(type, currentLevel, playerBase.BuildingLevels.ResearchLab, speedMultiplier);
            var item = new ResearchQueueItem { UserId = user.Id, User = user, ResearchType = type, TargetLevel = currentLevel + 1, StartedAtUtc = nowUtc, CompletesAtUtc = nowUtc.AddSeconds(seconds) };
            user.ResearchQueue.Add(item);
            return item;
        }

        public int CompleteFinishedResearch(User user, DateTime nowUtc)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.ResearchLevels == null) throw new ArgumentException("User has no research levels.", "user");
            if (user.ResearchQueue == null) throw new ArgumentException("User has no research queue.", "user");
            int completed = 0;
            foreach (var item in user.ResearchQueue.Where(q => q.CompletesAtUtc <= nowUtc).OrderBy(q => q.CompletesAtUtc).ToList())
            {
                int current = user.ResearchLevels.GetLevel(item.ResearchType);
                if (item.TargetLevel > current) user.ResearchLevels.SetLevel(item.ResearchType, item.TargetLevel);
                user.ResearchQueue.Remove(item);
                completed++;
            }
            return completed;
        }

        private static void Validate(User user, PlayerBase playerBase)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            if (user.ResearchLevels == null) throw new ArgumentException("User has no research levels.", "user");
            if (user.ResearchQueue == null) throw new ArgumentException("User has no research queue.", "user");
            if (playerBase.Resources == null) throw new ArgumentException("Base has no resources.", "playerBase");
            if (playerBase.BuildingLevels == null) throw new ArgumentException("Base has no building levels.", "playerBase");
        }
    }
}
