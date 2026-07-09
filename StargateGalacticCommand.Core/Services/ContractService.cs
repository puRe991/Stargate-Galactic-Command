using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class ContractService
    {
        private static readonly IList<ContractDefinition> Definitions = new List<ContractDefinition>
        {
            new ContractDefinition("DailyGateMissions", "Gate-Auftrag", "3 Gate-Missionen abschließen.", ContractGoalType.GateMissions, 3, false, new BuildCost { Naquadah = 150, Trinium = 80 },
                new Dictionary<string, string> { { "SGC", "SGC-Erkundungsauftrag" }, { "Jaffa", "Jaffa-Ehrenauftrag" }, { "Tok’ra", "Tok’ra-Geheimoperation" }, { "Lucian", "Lucian-Deal" } }),
            new ContractDefinition("DailyFleetMissions", "Flottenbefehl", "2 Flottenmissionen abschließen (Transport, Erkundung, Stationierung oder Bergung).", ContractGoalType.FleetMissions, 2, false, new BuildCost { Supplies = 120, Energy = 60 }),
            new ContractDefinition("DailyTrade", "Handelskontrakt", "1 Marktgeschäft abschließen (kaufen oder verkaufen).", ContractGoalType.Trades, 1, false, new BuildCost { Intel = 5 }),
            new ContractDefinition("WeeklyGateMissions", "Wöchentlicher Erkundungsauftrag", "15 Gate-Missionen in dieser Woche abschließen.", ContractGoalType.GateMissions, 15, true, new BuildCost { Naquadah = 600, Trinium = 400, Intel = 10 })
        };

        public IReadOnlyList<ContractDefinition> GetAll()
        {
            return Definitions.ToList();
        }

        public ContractDefinition Get(string key)
        {
            var definition = Definitions.FirstOrDefault(d => d.Key == key);
            if (definition == null) throw new ArgumentException("Unbekannter Kontrakt.", "key");
            return definition;
        }

        public DateTime GetPeriodStart(ContractDefinition definition, DateTime nowUtc)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (!definition.IsWeekly) return nowUtc.Date;
            int daysSinceMonday = ((int)nowUtc.DayOfWeek + 6) % 7;
            return nowUtc.Date.AddDays(-daysSinceMonday);
        }

        public void Claim(ContractDefinition definition, ContractProgress progress, PlayerBase playerBase, int currentProgress, DateTime nowUtc)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (progress == null) throw new ArgumentNullException("progress");
            if (playerBase == null) throw new ArgumentNullException("playerBase");
            if (progress.ClaimedAtUtc.HasValue) throw new InvalidOperationException("Kontrakt wurde für diesen Zeitraum bereits abgeschlossen.");
            if (currentProgress < definition.GoalAmount) throw new InvalidOperationException("Zielwert des Kontrakts ist noch nicht erreicht.");

            playerBase.Resources.Naquadah += definition.Reward.Naquadah;
            playerBase.Resources.Trinium += definition.Reward.Trinium;
            playerBase.Resources.Supplies += definition.Reward.Supplies;
            playerBase.Resources.Energy += definition.Reward.Energy;
            playerBase.Resources.Personnel += definition.Reward.Personnel;
            playerBase.Resources.Intel += definition.Reward.Intel;
            progress.ClaimedAtUtc = nowUtc;
        }
    }
}
