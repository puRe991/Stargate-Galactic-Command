# Gameplay-Ideen: kurzfristige Verbesserungen & Langzeit-Motivation

Dieses Dokument sammelt ausgearbeitete Ideen für Verbesserungen an bestehenden
Systemen sowie Mechaniken für Langzeit-Motivation. Es ergänzt `ROADMAP.md`
(dort geht es um den großen Sprung zu Echtzeit-Away-Teams) um Dinge, die sich
**innerhalb des aktuellen Text-/Formular-MVC-Modells** umsetzen lassen, ohne
auf SignalR/2D-Client zu warten. Jeder Punkt nennt den Ist-Zustand im Code,
das Konzept und eine grobe Umsetzungsskizze, damit sich daraus direkt Tickets
schneiden lassen.

## 1. Kurzfristige Gameplay-Verbesserungen

### 1.1 Kontextsensitive Gate-Missionen — ✅ umgesetzt (Bonus-Variante)
- **Ist-Zustand (vor Umsetzung)**: `GateMissionType` (`Explore`, `SecureResources`,
  `SearchArtifact`, `DiplomaticContact`, `RiskAnalysis`, `AnalyzeAddress`,
  `FoundColony`) wird in `GameController.StartGateMission` unabhängig von der
  Fraktion des Spielers angeboten; `GateMissionService` kennt nur den Typ, nicht
  `user.Faction`.
- **Konzept**: Jede Startfraktion (Tau'ri/SGC, Freie Jaffa, Tok'ra, Lucian
  Alliance) bekommt 1–2 exklusive Missionstypen bzw. abweichende
  Erfolgschancen/Belohnungen für existierende Typen:
  - Tau'ri/SGC: Bonus auf `SearchArtifact` (Technologiefokus), Zugriff auf
    `DiplomaticContact` mit anderen Kulturen zuerst.
  - Freie Jaffa: Bonus auf `RiskAnalysis`/Kampfmissionen, eigener Typ
    "Befreiungsoperation" (befreit versklavte Jaffa als Personnel-Bonus).
  - Tok'ra: Bonus auf `AnalyzeAddress`/Spionage-lastige Missionen, Zugriff auf
    Infiltrationsvarianten von `DiplomaticContact`.
  - Lucian Alliance: Bonus auf `SecureResources`, eigener Typ "Schmuggeloperation"
    (höheres Risiko, höhere Ressourcenbeute, Rufabzug bei anderen Fraktionen).
- **Umsetzung**: `GateMissionService` bekommt eine Methode
  `GetAvailableMissionTypes(Faction)`, die die Basisliste filtert/erweitert;
  Erfolgsformel in `CalculateOutcome`-artigen Methoden um einen
  faktionsabhängigen Modifikator ergänzen (ähnlich `FactionModifierService`,
  der schon für Verteidigung/Wirtschaft existiert – hier konsequent
  wiederverwenden statt neue Parallelstruktur).
- **Balancing**: Boni als Erfolgschance-Modifikator (+5–10 %) statt Freischaltung
  ganzer neuer Ressourcenklassen, um Startfraktionen nicht zu sehr zu spreizen.
- **Tatsächliche Umsetzung**: Statt exklusiver Missionstypen (Balancing-Hinweis
  empfahl ausdrücklich Modifikator statt Exklusivität) gibt es jetzt
  `FactionModifierService.GetGateMissionScoreBonus(Faction, GateMissionType)`
  mit einem festen `+4`-Bonus auf den Erfolgs-Score aus
  `GateMissionService.CompleteMission` für die jeweilige
  Fraktionsspezialität: SGC → `SearchArtifact`, Freie Jaffa →
  `RiskAnalysis`, Tok'ra → `AnalyzeAddress`, Lucian Alliance →
  `SecureResources`. Der Bonus wird auf Basis von `playerBase.Faction`
  angewendet (bereits über `LoadCurrentBase` geladen, keine zusätzliche
  Query nötig) und wirkt direkt auf die Erfolgsschwelle (28 = Erfolg, 20 =
  Teilerfolg), wodurch Grenzfälle für die Spezialfraktion häufiger zu einem
  vollen Erfolg (z. B. `ArtifactLeadFound = true` statt nur Teilerfolg)
  werden. Die Missionsauswahl im Gate-Raum (`GateRoom.cshtml`) markiert die
  Spezialität der eigenen Fraktion mit einem ★. `GateMissionService` erhält
  `FactionModifierService` als optionalen Konstruktorparameter (Default:
  neue Instanz), damit bestehende Testaufrufe ohne Änderung weiterlaufen.
  Exklusive Missionstypen pro Fraktion (Befreiungsoperation,
  Schmuggeloperation, …) sind bewusst nicht Teil dieser ersten Umsetzung –
  eigenes, größeres Ticket.

### 1.2 Espionage-Gegenmaßnahmen: Köder & Falschinformationen — ✅ umgesetzt
- **Ist-Zustand (vor Umsetzung)**: `CounterIntelligenceLevel` (`Low`/`Guarded`/`Hardened`/
  `Lockdown`) existiert bereits und wird in `EspionageService` als reiner
  Abwehr-Multiplikator genutzt (Spionageerfolg sinkt, `SpyDefenseResult`
  meldet ggf. Enttarnung des Spions).
- **Konzept**: Ein aktives Gegenmittel statt nur passivem Multiplikator:
  Spieler können auf `Hardened`/`Lockdown` einen "Ködervorrat" hinterlegen
  (z. B. künstlich überhöhte Flottenzahlen), der Spionageberichten gezielt
  falsche Werte unterschiebt. Der bespitzelte Spieler bekommt eine Meldung
  ("Verdacht auf Falschinformation"), sieht aber nicht, welche Werte
  manipuliert wurden – Bluff-Mechanik.
