# Stargate Galactic Command

Browserbasierter Strategie-MMO-Prototyp im OGame-Stil mit Stargate-inspirierter Lore. Spieler verwalten keine ganzen Planeten, sondern geheime Basissektoren auf gemeinsam genutzten Welten und treten gegen andere Fraktionen um Einfluss, Ressourcen und Kontrolle an.

## Inhaltsverzeichnis

- [Status dieser Version](#status-dieser-version)
- [Features im Überblick](#features-im-überblick)
- [Lore-Leitplanken](#lore-leitplanken)
- [Projektstruktur](#projektstruktur)
- [Voraussetzungen](#voraussetzungen)
- [Lokal starten](#lokal-starten)
- [Tests ausführen](#tests-ausführen)
- [Betrieb & Deployment](#betrieb--deployment)
- [Offene TODOs](#offene-todos)
- [Erledigt](#erledigt)

## Status dieser Version

Version 0.0.9 macht gemeinsame Planeten spielmechanisch aktiv:

- ASP.NET Core MVC/Razor Webanwendung
- getrennte Projekte für Web, Core, Data und Tests
- SQLite-Anbindung über Entity Framework Core
- Basismodelle für Planeten, Basissektoren, Ressourcen, Gebäude und Fraktionen
- Economy-Service mit Forschungs-, Fraktions- und lokalen Sektorboni
- friedliche Beanspruchung neutraler Ressourcenzonen auf gemeinsamen Planeten
- Sektorkontrolle, laufende lokale Aktionen, Abschlussberichte und Einflussberechnung
- Planetenseite mit Sektorstatus, kontrollierten Sektoren und Einflussrangliste
- Gebäude, Forschung, Gate-Raum und PvE-Gate-Missionen aus den Vorversionen
- Prozedural generierte Galaxie mit 300+ erforschbaren Gate-Adressen (`GalaxyGeneratorService`); zufällige Entdeckung neuer Adressen sowohl über Gate-Missionen ("Adresse analysieren") als auch über Erkundungsflüge von Schiffen ohne festes Ziel ("Fernaufklärung")
- Server-Verwaltung: mehrere unabhängige Spielwelten (`GameServer`), jede mit eigener prozedural generierter Galaxie und eigenen Accounts. Spieler wählen unter „Server auswählen“ (`/Server/Select`) eine Welt, bevor sie sich einloggen oder registrieren. Ein separater Admin-Bereich (`/Admin/Login`) erlaubt dem Betreiber, Server zu erstellen, zu starten, zu pausieren (keine neuen Registrierungen) und zu stoppen (Welt verschwindet aus der Auswahl, aktive Sitzungen werden beendet)

## Server-Verwaltung für den Betrieb

Das Admin-Passwort wird über die Konfiguration `Admin:Password` gesetzt (Standard in `appsettings.json`: `change-me`). Für echte Deployments sollte es per Umgebungsvariable überschrieben werden, z. B.:

```bash
export Admin__Password="ein-sicheres-passwort"
```

Ein Startup-Check verhindert, dass das vergessen wird: Außerhalb von `Development` bricht die Anwendung den Start mit einer Fehlermeldung ab, solange `Admin:Password` noch auf `change-me` steht; im `Development`-Profil (Standard bei `dotnet run`, siehe `Properties/launchSettings.json`) gibt es stattdessen nur eine Log-Warnung, damit lokales Entwickeln ohne zusätzliche Konfiguration funktioniert.

## Features im Überblick

Kernsysteme, implementiert als Services in `StargateGalacticCommand.Core/Services` und als Seiten unter `StargateGalacticCommand.Web/Views/Game`:

| Bereich | Beschreibung |
| --- | --- |
| Basis & Ressourcen | Gebäude, Bauwarteschlange, Ressourcenproduktion mit Fraktions- und Sektorboni |
| Forschung | Forschungskatalog und -warteschlange mit Auswirkung auf Economy und Missionen |
| Gate-Raum | PvE-Gate-Missionen (Adresse analysieren, Artefakt suchen, Risikoanalyse, Ressourcen sichern u. a.), wöchentliche Fokusadressen mit Bonusbelohnung |
| Galaxie & Flotten | Prozedural generierte Galaxie, Schiffswerft, Flottenversand, Weltraumkampf, Trümmerfeldbergung, Handelsrouten |
| Planeten & Sektoren | Gemeinsame Planeten mit beanspruchbaren Sektoren, Sektorkontrolle, Einfluss-Zerfall, Ranglisten |
| Diplomatie | Allianzen, Allianz-Kriegsziele, Mentoren-System, Marktplatz zwischen Spielern |
| Geheimdienst | Spionage sowie Gegenspionage inklusive Köderdaten |
| Meta-Progression | Kodex/Achievements, tägliche/wöchentliche Kontrakte, Erleuchtung (Prestige/Ascension), Weltevents, Charakter-Skilltrees, fraktionsspezifische Questlines |
| Kommunikation | Postfach/Nachrichten sowie Kampf- und Flottenberichte |

Eine ausführliche Historie der einzelnen Features steht im Abschnitt [Erledigt](#erledigt); geplante bzw. angedachte Erweiterungen sind in [ROADMAP.md](ROADMAP.md) und [GAMEPLAY_IDEAS.md](GAMEPLAY_IDEAS.md) gesammelt.

## Lore-Leitplanken

- Das Stargate-Programm bleibt geheim.
- Erde, Atlantis und Dakara sind als normale Spielerplaneten gesperrt.
- Stargates transportieren Teams und kleine Ausrüstung, keine Großflotten.
- Große Schiffe reisen per Hyperraum.
- Ancient-, Asgard- und ZPM-Technologie bleibt selten und wird in dieser Version noch nicht als Standardressource modelliert.
- Startfraktionen: Tau'ri/SGC, Freie Jaffa, Tok'ra und Lucian Alliance.

## Projektstruktur

```text
StargateGalacticCommand.Core   Domänenmodelle und Services
StargateGalacticCommand.Data   Entity Framework Core DbContext und DB-Initialisierung
StargateGalacticCommand.Web    ASP.NET Core MVC/Razor Anwendung
StargateGalacticCommand.Tests  Unit-Tests für Economy-Formeln, Gate-Missionen, Forschung, Bauwarteschlangen und lokale Sektoren
```

## Voraussetzungen

- Visual Studio 2022 17.8 oder neuer
- .NET SDK 8.0 oder neuer (das Repository enthält eine `global.json` mit Roll-forward auf neuere installierte SDKs)
- SQLite wird lokal über `Microsoft.EntityFrameworkCore.Sqlite` verwendet; kein separater Server ist nötig.

> Hinweis: Die Projekte zielen auf .NET 8 LTS. Wenn Visual Studio die Projekte nicht lädt, installiere das aktuelle .NET SDK und stelle sicher, dass der Workload „ASP.NET und Webentwicklung" aktiv ist.

## Lokal starten

### Windows Batch

```bat
build-and-run.bat
```

Das Skript prüft zuerst, ob ein .NET SDK 8.0 oder neuer verfügbar ist. Fehlt es, versucht das Skript eine automatische Installation: zuerst über `winget`, danach als Fallback per offiziellem `dotnet-install.ps1` in das Benutzerprofil. Anschließend stellt es NuGet-Pakete wieder her, baut die Solution und startet die Webanwendung unter `http://localhost:5000`. Beim Start öffnet es die URL automatisch im Standardbrowser. Wenn ein Fehler auftritt (z. B. fehlende Installationsrechte, Netzwerkproblem oder Buildfehler), bleibt das Konsolenfenster geöffnet, damit die Fehlermeldung lesbar bleibt.

### Manuell per .NET CLI

```bash
dotnet restore

dotnet build StargateGalacticCommand.sln

dotnet run --project StargateGalacticCommand.Web
```

Die Anwendung erstellt beim Start eine lokale SQLite-Datei `stargate-galactic-command.db`, sofern sie noch nicht existiert. Öffne anschließend die in der Konsole ausgegebene lokale URL.

## Tests ausführen

```bash
dotnet test StargateGalacticCommand.sln
```

## Betrieb & Deployment

- **EF-Core-Migrationen**: Das Schema wird über echte Migrationen (`StargateGalacticCommand.Data/Migrations`) verwaltet, nicht mehr über `EnsureCreated()`. Neue Migration nach einer Modelländerung erzeugen:
  ```bash
  dotnet ef migrations add <Name> --project StargateGalacticCommand.Data --startup-project StargateGalacticCommand.Web
  ```
  Bestehende Datenbanken, die noch vom alten `EnsureCreated()`-Fallback stammen (keine `__EFMigrationsHistory`-Tabelle), werden beim ersten Start automatisch "baselined" (die vorhandenen Migrationen werden als bereits angewendet markiert, ohne das Schema anzufassen) statt einen Fehler zu werfen.
- **SQLite im Mehrbenutzerbetrieb**: SQLite ist Single-File/Single-Writer; parallele Schreibzugriffe können sich blockieren. Die App aktiviert beim Start WAL-Journaling (`PRAGMA journal_mode=WAL`) und einen Busy-Timeout von 30 s (`Default Timeout` in der Connection-String), damit Leser und ein Schreiber nebeneinander arbeiten können und kurze Kollisionen nicht sofort fehlschlagen. Das behebt das grundsätzliche Single-Writer-Limit aber nicht – für echte Mehrspieler-Last unter Last ist der in der [ROADMAP](ROADMAP.md) (Phase 5) vorgesehene Wechsel zu PostgreSQL der richtige Schritt.
- **Backups**: `scripts/backup-sqlite.sh [db] [backup-dir] [retention-days]` erstellt über SQLites Online-Backup-API (`.backup`) eine konsistente Kopie der laufenden Datenbank (auch im WAL-Modus sicher) und räumt alte Kopien nach der Aufbewahrungsfrist auf. Beispiel für einen täglichen Cronjob steht im Skriptkopf.
- **Logging**: Strukturiertes Logging über Serilog, konfiguriert in `appsettings.json` (Abschnitt `Serilog`) – Konsole wie bisher, zusätzlich rollierende Tagesdateien unter `logs/` (14 Tage Aufbewahrung). Für produktives Monitoring lassen sich weitere Sinks (z. B. Seq, ein Log-Aggregator) rein über Konfiguration ergänzen, ohne Code zu ändern.
- **Docker**: `Dockerfile` baut ein Multi-Stage-Image (SDK zum Bauen/Publishen, `aspnet:8.0` zur Laufzeit). Die SQLite-Datei liegt im Container unter `/data` (als Volume mountbar), Port ist `8080`. `Admin__Password` muss beim Start per Umgebungsvariable gesetzt werden.
  ```bash
  docker build -t stargate-galactic-command .
  docker run -p 8080:8080 -v sgc-data:/data -e Admin__Password=ein-sicheres-passwort stargate-galactic-command
  ```
- **CI**: `.github/workflows/build-and-test.yml` baut die Solution und führt `dotnet test` bei jedem Push/PR auf `main` aus.

## Offene TODOs

- Balancing-Werte für Kosten und Produktion mit Spieldesign-Zielen abgleichen. Analyse:

  `BuildingCatalogService.CalculateCost` skaliert Baukosten mit `1.6^Level` (Forschung: `1.7^Level`), während die Produktion pro Level linear/additiv bleibt (z. B. `EconomyService`: pauschal +30 Naquadah/h je Raffinerie-Level, unabhängig vom bereits erreichten Level; Forschungseffekte analog pauschal +2 %/Level). Weil die Kosten exponentiell, der Ertrag pro Level aber konstant wächst, explodiert die Amortisationszeit je Ausbaustufe: Raffinerie-Level 0→1 kostet 60 Naquadah für +30 Naquadah/h (≈2 h Amortisation), Level 10→11 kostet ≈6.600 Naquadah für dieselben +30 Naquadah/h (≈220 h ≈ 9 Tage), Level 15→16 bereits ≈69.000 Naquadah (≈96 Tage) – nur für die Naquadah-Teilkosten, ohne Trinium/Supplies. Ab etwa Level 12–15 wird der Ausbau damit praktisch unwirtschaftlich, was der in ROADMAP.md gewünschten Langzeitprogression entgegensteht.

  Mögliche Stellschrauben (nicht umgesetzt, da eine Spieldesign-Entscheidung mit Auswirkung auf bestehende Balance-Erwartungen, u. a. den versionierten Test `CalculateCost_UsesVersion002Formula`): Kostenwachstum abflachen (z. B. 1.6 → ~1.35–1.45), und/oder Produktion pro Level leicht überproportional wachsen lassen (z. B. kleiner kombinierender Faktor statt rein linear), damit die Amortisationszeit über die Levels hinweg begrenzt bleibt statt unbegrenzt zu wachsen.

## Erledigt

- Produktionsreife-Grundlagen: echte EF-Core-Migrationen statt `EnsureCreated()`-Fallback (bestehende Datenbanken werden beim ersten Start automatisch baselined statt zu crashen), WAL-Modus + Busy-Timeout für SQLite, Serilog-Datei-/Konsolenlogging, SQLite-Backup-Skript (`scripts/backup-sqlite.sh`), Dockerfile, GitHub-Actions-CI (Build+Test) und ein Startup-Check, der das Standard-Admin-Passwort `change-me` außerhalb von `Development` blockiert statt es stillschweigend laufen zu lassen. Details unter [Betrieb & Deployment](#betrieb--deployment).
- PvP-Regeln für umkämpfte Sektoren sauber modelliert: Bewaffnete Sektorangriffe (`LocalCombatService.StartMission`) sind jetzt nur noch auf Planeten mit Status „umkämpft" erlaubt; auf dem geteilten Startplaneten und auf neutralen Planeten bleibt nur friedliche Beanspruchung (`LocalSectorService.StartClaim`) möglich. Der Anfängerschutz wurde vereinheitlicht: Lokale Kämpfe prüfen jetzt dieselbe persistierte `PlayerProtectionStatus` wie der Weltraumkampf, statt einer zweiten, abweichenden 7-Tage-Regel auf Basis von `User.CreatedAtUtc`.
- Bauwarteschlange pro Basis erlaubt jetzt bis zu `BuildQueueService.MaxQueueLength` (5) aufeinanderfolgende Aufträge statt nur einen; Kosten und Ziellevel berücksichtigen bereits wartende Aufträge desselben Gebäudetyps.
- Trümmerfeldbergung: Bergungsflotten (neue Flottenmission `FleetMissionType.Recycle`) können zu Trümmerfeldern nach Raumkämpfen geschickt werden, sammeln Naquadah/Trinium bis zur Frachtkapazität ein und liefern es an die Heimatbasis; Felder werden bei unzureichender Kapazität nur teilweise abgebaut.
- Fraktionsspezialisierung bei Gate-Missionen: Jede Startfraktion bekommt einen Erfolgs-Score-Bonus auf eine Missionsart, die ihrer Lore-Rolle entspricht (SGC → Artefakt suchen, Freie Jaffa → Risikoanalyse, Tok'ra → Adresse analysieren, Lucian Alliance → Ressourcen sichern); im Gate-Raum mit ★ markiert.
- Tägliche/wöchentliche Kontrakte: neue Seite „Kontrakte" mit vier wiederkehrenden Aufträgen (Gate-Missionen, Flottenmissionen, Markthandel täglich; Gate-Missionen wöchentlich), Fortschritt wird live aus bestehenden Berichten berechnet, Belohnung wird einmal pro Zeitraum abgeholt; Namen sind fraktionsspezifisch eingefärbt (z. B. „Jaffa-Ehrenauftrag").
- Kodex/Achievements: neue Seite „Kodex" mit 12 dauerhaften Sammel-Errungenschaften über sechs Kategorien (entdeckte Adressen, Raumschlachtsiege, alle Gate-Missionstypen, Allianzbeitritt, gegründete Kolonien, Markttransaktionen); schaltet sich automatisch frei und meldet sich als Bericht, ohne Ressourcenbelohnung.
- Einfluss-Zerfall für Sektorkontrolle: kontrollierte Sektoren verlieren ohne bestätigte Präsenz (`ReinforceSector`) nach 24 h graduell an Einfluss und werden nach 96 h automatisch wieder neutral und frei beanspruchbar; Planetenseite zeigt Countdown und „Präsenz bestätigen"-Aktion pro Sektor.
- Allianz-Kriegsziele: Gründer/Offiziere können auf der Allianzen-Seite einen Zielplaneten erklären; hält die Allianz gemeinsam genug Sektoren (nach Allianzgröße gestaffelt) 24 Stunden durchgehend, erhält jedes Mitglied eine einmalige Ressourcenbelohnung. Fortschritt wird live berechnet, kein Cronjob nötig.
- Erleuchtung (Prestige/Ascension): ab Basis-Score 15.000 (mit 24 h Cooldown zwischen Erleuchtungen) kann die Basis freiwillig auf den Ausgangsstand zurückgesetzt werden (Gebäude, Forschung, Schiffe, Ressourcen) gegen einen permanenten Produktionsbonus (+3 % pro Erleuchtung, gedeckelt bei +30 %); Berichte, Kodex-Fortschritt, Kontrakte und Allianzmitgliedschaft bleiben erhalten. Sichtbares ✦-Abzeichen in der Rangliste.
- Weltevents: serverweite Bedrohungen (Replikatoren-Invasion / Ori-Einfall) starten automatisch nach 24 h Cooldown und laufen 48 h; jeder Spieler kann alle 4 h einen Verteidigungsbeitrag leisten (kostet Versorgungsgüter/Personal), gemeinsamer Fortschrittsbalken auf der Übersichtsseite; bei Erfolg erhält jeder Teilnehmer (nicht nur die stärksten Beitragenden) eine Belohnung.
- Ancient/Asgard-Anomalien: `Explore`- und `AnalyzeAddress`-Gate-Missionen haben bei Erfolg eine kleine Zufallschance (2 %) auf einen einmaligen Fund (antike Ruine oder Asgard-Wrack) mit Ressourcenbonus; jede Adresse liefert das höchstens einmal, danach gilt sie als erschöpft. Im Gate-Raum mit ✧ markiert.
- Espionage-Köder: Ab Gegenspionagestufe „Hardened" kann auf der Geheimdienst-Seite ein Ködervorrat mit erfundenen Ressourcen-/Flottenwerten aufgeladen werden (kostet Intel, begrenzte Einsätze). Erfolgreiche Spionageangriffe können dadurch mit Falschwerten getäuscht werden; die Erkennungschance des Angreifers sinkt mit dessen Sensorik-/Tarntechnologie-Stufe.
- Handelsrouten: Auf „Flotte senden" lassen sich wiederkehrende automatische Transporte zwischen zwei Basen einrichten (Intervall 2–168 h, max. 5 aktive Routen), die ohne manuelle Bestätigung laufen; fehlende Schiffe/Ressourcen lassen einen Zyklus ausfallen und automatisch beim nächsten Versuch nachholen. Kein Abfangrisiko in dieser Version.
- Season-Pässe: Eine deterministisch aus dem UTC-Datum berechnete „Fokuswoche" (0–59, rotiert alle 60 Wochen) markiert im Gate-Raum eine Teilmenge der bekannten Gate-Adressen mit ☀; erfolgreiche Gate-Missionen zu diesen Adressen bringen 50 % mehr Ressourcen-/Intel-Belohnung. Kein neues Datenmodell, keine sperrbaren Adressen – reine Bonusrotation ohne FOMO.
- Charakter-Skilltrees: Auf der Forschungsseite lassen sich Skillpunkte (einer pro abgeschlossener Gate-Mission) auf drei Rollen verteilen (Militär, Wissenschaft, Diplomatie, je Stufe 0–10). Militär erhöht den Erfolg bei Risikoanalyse-, Wissenschaft bei Artefaktsuche- und Diplomatie bei diplomatischen Kontakt-Missionen; Wissenschaft beschleunigt zusätzlich die Forschung um bis zu 30 %.
- Mentoren-System: Neue Allianzmitglieder können sich innerhalb von 14 Tagen nach Beitritt einen Mentor aus der eigenen Allianz wählen. Erreicht der Schützling seine erste Gate-Mission bzw. seinen ersten kontrollierten Sektor, erhält der Mentor automatisch eine Ressourcenbelohnung (ohne Cronjob, live bei Seitenaufruf berechnet).
- Diplomatie-Layer: Allianzen können sich gegenseitig Nichtangriffspakte vorschlagen und annehmen; ein aktiver Pakt blockiert Weltraumangriffe zwischen den Allianzen und reduziert die Markt-Gebühr beim Handel zwischen ihnen um 10 %. Krieg lässt sich jederzeit erklären; ein während eines Pakts gebrochener Frieden sperrt die brechende Allianz für 7 Tage von einem neuen Paktvorschlag an dieselbe Allianz.
- Fraktionsspezifische Questlines: Neue Seite „Questline" mit vier inhaltlich eigenständigen, aufeinander aufbauenden Missionsreihen pro Startfraktion (antike Ausgrabung, Jaffa-Aufstand, Tok'ra-Infiltration, Lucian-Schmuggelroute). Schritte schalten sich der Reihe nach frei, sobald genug passende Gate-Missionen erfolgreich abgeschlossen wurden, und bringen eine kleine Intel-Belohnung sowie einen neuen Handlungsabschnitt als Bericht.
- Forschungsbaum ausgebaut auf 50 Technologien (14 allgemeine + 9 pro Startfraktion, teils mehrstufig aufeinander aufbauend) für mehr Langzeitprogression. Jede Forschung hat eine echte, im Code verdrahtete Wirkung: Ressourcenproduktion (alle sechs Ressourcentypen), Bau-/Werfttempo, Schiffsbaukosten, Gate-Missions-Erfolg je Missionsart, Anomalie-Fundchance/-ausbeute, Verluste bei fehlgeschlagenen Missionen, Weltraum-/Bodenkampfstärke, Spionageerfolg/-abwehr sowie Markt-Handelsgebühren.
