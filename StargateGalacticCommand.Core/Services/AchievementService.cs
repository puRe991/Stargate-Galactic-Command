using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class AchievementService
    {
        private static readonly IList<AchievementDefinition> Definitions = new List<AchievementDefinition>
        {
            new AchievementDefinition("Explorer1", "Kundschafter", "5 Gate-Adressen entdeckt.", AchievementGoalType.DiscoveredAddresses, 5),
            new AchievementDefinition("Explorer2", "Sternenkartograph", "15 Gate-Adressen entdeckt.", AchievementGoalType.DiscoveredAddresses, 15),
            new AchievementDefinition("Explorer3", "Wegbereiter der Galaxis", "40 Gate-Adressen entdeckt.", AchievementGoalType.DiscoveredAddresses, 40),
            new AchievementDefinition("Warrior1", "Erster Sieg", "1 gewonnene Raumschlacht.", AchievementGoalType.SpaceCombatVictories, 1),
            new AchievementDefinition("Warrior2", "Kriegsheld", "10 gewonnene Raumschlachten.", AchievementGoalType.SpaceCombatVictories, 10),
            new AchievementDefinition("Warrior3", "Schrecken der Sterne", "30 gewonnene Raumschlachten.", AchievementGoalType.SpaceCombatVictories, 30),
            new AchievementDefinition("Versatile1", "Vielseitiger Agent", "Alle 7 Gate-Missionstypen mindestens einmal erfolgreich abgeschlossen.", AchievementGoalType.DistinctMissionTypesCompleted, 7),
            new AchievementDefinition("Ally1", "Verbündeter", "Einer Allianz beigetreten.", AchievementGoalType.AllianceMembership, 1),
            new AchievementDefinition("Colonist1", "Kolonist", "Erste Kolonie gegründet.", AchievementGoalType.ColoniesFounded, 1),
            new AchievementDefinition("Colonist2", "Architekt neuer Welten", "3 Kolonien gegründet.", AchievementGoalType.ColoniesFounded, 3),
            new AchievementDefinition("Trader1", "Händler", "5 Marktgeschäfte abgeschlossen.", AchievementGoalType.MarketTrades, 5),
            new AchievementDefinition("Trader2", "Handelsfürst", "25 Marktgeschäfte abgeschlossen.", AchievementGoalType.MarketTrades, 25)
        };

        public IReadOnlyList<AchievementDefinition> GetAll()
        {
            return Definitions.ToList();
        }

        public AchievementDefinition Get(string key)
        {
            var definition = Definitions.FirstOrDefault(d => d.Key == key);
            if (definition == null) throw new ArgumentException("Unbekannte Kodex-Errungenschaft.", "key");
            return definition;
        }

        // No resource reward by design (see GAMEPLAY_IDEAS.md balancing note): achievements are a cosmetic collection layer, not a second economy.
        public bool TryUnlock(AchievementDefinition definition, AchievementProgress progress, int currentProgress, DateTime nowUtc)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (progress == null) throw new ArgumentNullException("progress");
            if (progress.UnlockedAtUtc.HasValue) return false;
            if (currentProgress < definition.GoalAmount) return false;
            progress.UnlockedAtUtc = nowUtc;
            return true;
        }
    }
}
