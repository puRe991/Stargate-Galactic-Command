# Stargate Galactic Command

Browserbasierter Strategie-MMO-Prototyp im OGame-Stil mit Stargate-inspirierter Lore. Spieler verwalten keine ganzen Planeten, sondern geheime Basissektoren auf gemeinsam genutzten Welten.

## Status dieser Version

Version 0.0.2 erweitert die technische Grundstruktur um Gebäudeausbau:

- ASP.NET Core MVC/Razor Webanwendung
- getrennte Projekte für Web, Core, Data und Tests
- SQLite-Anbindung über Entity Framework Core
- Basismodelle für Planeten, Basissektoren, Ressourcen, Gebäude und Fraktionen
- Economy-Service für einfache Produktionsformeln
- Gebäudekatalog mit Kosten- und Bauzeitformeln
- serverseitige Bauwarteschlange mit Ressourcenabzug und automatischem Abschluss
- Gebäudeübersicht mit Ausbauaktionen, Baukosten, Bauzeit und laufendem Timer
- dunkle tabellarische Startseite mit freigegebenen Welten und einer Startbasis

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
StargateGalacticCommand.Tests  Unit-Tests für Economy-Formeln
```

## Voraussetzungen

- Visual Studio 2019
- .NET Core SDK 3.1
- SQLite wird lokal über `Microsoft.EntityFrameworkCore.Sqlite` verwendet; kein separater Server ist nötig.

> Hinweis: .NET Core 3.1 ist end-of-life. Es wurde hier gewählt, um Visual-Studio-2019-Kompatibilität einfach zu halten. Für produktive Weiterentwicklung sollte ein Upgrade auf eine unterstützte .NET-LTS-Version geplant werden.

## Lokal starten

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

- Spieler- und Authentifizierungsmodell ergänzen.
- Parallelbau und erweiterte Warteschlangenlogik modellieren.
- Gate-Missionen strikt von Hyperraum-Flotten trennen.
- Erste Migrationen ergänzen, sobald das Datenmodell stabiler ist.
- Balancing-Werte für Kosten und Produktion mit Spieldesign-Zielen abgleichen.