- **Umsetzung**: neues Feld `DecoyProfile` (JSON oder eigene Tabelle mit
  Ressourcen-/Flottenwerten) an `PlayerBase`/`User` hängen; in
  `EspionageService.ResolveMission` bei erfolgreichem Spionageangriff auf eine
  Basis mit aktivem Köder und `CounterIntelligenceLevel >= Hardened` eine
  Zufallschance einbauen, den `IntelligenceReport` mit den Köderwerten statt
  der echten Werte zu befüllen. Kosten: Ködervorrat muss aus Intel-Ressource
  "aufgeladen" werden (limitierte, verbrauchbare Ressource, kein Dauerbuff).
- **Balancing**: Aufdeckbar machen – wiederholte Angriffe mit hoher eigener
  Spionagestufe sollten die Täuschung mit steigender Wahrscheinlichkeit
  durchschauen, sonst wird Spionage komplett entwertet.
- **Tatsächliche Umsetzung**: Eigene Tabelle `DecoyProfile` (1:1 an
  `PlayerBase`) statt JSON-Feld, mit Fake-Ressourcenwerten und einem
  Fake-Schiffs-Gesamtwert sowie einem verbrauchbaren `Charges`-Zähler
  (max. 3). `EspionageService.ArmDecoy` "lädt" pro Aufruf genau einen
  Einsatz gegen `DecoyChargeIntelCost` (40 Intel) auf – kein Dauerbuff,
  exakt wie gefordert. `EspionageMission` bekommt ein neues Feld
  `TargetCounterIntelligenceLevel`, das bereits bei `StartMission`
  gesetzt wird (der Wert wurde vorher berechnet, aber verworfen). In
  `CreateReport` greift die Täuschung nur, wenn der Köder aktiv ist,
  Charges > 0 sind und die Stufe mindestens `Hardened` ist;
  `CalculateDeceptionChance` startet bei 60 % und sinkt um 3 Prozentpunkte
  pro Sensorik-/Tarntechnologie-Punkt des Angreifers (Boden bei 10 %) –
  erfüllt den Balancing-Hinweis direkt als Formel statt als vage Absicht.
  Bei Erfolg wird der Bericht mit den Köderwerten befüllt und
  `IntelligenceReport.IsSuspectedDecoy = true` gesetzt; der **Angreifer**
  sieht im Bericht einen vagen Hinweis auf mögliche Falschinformation
  (nicht der Verteidiger – die Bluff-Mechanik funktioniert nur, wenn der
  Spion selbst im Unklaren bleibt, welche Werte manipuliert wurden). Neuer
  Abschnitt „Köderprofil“ auf der Geheimdienst-Seite zum Aufladen; dabei
  auch eine vorbestehende Lücke behoben, dass diese Seite `TempData`-
  Meldungen (Erfolg/Fehler) bisher gar nicht anzeigte. Abgedeckt durch 7
  neue Tests (`EspionageServiceTests`); Köder-Aufladen-Formular und
  korrekt sichtbare Fehlermeldung bei zu wenig Intel manuell gegen die
  laufende App verifiziert.

### 1.3 Dynamische Sektorkontrolle: Einfluss-Zerfall — ✅ umgesetzt
- **Ist-Zustand (vor Umsetzung)**: `SectorControl` speichert nur `ControlledAtUtc`, kein
  Verfallsmechanismus; `SectorClaim` hat `StartedAtUtc`/`CompletesAtUtc` für
  den *Erwerb*, aber Kontrolle danach ist statisch/dauerhaft in
  `LocalSectorService`.
- **Konzept**: Kontrolle erfordert kontinuierliche Präsenz/Aktivität statt
  einmaligem Claim. Ohne "Nachweis-Aktivität" (z. B. laufende lokale Aktion,
  Truppenpräsenz, oder simple X-Tage-Inaktivität des kontrollierenden
  Spielers) sinkt der Einfluss schrittweise, bis der Sektor wieder neutral
  wird und neu beansprucht werden kann.
