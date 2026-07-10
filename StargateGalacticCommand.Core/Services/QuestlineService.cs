using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class QuestlineService
    {
        // Jede Fraktion bekommt eine inhaltlich eigene Kette, keine reskinnte Kopie der anderen (siehe GAMEPLAY_IDEAS 4.3 Konzept-Hinweis).
        private static readonly IList<QuestStepDefinition> Steps = new List<QuestStepDefinition>
        {
            new QuestStepDefinition("SGC_1", "SGC", 1, "Erste Kontaktaufnahme", "Ein SG-Team stößt auf erste Hinweise einer verlassenen antiken Ausgrabungsstätte.", GateMissionType.SearchArtifact, 1, 20),
            new QuestStepDefinition("SGC_2", "SGC", 2, "Spuren der Antiker", "Wiederholte Funde deuten auf eine größere antike Anlage jenseits des bekannten Gate-Netzes hin.", GateMissionType.SearchArtifact, 3, 40),
            new QuestStepDefinition("SGC_3", "SGC", 3, "Wettlauf gegen die Lucian-Allianz", "Ein Konkurrenzteam der Lucian-Allianz ist offenbar hinter denselben Artefakten her.", GateMissionType.SearchArtifact, 6, 60),
            new QuestStepDefinition("SGC_4", "SGC", 4, "Das Erbe der Antiker", "Der vollständige Datensatz eines antiken Außenpostens wird endlich gesichert.", GateMissionType.SearchArtifact, 10, 80),

            new QuestStepDefinition("Jaffa_1", "Jaffa", 1, "Erste Erkundung feindlichen Territoriums", "Ein Jaffa-Trupp kundschaftet ein von Goa'uld kontrolliertes Gebiet für einen künftigen Aufstand aus.", GateMissionType.RiskAnalysis, 1, 20),
            new QuestStepDefinition("Jaffa_2", "Jaffa", 2, "Widerstandszellen kontaktieren", "Verstreute Jaffa-Widerstandszellen werden aufgespürt und für die Sache gewonnen.", GateMissionType.RiskAnalysis, 3, 40),
            new QuestStepDefinition("Jaffa_3", "Jaffa", 3, "Waffenlager der Goa'uld aufspüren", "Ein gut bewachtes Waffenlager wird lokalisiert – der Schlüssel für einen erfolgreichen Aufstand.", GateMissionType.RiskAnalysis, 6, 60),
            new QuestStepDefinition("Jaffa_4", "Jaffa", 4, "Befreiung einer Jaffa-Garnison", "Eine ganze Garnison schließt sich dem Aufstand gegen die falschen Götter an.", GateMissionType.RiskAnalysis, 10, 80),

            new QuestStepDefinition("Tokra_1", "Tok’ra", 1, "Erstes Auskundschaften einer Goa'uld-Adresse", "Ein Tok'ra-Agent analysiert eine Gate-Adresse im Herrschaftsbereich eines Systemherren.", GateMissionType.AnalyzeAddress, 1, 20),
            new QuestStepDefinition("Tokra_2", "Tok’ra", 2, "Tarnidentität aufbauen", "Eine glaubwürdige Tarnidentität innerhalb der Goa'uld-Hierarchie wird etabliert.", GateMissionType.AnalyzeAddress, 3, 40),
            new QuestStepDefinition("Tokra_3", "Tok’ra", 3, "Zugang zum inneren Kreis", "Der Agent erreicht den inneren Kreis des Systemherren und dessen Kommandostruktur.", GateMissionType.AnalyzeAddress, 6, 60),
            new QuestStepDefinition("Tokra_4", "Tok’ra", 4, "Sabotage der Kommandostruktur", "Die Infiltration gipfelt in der gezielten Sabotage der Goa'uld-Kommandostruktur.", GateMissionType.AnalyzeAddress, 10, 80),

            new QuestStepDefinition("Lucian_1", "Lucian", 1, "Erste Warenlieferung sichern", "Eine erste Lieferung wird erfolgreich über eine neue Route abgewickelt.", GateMissionType.SecureResources, 1, 20),
            new QuestStepDefinition("Lucian_2", "Lucian", 2, "Route vor Konkurrenten abschirmen", "Die Route wird gegen rivalisierende Schmugglerbanden abgesichert.", GateMissionType.SecureResources, 3, 40),
            new QuestStepDefinition("Lucian_3", "Lucian", 3, "Bestechung lokaler Wächter", "Lokale Wachposten entlang der Route werden auf die Gehaltsliste gesetzt.", GateMissionType.SecureResources, 6, 60),
            new QuestStepDefinition("Lucian_4", "Lucian", 4, "Monopol über die Schmuggelroute", "Die Lucian-Allianz kontrolliert die Route nun vollständig.", GateMissionType.SecureResources, 10, 80),
        };

        public IReadOnlyList<QuestStepDefinition> GetStepsForFaction(string factionShortName)
        {
            return Steps.Where(s => s.FactionShortName == factionShortName).OrderBy(s => s.Order).ToList();
        }

        public bool IsStepUnlocked(QuestStepDefinition step, IReadOnlyList<QuestStepDefinition> factionSteps, ISet<string> completedStepKeys)
        {
            if (step == null) throw new ArgumentNullException("step");
            if (factionSteps == null) throw new ArgumentNullException("factionSteps");
            if (completedStepKeys == null) throw new ArgumentNullException("completedStepKeys");
            return factionSteps.Where(s => s.Order < step.Order).All(s => completedStepKeys.Contains(s.Key));
        }

        public bool IsStepComplete(QuestStepDefinition step, int successfulMissionCount)
        {
            if (step == null) throw new ArgumentNullException("step");
            return successfulMissionCount >= step.RequiredSuccessfulCount;
        }

        public QuestlineStepProgress TryCompleteStep(User user, QuestStepDefinition step, bool isUnlocked, bool isComplete, ISet<string> completedStepKeys, DateTime nowUtc)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (step == null) throw new ArgumentNullException("step");
            if (completedStepKeys == null) throw new ArgumentNullException("completedStepKeys");
            if (!isUnlocked || !isComplete || completedStepKeys.Contains(step.Key)) return null;
            return new QuestlineStepProgress { UserId = user.Id, StepKey = step.Key, CompletedAtUtc = nowUtc };
        }
    }
}
