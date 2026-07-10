using System;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class WorldEventService
    {
        public const int EventDurationHours = 48;
        public const int CooldownHoursBetweenEvents = 24;
        public const int GoalProgress = 300;
        public const int ContributionAmount = 10;
        public const int ContributionCooldownHours = 4;

        public static readonly BuildCost ContributionCost = new BuildCost { Supplies = 50, Personnel = 10 };
        public static readonly BuildCost ParticipationReward = new BuildCost { Naquadah = 200, Trinium = 150, Intel = 15 };

        public string GetName(WorldEventType type)
        {
            return type == WorldEventType.ReplicatorInvasion ? "Replikatoren-Invasion" : "Ori-Einfall";
        }

        public string GetDescription(WorldEventType type)
        {
            return type == WorldEventType.ReplicatorInvasion
                ? "Replikatorschwärme greifen mehrere Gate-Adressen gleichzeitig an. Alle Fraktionen sind aufgerufen, gemeinsam Verteidigungsressourcen zu entsenden."
                : "Ori-Prioren mobilisieren Gefolgschaften entlang unbekannter Gate-Adressen. Nur eine gemeinsame Abwehr aller Fraktionen kann den Vormarsch stoppen.";
        }

        // No new event may start while one is Active or the cooldown since the last one ended hasn't elapsed, so the threat isn't permanently active.
        public WorldEvent TryStartEvent(WorldEvent activeEvent, WorldEvent lastResolvedEvent, DateTime nowUtc)
        {
            if (activeEvent != null) return null;
            if (lastResolvedEvent != null && lastResolvedEvent.ResolvedAtUtc.HasValue)
            {
                double hoursSinceEnded = (nowUtc - lastResolvedEvent.ResolvedAtUtc.Value).TotalHours;
                if (hoursSinceEnded < CooldownHoursBetweenEvents) return null;
            }
            var type = lastResolvedEvent != null && lastResolvedEvent.Type == WorldEventType.ReplicatorInvasion ? WorldEventType.OriIncursion : WorldEventType.ReplicatorInvasion;
            return new WorldEvent { Type = type, Status = WorldEventStatus.Active, StartedAtUtc = nowUtc, EndsAtUtc = nowUtc.AddHours(EventDurationHours), GoalProgress = GoalProgress, CurrentProgress = 0 };
        }

        public bool ResolveIfExpired(WorldEvent worldEvent, DateTime nowUtc)
        {
            if (worldEvent == null || worldEvent.Status != WorldEventStatus.Active) return false;
            if (nowUtc < worldEvent.EndsAtUtc) return false;
            worldEvent.Status = worldEvent.CurrentProgress >= worldEvent.GoalProgress ? WorldEventStatus.Succeeded : WorldEventStatus.Failed;
            worldEvent.ResolvedAtUtc = nowUtc;
            return true;
        }

        public void Contribute(WorldEvent worldEvent, WorldEventContribution contribution, PlayerBase playerBase, DateTime nowUtc)
        {
            if (worldEvent == null) throw new ArgumentNullException("worldEvent");
            if (contribution == null) throw new ArgumentNullException("contribution");
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            if (worldEvent.Status != WorldEventStatus.Active || nowUtc >= worldEvent.EndsAtUtc) throw new InvalidOperationException("Dieses Weltevent ist nicht mehr aktiv.");
            if (contribution.LastContributedAtUtc.HasValue)
            {
                double hoursSinceLast = (nowUtc - contribution.LastContributedAtUtc.Value).TotalHours;
                if (hoursSinceLast < ContributionCooldownHours) throw new InvalidOperationException("Der nächste Verteidigungsbeitrag ist erst in " + Math.Ceiling(ContributionCooldownHours - hoursSinceLast) + "h möglich.");
            }
            if (playerBase.Resources.Supplies < ContributionCost.Supplies || playerBase.Resources.Personnel < ContributionCost.Personnel) throw new InvalidOperationException("Nicht genug Versorgungsgüter oder Personal für einen Verteidigungsbeitrag.");

            playerBase.Resources.Supplies -= ContributionCost.Supplies;
            playerBase.Resources.Personnel -= ContributionCost.Personnel;
            worldEvent.CurrentProgress += ContributionAmount;
            contribution.TotalAmount += ContributionAmount;
            contribution.LastContributedAtUtc = nowUtc;

            if (worldEvent.CurrentProgress >= worldEvent.GoalProgress)
            {
                worldEvent.Status = WorldEventStatus.Succeeded;
                worldEvent.ResolvedAtUtc = nowUtc;
            }
        }

        // Rewards participation, not just victory (per design note): anyone who contributed at least once gets the same reward once the event succeeds.
        public bool TryGrantParticipationReward(WorldEvent worldEvent, WorldEventContribution contribution, ResourceStock resources, DateTime nowUtc)
        {
            if (worldEvent == null || contribution == null || resources == null) return false;
            if (worldEvent.Status != WorldEventStatus.Succeeded) return false;
            if (contribution.TotalAmount <= 0) return false;
            if (contribution.RewardGrantedAtUtc.HasValue) return false;

            resources.Naquadah += ParticipationReward.Naquadah;
            resources.Trinium += ParticipationReward.Trinium;
            resources.Intel += ParticipationReward.Intel;
            contribution.RewardGrantedAtUtc = nowUtc;
            return true;
        }
    }
}