- **Umsetzung**: `SectorControl` um `LastReinforcedAtUtc` erweitern;
  periodischer Hintergrundjob (oder Berechnung on-demand beim Laden der
  Planetenseite, analog zur bestehenden Lazy-Berechnung in
  `LocalSectorService.CalculateInfluence`) reduziert Einfluss pro Tag ohne
  Reinforcement. Reinforcement = neue leichtgewichtige Aktion ("Präsenz
  bestätigen"), die z. B. eine kleine Personnel-/Energiekosten hat.
- **Balancing**: Zerfallsrate so wählen, dass aktive aber nicht
  permanent-online Spieler (1x/Tag einloggen) Kontrolle halten können –
  verhindert Abwertung zu reinem Log-in-Zwang, aber auch dauerhaftes
  Ersteinnehmer-Privileg.
- **Tatsächliche Umsetzung**: `SectorControl.LastReinforcedAtUtc` neu
  ergänzt, gesetzt bei Erstübernahme (`LocalSectorService.CompleteClaim`)
  und bei Eroberung durch lokalen Kampf (`LocalCombatService.Resolve`).
  `LocalSectorService.CalculateSectorInfluenceWeight` liefert vollen Wert
  (1.0) innerhalb von `DecayGracePeriodHours` (24h) seit letzter Präsenz,
  fällt danach linear bis `DecayReleaseAfterHours` (96h/4 Tage) auf 0 ab –
  `CalculateInfluence` gewichtet jeden kontrollierten Sektor damit statt
  ihn pauschal mit vollen 15 Punkten zu zählen. Bei Erreichen der
  Freigabefrist wird der Sektor beim nächsten Laden der Planetenseite
  automatisch neutral (`GameController.GameView` entfernt abgelaufene
  `SectorControl`-Einträge, bevor die Übersicht gebaut wird) und ist damit
  wieder frei beanspruchbar. Neue leichtgewichtige Aktion „Präsenz
  bestätigen“ (`GameController.ReinforceSector` /
  `LocalSectorService.Reinforce`) setzt `LastReinforcedAtUtc` zurück; nur
  der kontrollierende Spieler darf sie auslösen. Planetenseite zeigt pro
  Sektor eine neue „Präsenz“-Spalte mit Countdown bis zur Freigabe. Der
  passive Produktionsbonus (`CalculateBonus`) bleibt bewusst ungedämpft und
  fällt erst komplett weg, sobald der Sektor tatsächlich freigegeben wird –
  nur der Einfluss-/Ranglistenwert zerfällt graduell, um die Wirtschaft
  nicht mit einem zweiten, unabhängigen Verfallstimer zu verkomplizieren.
  Abgedeckt durch 6 neue/angepasste Tests in `LocalSectorServiceTests`.

### 1.4 Handelsrouten statt Einzeltrades
- **Ist-Zustand**: `PlanetMarketOrder`/`PlanetMarketService` bilden
  Einzelangebote mit `ExpiresAtUtc` ab – jeder Trade ist ein manueller,
  einmaliger Vorgang.
- **Konzept**: "Handelsroute" als wiederkehrender, automatisierter Auftrag
  zwischen zwei eigenen Basen oder mit einem Handelspartner: alle N Stunden
  wird automatisch ein Trade zu vorher festgelegten Konditionen ausgeführt,
  ohne dass der Spieler jedes Mal manuell bestätigen muss. Fügt dem Spiel
  einen "Set-and-forget"-Idle-Layer hinzu, der Motivation zwischen Logins
  erzeugt.
- **Abfangrisiko**: Handelsrouten, die Sektor-/Systemgrenzen überschreiten,
  können von anderen Spielern (insbesondere Lucian Alliance mit Rollenfokus
  "Piraterie") abgefangen werden – ähnliche Mechanik wie `SpaceCombatService`-
  Angriffe, aber Ziel ist eine Transportroute statt einer Basis. Erfolgreiches
  Abfangen liefert einen Teil der transportierten Ressourcen als Beute
  (vergleichbar `Loot`-Logik in `SpaceCombatService`).
- **Umsetzung**: neues Modell `TradeRoute` (Quelle, Ziel, Ressourcen,
  Intervall, Eskorte/Schiffstyp, aktiv/pausiert) plus Scheduler-Logik
  analog zu `BuildQueueService`/`ResearchQueueService`, die beim
  Übersicht-Laden fällige Routen abwickelt.
- **Balancing**: Eskorte (Schiffe/Verteidigung) sollte Abfangchance senken,
  damit Route-Sicherheit eine echte Entscheidung ist, nicht nur ein Passiv-Feature.

### 1.5 Trümmerfeld-Recycling (Bergungsflotten) — ✅ umgesetzt
- **Ist-Zustand (vor Umsetzung)**: `DebrisField` wird bereits nach jedem `SpaceCombatService`-
  Kampf erzeugt (`CreateDebris`) und in der Übersicht angezeigt
  (`DebrisFields = ... Where(!IsRecycled)`), **aber es gibt aktuell keine
  Aktion, um ein Feld tatsächlich einzusammeln** – `IsRecycled` wird nirgends
  auf `true` gesetzt. Das ist also eine reine Anzeige ohne Gameplay-Payoff.
- **Konzept**: Bergungsschiffe (bestehender oder neuer `ShipType`, z. B.
  Recycler/Bergungsschiff) fliegen wie eine Flottenmission zum Trümmerfeld,
  sammeln Naquadah/Trinium proportional zur Bergungskapazität ein und bringen
  es zurück – analog zur bereits vorhandenen Flugzeit-/Distanzlogik in
  `SpaceCombatService.CalculateDistance/CalculateFlightSeconds`.
- **Umsetzung**: neue Missionsart in `FleetMissionType` ("Recycle"), Service-
  Methode `StartRecycle(origin, debrisField, shipCount, now)` analog zu
  `StartAttack`, die bei Ankunft `DebrisField.IsRecycled = true` setzt und
  Ressourcen der Herkunftsbasis gutschreibt (gedeckelt durch Frachtkapazität
  der eingesetzten Schiffe).
- **Balancing**: Trümmerfelder sollten nach X Stunden automatisch verfallen
  (despawnen), damit Recycling ein Zeitfenster/Wettlauf ist und nicht beliebig
  aufgeschoben werden kann – erzeugt zusätzlichen PvP-Anreiz um Kampfzonen.
  *(Noch offen – nicht Teil der ersten Umsetzung, siehe unten.)*
- **Tatsächliche Umsetzung**: `FleetMissionType.Recycle` ergänzt;
  `FleetService.StartRecycle` startet eine Bergungsflotte zu einem
  `DebrisField` (Distanz-/Treibstofflogik wie bei `Start`/`StartExploration`);
  `FleetService.Complete` sammelt bei Ankunft Naquadah/Trinium proportional
  bis zur Frachtkapazität aller eingesetzten Schiffe ein, reduziert das Feld
  entsprechend und setzt `IsRecycled = true`, sobald es leer ist – bei zu
  geringer Kapazität bleibt ein Rest für einen weiteren Flug stehen. Die
  Beute wird erst bei Rückkehr der Basis gutgeschrieben (`AddCargo` auf
  `OriginBase.Resources`), nicht am fremden Zielort. Neue Route
  `GameController.StartRecycle` sowie ein Formular auf der Seite „Flotte
  senden“ (`SendFleet.cshtml`) machen die Aktion spielbar; die
  Trümmerfeld-Übersicht (`CombatReports.cshtml`) zeigt jetzt Basisname und
  Ort statt der rohen ID. Abgedeckt durch vier neue Tests in
  `ShipyardAndFleetServiceTests`. Der automatische Verfall von
  Trümmerfeldern (siehe Balancing-Hinweis) ist noch nicht umgesetzt.

## 2. Langzeit-Motivation / Meta-Progression

### 2.1 Prestige/Ascension-Mechanik ("Erleuchtung") — ✅ umgesetzt
- **Konzept**: Ab einem definierten Machtlevel (z. B. Score-Schwelle aus
  `RankingService`) kann ein Spieler freiwillig "aufsteigen": Basis wird auf
  Startwerte zurückgesetzt, im Gegenzug gibt es einen permanenten,
  kleinen Produktions-/Forschungsbonus ("Spur von Ancient-Wissen") sowie ein
  sichtbares Prestige-Abzeichen in Ranglisten/Profil. Lore-konform bleibt
  echte Ancient-Technologie weiterhin selten – der Bonus ist symbolisch/klein,
  kein Gameplay-Reset-Zwang.
- **Umsetzung**: `User` um `AscensionLevel`/`AscensionCount` erweitern;
  `EconomyService`/`FactionModifierService` (dort existiert bereits die
  Modifikator-Infrastruktur) um einen zusätzlichen, kumulativen
  Ascension-Multiplikator ergänzen. Reset-Aktion räumt `BuildingLevels`,
  `ResearchLevels`, Ressourcen, behält aber Nachrichten/Statistik-Historie
  (Prestige soll sich nicht wie Datenverlust anfühlen).
- **Balancing**: Bonus pro Ascension klein genug halten (~2–5 %), damit
  Ascension eine Wahl bleibt und nicht zur Pflicht wird; Cooldown/Mindestlevel
  verhindert Ascension-Farming in kurzen Zyklen.
- **Tatsächliche Umsetzung**: `User.AscensionCount`/`LastAscendedAtUtc` neu
  ergänzt. `AscensionService.CalculateProductionBonus` gibt `+3 %` pro
  Erleuchtung, gedeckelt bei 10 Erleuchtungen (max. `+30 %`) – am unteren
  Ende der empfohlenen Spanne, damit es eine Wahl bleibt. Voraussetzungen
  (`ValidateCanAscend`): Basis-Score (`RankingService.CalculateBaseScore`)
  von mindestens 15.000 *und* 24 h seit der letzten Erleuchtung, verhindert
  Kurzzyklus-Farming. Reset (`AscensionService.Ascend`) setzt Forschung,
  Gebäude, Schiffe und Ressourcen auf Startwerte zurück, lässt aber Berichte,
  Kodex-Fortschritt, Kontrakt-Fortschritt, bekannte Gate-Adressen,
  Missionsteams und Allianzmitgliedschaft unangetastet – bewusst nicht
  zurückgesetzt: Sektorkontrolle (eigener Scope-Schnitt, siehe unten). Statt
  `FactionModifierService` (das ist reine Fraktionslogik ohne User-Bezug)
  ist der Bonus direkt in `EconomyService.CalculateHourlyProduction`
  eingebaut: eine neue 5-Parameter-Überladung
  (`..., int ascensionCount`) multipliziert alle Ressourcentypen zusätzlich
  mit dem Ascension-Faktor, während `ApplyOfflineProduction` ihn automatisch
  aus `playerBase.User.AscensionCount` zieht – dadurch übernehmen **alle**
  bestehenden Aufrufstellen der Offline-Produktion den Bonus automatisch,
  ohne dass eine einzige der rund zehn Stellen im `GameController` angepasst
  werden musste (nur der reine Anzeige-Aufruf in der Übersicht wurde
  ergänzt). Sichtbares Prestige-Abzeichen (✦ pro Erleuchtung) in der
  Rangliste über neues Feld `PlayerRankingEntry.AscensionCount`. Neue
  Übersichts-Sektion „Erleuchtung“ mit Score-Fortschritt, Bonusanzeige und
  Bestätigungsdialog vor dem Reset. Abgedeckt durch 12 neue Tests
  (`AscensionServiceTests` + Ergänzung in `EconomyServiceTests`);
  Ablehnung bei zu niedrigem Score manuell gegen die laufende App
  verifiziert (echte Erfolgsfreischaltung würde einen künstlich
  hochgespielten Account erfordern, daher nicht end-to-end getestet).

### 2.2 Season-Pässe / wöchentliche Storyline
- **Konzept**: Statt alle 300+ Gate-Adressen aus `GalaxyGeneratorService` von
  Anfang an gleichwertig verfügbar zu machen, wird ein rotierender
  "Fokusbereich" (z. B. eine Handvoll neu "aktivierter" Adressen pro Woche)
  mit eigener Mini-Storyline und Bonusbelohnungen eingeführt. Erzeugt
  planbaren, wiederkehrenden Content-Drip statt eines einmaligen riesigen
  Contentbergs.
- **Umsetzung**: neues Modell `SeasonEvent` (Zeitraum, beteiligte
  `GateAddress`-IDs, Bonusfaktor auf Missionsbelohnungen, Abschluss-
  Fortschritt); Anzeige als eigener Reiter/Banner in der Übersicht.
- **Balancing**: Season-Inhalte sollten kosmetisch/Fortschritts-Boni bringen,
  keine exklusiven Power-Vorteile, die verpasste Seasons dauerhaft
  benachteiligen (Fear-of-missing-out vermeiden, das reine Log-in-Zwang erzeugt).

### 2.3 Achievements / Lore-Kodex — ✅ umgesetzt
- **Konzept**: Ein sich füllender Kodex (entdeckte Gate-Adressen, besiegte
  Gegnerfraktionen, abgeschlossene Missionstypen, kontaktierte Kulturen) als
  Sammelanreiz unabhängig von reiner Score-Progression. Gut geeignet für
  Spieler, die nicht PvP-fokussiert sind ("Explorer"-Spielertyp).
- **Umsetzung**: `KnownGateAddress` liefert bereits Rohdaten für einen
  Entdeckungs-Fortschrittsbalken; zusätzlich neues Modell
  `AchievementProgress` (UserId, AchievementKey, erreicht am). Trigger-Punkte
  an bestehenden Stellen einhängen: `GateMissionService`-Abschluss,
  `SpaceCombatService.Resolve`, `AllianceService`-Beitritt usw.
- **Balancing**: Kleine kosmetische/Titel-Belohnungen statt Ressourcenboni,
  damit Achievements nicht zum verdeckten zweiten Wirtschaftssystem werden.
- **Tatsächliche Umsetzung**: Gleiches Muster wie bei den Kontrakten (2.4):
  Fortschritt wird **live aus bestehenden Tabellen berechnet**
  (`KnownGateAddresses`, `SpaceCombatReports`, `GateMissionReports`,
  `AllianceMembers`, `TradeReports`) statt Trigger-Hooks in jeden einzelnen
  Service (`GateMissionService`, `SpaceCombatService`, `AllianceService`, …)
  einzubauen – keiner dieser Services musste angefasst werden.
  `AchievementProgress` speichert nur `UnlockedAtUtc` (kein Zählerstand, da
  Errungenschaften anders als Kontrakte dauerhaft sind, kein Reset-Zyklus).
  Katalog mit 12 Einträgen in `AchievementService` über 6 Zielkategorien
  (`AchievementGoalType`): entdeckte Adressen (3 Stufen), gewonnene
  Raumschlachten (3 Stufen), alle 7 Gate-Missionstypen mindestens einmal
  erfolgreich abgeschlossen, Allianzbeitritt, gegründete Kolonien (2 Stufen),
  abgeschlossene Markttransaktionen (2 Stufen). Ohne Ressourcenbelohnung wie
  im Balancing-Hinweis gefordert – reines Sammel-/Titel-Feature. Freischaltung
  passiert automatisch beim nächsten Seitenaufruf (`GameController.
  BuildAchievementStatuses`, aufgerufen aus dem gemeinsamen Overview-Builder)
  und erzeugt einen normalen Report-Eintrag ("Kodex-Eintrag freigeschaltet:
  …") zur Benachrichtigung. Neue Seite „Kodex“ (`Codex.cshtml`) zeigt alle
  Einträge inkl. Fortschrittsbalken und Freischaltdatum. Abgedeckt durch 5
  neue Tests (`AchievementServiceTests`); Vollzyklus (Registrierung → Kodex-
  Seite zeigt sofort Fortschritt „1 / 5“ für die Startadresse) manuell gegen
  die laufende App verifiziert.

### 2.4 Tägliche/wöchentliche Kontrakte (fraktionsspezifisch) — ✅ umgesetzt
- **Konzept**: Kurze, planbare Auftragslisten pro Fraktion (SGC-Aufträge,
  Jaffa-Ehrenaufträge, Tok'ra-Geheimoperationen, Lucian-Alliance-Deals), die
  sich täglich/wöchentlich erneuern und kleine, aber verlässliche
  Belohnungen bringen. Zentraler Hebel für "10 Minuten am Tag reichen, um
  voranzukommen" – wichtig für Spieler ohne Zeit für Dauer-Online-Strategie.
- **Umsetzung**: neues Modell `ContractDefinition` (Fraktion, Zieltyp z. B.
  "X Gate-Missionen abschließen", "Y Ressourcen handeln") plus
  `ContractProgress` pro Nutzer; Reset-Job täglich/wöchentlich, ähnlich wie
  bei Season-Events.
- **Balancing**: Kontrakte dürfen nicht die einzige sinnvolle Ressourcenquelle
  werden, sonst wird Nicht-Einloggen bestraft statt Einloggen belohnt – als
  Bonus obendrauf, nicht als Ersatz für Kernwirtschaft designen.
- **Tatsächliche Umsetzung**: Statt eines separaten `AchievementProgress`-artigen
  Zähler-Modells, der bei jeder Aktion in jedem Service hochgezählt werden
  müsste, wird der Fortschritt **live aus bereits bestehenden Report-Tabellen
  berechnet** (`GateMissionReports`, `FleetReports`, `TradeReports` mit
  `CreatedAtUtc >= Periodenbeginn`) – kein einziger bestehender Service
  musste angefasst werden. `ContractProgress` speichert dadurch nur, *ob*
  ein Kontrakt für einen Zeitraum bereits abgeholt wurde, nicht den
  Zählerstand selbst. Katalog mit vier Kontrakten in `ContractService`
  (Muster wie `ResearchCatalogService`: statische `ContractDefinition`-Liste,
  keine DB-Tabelle): `DailyGateMissions` (3 Gate-Missionen), `DailyFleetMissions`
  (2 Flottenmissionen), `DailyTrade` (1 Markttransaktion), `WeeklyGateMissions`
  (15 Gate-Missionen/Woche). `ContractDefinition.GetDisplayName(Faction)`
  liefert die fraktionsspezifischen Namen (SGC-Erkundungsauftrag,
  Jaffa-Ehrenauftrag, Tok'ra-Geheimoperation, Lucian-Deal) für denselben
  zugrundeliegenden Kontrakt statt eigener Kontrakte pro Fraktion – kleinerer
  Scope, gleiche Wirkung. Neue Seite „Kontrakte“ (`Contracts.cshtml`) mit
  Abhol-Button pro Kontrakt (`GameController.ClaimContract`), Navigation
  ergänzt. Abgedeckt durch 9 neue Tests (`ContractServiceTests`); Vollzyklus
  Registrierung → Kontrakte-Seite → Fraktionsname → Ablehnung bei
  unerreichtem Ziel manuell gegen die laufende App verifiziert. Automatischer
  Reset ist implizit durch die periodenbasierte Berechnung gelöst (kein
  Cronjob nötig, da `PeriodStartUtc` bei jedem Aufruf neu berechnet wird).

### 2.5 Skilltrees pro Charakterrolle (vorgezogen aus Roadmap-Phase 4)
- **Konzept**: Auch ohne den 2D-Client aus der Roadmap lässt sich ein
  reines Text-Skilltree-System für die drei Rollen (Militär/Wissenschaft/
  Diplomatie) vorziehen, das Missionsboni beeinflusst (z. B. Militär-Skill
  erhöht `RiskAnalysis`/Kampferfolg, Wissenschaft erhöht `SearchArtifact`/
  Forschungsgeschwindigkeit, Diplomatie erhöht `DiplomaticContact`-Erfolg und
  Allianz-Interaktionen).
- **Umsetzung**: setzt ein leichtgewichtiges `Character`-Modell voraus (in
  Roadmap Phase 1 ohnehin geplant) – lässt sich aber vorab als reine
  Punkte-Verteilung auf `User` (ohne Avatar/2D-Bezug) realisieren, sodass
  Phase 1 später nur noch die visuelle Schicht ergänzt statt die Mechanik neu
  zu bauen.
- **Balancing**: Skillpunkte über Aktivität statt Kauf vergeben (z. B. pro
  abgeschlossener Mission), damit es Progression statt Pay2Win ist.

## 3. Social / Allianzen / PvP

### 3.1 Allianz-Kriege mit klaren Zielen — ✅ umgesetzt
- **Ist-Zustand (vor Umsetzung)**: `AllianceService`/`AllianceRankingEntry` bilden bereits
  Mitgliedschaft und Ranglisten ab, aber Sektorkontrolle (`SectorControl`)
  ist rein individuell, kein Allianz-Bezug.
- **Konzept**: Allianzen können eine Gate-Adresse/einen Planeten als
  "Kriegsziel" markieren; gemeinsames Halten von Sektorkontrolle (Summe über
  alle Mitglieder) über einen Zeitraum X schaltet einen Allianz-weiten
  temporären Fraktionsbonus frei (z. B. via `FactionModifierService`
  zeitlich begrenzt erhöht).
- **Umsetzung**: `SectorControl`/`PlanetInfluence` um Allianz-Aggregation
  erweitern (Summe der Kontrolle aller Mitglieder einer Allianz auf einem
  Planeten); neues Modell `AllianceWarGoal` (Alliance, Planet/Zielsektor,
  Start, benötigte Dauer, Status).
- **Balancing**: Boni zeitlich befristet und moderat halten, damit kleine
  Allianzen nicht dauerhaft abgehängt werden – evtl. gestaffelte Ziele nach
  Allianzgröße.
- **Tatsächliche Umsetzung**: Neues Modell `AllianceWarGoal` (Allianz,
  Zielplanet, benötigte Sektoranzahl, benötigte durchgehende Haltezeit,
  Status Active/Achieved/Abandoned, `HoldStreakStartedAtUtc` für die
  laufende Serie). `AllianceWarService.CalculateRequiredSectors` staffelt
  den Schwellenwert nach Allianzgröße (`Math.Ceiling(Mitglieder * 0.5)`,
  Minimum 2) statt eines festen Werts. Fixe Parameter für die erste Version:
  `RequiredHours = 24`. `EvaluateProgress` läuft bei jedem Seitenaufruf
  eines Allianzmitglieds über `GameController.EvaluateAllianceWarState`
  (gleiches "kein Cronjob nötig"-Muster wie bei Sektorverfall/Kontrakten/
  Achievements): Summe der von *allen* Allianzmitgliedern kontrollierten
  Sektoren auf dem Zielplaneten wird live gezählt; unterschreitet sie den
  Schwellenwert, reißt die Serie ab (`HoldStreakStartedAtUtc = null`) statt
  nur den Fortschritt zu pausieren – bewusst kompromisslos, damit "Halten"
  wörtlich genommen wird. Bewusste Vereinfachung ggü. Konzept: Die
  Belohnung ist eine **einmalige Ressourcengutschrift** pro Mitgliedsbasis
  (`AllianceWarService.VictoryRewardPerMember`) statt eines zeitlich
  befristeten Produktionsmultiplikators über `FactionModifierService` – das
  hätte Änderungen an `EconomyService` und all seinen Aufrufstellen
  erfordert. Nur Gründer/Offiziere dürfen Kriegsziele erklären oder
  aufgeben (`GameController.DeclareWarGoal`/`AbandonWarGoal`, Prüfung wie in
  `AllianceService.Accept`). Neuer Abschnitt auf der Allianzen-Seite zeigt
  aktives Ziel, Fortschritt und Serienstatus. Abgedeckt durch 10 neue Tests
  in `AllianceWarServiceTests`; Vollzyklus (Allianz gründen → Kriegsziel
  erklären → Fortschrittsanzeige "0/2 Sektoren") manuell gegen die laufende
  App verifiziert.

### 3.2 Mentoren-System für neue Spieler
- **Konzept**: Erfahrene Allianzmitglieder erhalten Belohnungen (Ressourcen,
  Kodex-Eintrag, Titel) fürs Betreuen von Neulingen – z. B. wenn ein
  gementeeter Spieler innerhalb von X Tagen bestimmte Meilensteine erreicht
  (erste Gate-Mission, erste Sektorkontrolle). Senkt Einstiegshürde für neue
  Spieler und gibt Veteranen einen nicht-kompetitiven Progressionsanreiz.
- **Umsetzung**: `AllianceMember` um optionales `MentorUserId` erweitern;
  Meilenstein-Tracking kann dieselbe Infrastruktur wie Achievements (2.3)
  nutzen.
- **Balancing**: Mentor-Belohnung an echten Fortschritt des Schützlings
  koppeln (nicht nur Beitritt), um Missbrauch durch Zweitaccounts zu
  begrenzen (z. B. Cap pro Woche/IP-Heuristik, falls vorhanden).

### 3.3 Diplomatie-Layer zwischen Allianzen
- **Konzept**: Formale Nichtangriffspakte/Handelsabkommen zwischen Allianzen
  als echte Spielmechanik: aktive Pakte verhindern/erschweren gegenseitige
  Angriffe (`SpaceCombatService.ValidateAttack` prüft zusätzlich Paktstatus)
  bzw. reduzieren Marktsteuern (`TradeTaxRule` existiert schon für
  Handelssteuern) zwischen verbündeten Allianzen.
- **Umsetzung**: neues Modell `AllianceDiplomacyStatus` (Allianz A, Allianz B,
  Status: Neutral/Pakt/Krieg, seit wann); Prüfungen in
  `SpaceCombatService.ValidateAttack` und `PlanetMarketService` ergänzen.
- **Balancing**: Pakt-Bruch sollte spürbare Konsequenzen haben (z. B.
  Reputationsverlust, kurzfristiger Vertrauensmalus), sonst ist Diplomatie
  nur ein kostenloses An/Aus-Feature ohne Gewicht.

## 4. Content-/Lore-getriebene Ideen

### 4.1 Serverweite Ori-/Replikatoren-Events — ✅ umgesetzt
- **Konzept**: Periodisches, serverweites Bedrohungsereignis (z. B. eine
  "Replikatoren-Invasion" auf zufällig gewählten Gate-Adressen), das für
  seine Dauer PvP-Anreize senkt und Kooperationsanreize erhöht: Spieler aller
  Fraktionen können gemeinsam an Abwehrmissionen teilnehmen, serverweiter
  Fortschrittsbalken schaltet bei Erfolg temporäre Boni für alle frei.
- **Umsetzung**: neues Modell `WorldEvent` (Typ, Start/Ende, Zielwert,
  aktueller Fortschritt); neue Gate-Missionsvariante "Abwehrmission", die
  bei Abschluss den globalen Fortschritt erhöht statt nur individuelle
  Belohnung zu geben. Admin-/Zeitplan-getriggert, ähnlich Season-Events (2.2).
- **Balancing**: Event darf nicht permanent aktiv sein (Erschöpfung), klare
  Cooldown-Fenster zwischen Events; Belohnung sollte Teilnahme statt nur
  "Sieg" belohnen, damit auch kleinere Spieler etwas davon haben.
- **Tatsächliche Umsetzung**: Kein Admin-/Zeitplan-Trigger nötig – gleiches
  "kein Cronjob"-Muster wie bei allen anderen Live-Feature dieser Session:
  `WorldEventService.TryStartEvent` wird bei jedem Seitenaufruf eines
  beliebigen Spielers geprüft (`GameController.EvaluateWorldEventState`) und
  startet automatisch ein neues Event, sobald keines aktiv ist und die
  Cooldown-Frist (`CooldownHoursBetweenEvents` = 24h) seit dem letzten
  Ereignisende verstrichen ist; Typ alterniert deterministisch zwischen
  `ReplicatorInvasion` und `OriIncursion` statt zufällig. Statt einer neuen
  Gate-Missionsvariante (hätte `GateMissionService` angefasst) gibt es eine
  eigenständige "Verteidigungsbeitrag"-Aktion
  (`GameController.ContributeToWorldEvent`): kostet Versorgungsgüter +
  Personal, erhöht den globalen Fortschritt um einen festen Betrag, mit
  eigenem Cooldown pro Spieler (`ContributionCooldownHours` = 4h) gegen
  Spam. Läuft das Zeitfenster (`EventDurationHours` = 48h) ab, bevor das
  Ziel (`GoalProgress` = 300) erreicht ist, endet das Event als `Failed`
  ohne Bestrafung – wird das Ziel erreicht, als `Succeeded`. Belohnung
  (`ParticipationReward`) geht an **jeden**, der mindestens einmal
  beigetragen hat (nicht nur an "Gewinner"), exakt wie im Balancing-Hinweis
  gefordert; `WorldEventContribution.RewardGrantedAtUtc` verhindert
  Doppelauszahlung auch wenn mehrere Spieler gleichzeitig die
  Abschluss-Prüfung auslösen. Neue Sektion „Weltbedrohung“ auf der
  Übersichtsseite zeigt Fortschritt, Zeitfenster und eigenen Beitrag.
  Abgedeckt durch 15 neue Tests (`WorldEventServiceTests`); Vollzyklus
  (Event startet automatisch beim ersten Seitenaufruf → Beitrag leisten →
  Cooldown-Sperre beim zweiten Versuch) manuell gegen die laufende App
  verifiziert.

### 4.2 Zufällige Anomalien auf Gate-Adressen (Ancient/Asgard-Encounter) — ✅ umgesetzt
- **Konzept**: Seltene High-Value-Encounter bei `AnalyzeAddress`/`Explore`-
  Missionen: Ancient-Ruinen oder Asgard-Wracks als sehr seltene
  Zusatzergebnisse, die kleine, einmalige Boni geben (z. B. Forschungs-
  Schub, kosmetischer Kodex-Eintrag) statt dauerhafte ZPM-artige Ressourcen –
  bleibt damit konsistent zur Lore-Leitplanke "Ancient/Asgard/ZPM bleibt selten".
- **Umsetzung**: in `GateMissionService` bei Missionsauflösung eine niedrige
  Zufallschance (~1–2 %) auf ein `GateMissionOutcome`-Zusatzflag oder
  separates `AnomalyEncounter`-Ergebnis einbauen, das einen eigenen
  Report-Text und einmaligen Bonus erzeugt.
- **Balancing**: Bewusst nicht wiederholbar pro Adresse (einmal gefunden,
  danach "erschöpft"), um Farming zu verhindern und den Seltenheitswert zu
  erhalten.
- **Tatsächliche Umsetzung**: `GateAddress.AnomalyFound` (bool) markiert eine
  Adresse als erschöpft. `GateMissionService.CompleteMission` würfelt bei
  jedem nicht fehlgeschlagenen `Explore`- oder `AnalyzeAddress`-Abschluss
  mit `AnomalyChance` (2 %) über eine neue private `TryTriggerAnomaly`-
  Methode; ein optionaler `Random`-Parameter (Default `Random.Shared`) macht
  das deterministisch testbar. Trifft der Wurf, wird zufällig zwischen
  `GateAnomalyType.AncientRuin` (Intel-Fund) und `AsgardWreck` (Naquadah-Fund)
  gewählt, ein einmaliger Ressourcenbonus gutgeschrieben und die Adresse als
  erschöpft markiert – kein dauerhafter Multiplikator, bleibt damit
  konsistent zur Lore-Leitplanke. Der Fund wird auf `GateMissionReport.
  AnomalyType` gespeichert und im Gate-Raum mit ✧ markiert; da jeder
  Bericht dauerhaft in der Berichtsliste stehen bleibt, fungiert das
  bereits als der im Konzept erwähnte "kosmetische Kodex-Eintrag" – kein
  Ausbau des Achievement-Systems nötig. Abgedeckt durch 5 neue Tests
  (Auslösung bei Erfolg, kein Auslösen bei verfehltem Wurf, keine Auslösung
  bei anderen Missionstypen oder bereits erschöpfter Adresse, keine
  Auslösung bei Fehlschlag); Gate-Raum-Rendering mit dem neuen Schema
  manuell gegen die laufende App verifiziert (ein echter Fund ließ sich
  wegen der 2-Minuten-Missionsdauer nicht in Echtzeit end-to-end auslösen).

### 4.3 Fraktionsspezifische Questlines
- **Konzept**: Eigene, inhaltlich unterschiedliche (nicht nur reskinnte)
  Missionsreihen pro Startfraktion mit eigenen NPC-Kontakten und
  Entscheidungen (z. B. Tok'ra-Questline um Infiltration eines Goa'uld-
  Systems vs. Lucian-Alliance-Questline um Kontrolle einer Schmuggelroute).
  Stärkt Wiederspielwert bei Fraktionswechsel/Zweitaccount und macht die vier
  Fraktionen spielerisch differenzierter als nur unterschiedliche Zahlen-Mods.
- **Umsetzung**: baut auf Kontextsensitive Missionen (1.1) auf; zusätzlich
  ein leichtgewichtiges Questline-Modell (geordnete Kette von
  `GateMission`-Vorlagen mit Freischaltbedingungen) statt aller Missionen
  gleichzeitig verfügbar.
- **Balancing**: Questline-Abschluss sollte Fortschritt/Lore bringen, keine
  klar überlegene Ressourcenquelle gegenüber anderen Fraktionen – sonst wird
  Fraktionswahl zur reinen Powerchoice statt Rollenspiel-Entscheidung.

## Priorisierungsvorschlag

Kein Umbau des Datenmodells nötig, sofort startbar:
1. **1.5 Trümmerfeld-Recycling** ✅ – Datenmodell existiert bereits vollständig,
   es fehlt nur die Aktion/der Service-Aufruf. Kleinster Aufwand, klarer
   Mehrwert.
2. **1.1 Kontextsensitive Gate-Missionen** ✅ – nutzt bestehende
   `FactionModifierService`-Infrastruktur, keine neuen Tabellen nötig.

Mittlerer Aufwand, hoher Bindungseffekt:
3. **2.4 Tägliche/wöchentliche Kontrakte** ✅
4. **2.3 Achievements/Lore-Kodex** ✅
5. **1.3 Einfluss-Zerfall** ✅

Größerer Aufwand / mehr Design-Abstimmung nötig, aber hohe Langzeitwirkung:
6. **3.1 Allianz-Kriege** ✅, **2.1 Ascension** ✅, **4.1 Weltevents** ✅

Alle sechs priorisierten Punkte sind damit umgesetzt. Danach zusätzlich
umgesetzt: **4.2 Ancient/Asgard-Anomalien** ✅, **1.2 Espionage-Köder** ✅.
Verbleibend aus dem Gesamt-Backlog (nicht priorisiert): 1.4
(Handelsrouten), 2.2 (Season-Pässe), 2.5 (Rollen-Skilltrees), 3.2
(Mentoren-System), 3.3 (Diplomatie-Layer), 4.3 (Fraktionsspezifische
Questlines).
