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
| Meta-Progression | Kodex/Achievements, tägliche/wöchentliche Kontrakte, Erleuchtung (Prestige/Ascension), Weltevents, Charakter-Skilltrees |
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

## Offene TODOs

- PvP-Regeln für umkämpfte Sektoren erst nach dem Startplanet-Schutz sauber modellieren.
- Erste Migrationen ergänzen, sobald das Datenmodell stabiler ist.
- Balancing-Werte für Kosten und Produktion mit Spieldesign-Zielen abgleichen.

## Erledigt

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
