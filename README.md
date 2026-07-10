# Stargate Galactic Command

Browserbasierter Strategie-MMO-Prototyp im OGame-Stil mit Stargate-inspirierter Lore. Spieler verwalten keine ganzen Planeten, sondern geheime Basissektoren auf gemeinsam genutzten Welten.

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

> Hinweis: Die Projekte zielen auf .NET 8 LTS. Wenn Visual Studio die Projekte nicht lädt, installiere das aktuelle .NET SDK und stelle sicher, dass der Workload „ASP.NET und Webentwicklung“ aktiv ist.

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
- Tägliche/wöchentliche Kontrakte: neue Seite „Kontrakte“ mit vier wiederkehrenden Aufträgen (Gate-Missionen, Flottenmissionen, Markthandel täglich; Gate-Missionen wöchentlich), Fortschritt wird live aus bestehenden Berichten berechnet, Belohnung wird einmal pro Zeitraum abgeholt; Namen sind fraktionsspezifisch eingefärbt (z. B. „Jaffa-Ehrenauftrag“).
- Kodex/Achievements: neue Seite „Kodex“ mit 12 dauerhaften Sammel-Errungenschaften über sechs Kategorien (entdeckte Adressen, Raumschlachtsiege, alle Gate-Missionstypen, Allianzbeitritt, gegründete Kolonien, Markttransaktionen); schaltet sich automatisch frei und meldet sich als Bericht, ohne Ressourcenbelohnung.
- Einfluss-Zerfall für Sektorkontrolle: kontrollierte Sektoren verlieren ohne bestätigte Präsenz (`ReinforceSector`) nach 24 h graduell an Einfluss und werden nach 96 h automatisch wieder neutral und frei beanspruchbar; Planetenseite zeigt Countdown und „Präsenz bestätigen“-Aktion pro Sektor.
- Allianz-Kriegsziele: Gründer/Offiziere können auf der Allianzen-Seite einen Zielplaneten erklären; hält die Allianz gemeinsam genug Sektoren (nach Allianzgröße gestaffelt) 24 Stunden durchgehend, erhält jedes Mitglied eine einmalige Ressourcenbelohnung. Fortschritt wird live berechnet, kein Cronjob nötig.
- Erleuchtung (Prestige/Ascension): ab Basis-Score 15.000 (mit 24 h Cooldown zwischen Erleuchtungen) kann die Basis freiwillig auf den Ausgangsstand zurückgesetzt werden (Gebäude, Forschung, Schiffe, Ressourcen) gegen einen permanenten Produktionsbonus (+3 % pro Erleuchtung, gedeckelt bei +30 %); Berichte, Kodex-Fortschritt, Kontrakte und Allianzmitgliedschaft bleiben erhalten. Sichtbares ✦-Abzeichen in der Rangliste.
- Weltevents: serverweite Bedrohungen (Replikatoren-Invasion / Ori-Einfall) starten automatisch nach 24 h Cooldown und laufen 48 h; jeder Spieler kann alle 4 h einen Verteidigungsbeitrag leisten (kostet Versorgungsgüter/Personal), gemeinsamer Fortschrittsbalken auf der Übersichtsseite; bei Erfolg erhält jeder Teilnehmer (nicht nur die stärksten Beitragenden) eine Belohnung.
- Ancient/Asgard-Anomalien: `Explore`- und `AnalyzeAddress`-Gate-Missionen haben bei Erfolg eine kleine Zufallschance (2 %) auf einen einmaligen Fund (antike Ruine oder Asgard-Wrack) mit Ressourcenbonus; jede Adresse liefert das höchstens einmal, danach gilt sie als erschöpft. Im Gate-Raum mit ✧ markiert.
