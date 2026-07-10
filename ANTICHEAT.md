# Anti-Cheat- & Anti-Abuse-Konzept

Dieses Dokument beschreibt das Bedrohungsmodell und die geplante Lösung für
unfaires Spielverhalten in Stargate Galactic Command. Es ergänzt den Punkt
"Sicherheit & Anti-Abuse" aus [ROADMAP.md](ROADMAP.md) (Phase 6) um ein
konkretes Konzept, das schon vor dem Echtzeit-Client (Phase 1–4) umsetzbar ist.

## Einordnung: Was für ein Spiel ist das aus Anti-Cheat-Sicht?

Aktuell ist Stargate Galactic Command ein vollständig serverautoritatives
Browserspiel: Der Client hält keinen eigenen Spielzustand, jede Aktion läuft
über einen MVC-POST (`GameController`, `AccountController`, ...) und wird von
Services in `StargateGalacticCommand.Core/Services` serverseitig berechnet.
Zeiten (Bauzeiten, Cooldowns, Flugzeiten) werden konsequent mit
`DateTime.UtcNow` serverseitig bestimmt, nicht vom Client geliefert.

Das bedeutet: klassischer Kernel-Level-Anti-Cheat wie bei Shootern (Aimbot-,
Wallhack-Erkennung, EAC/BattlEye) ist **nicht** das relevante Problem. Der
Bedrohungsraum eines textbasierten/rundenbasierten Strategie-MMOs ist ein
anderer:

1. Mehrfachaccounts (Multi-Accounting)
2. Automatisierung / Bots / Skripte gegen die HTTP-Endpunkte
3. Kontoübernahme (Credential Stuffing, Brute-Force auf den Login)
4. Race Conditions / Double-Spend in der Server-Logik
5. Markt-/Handelsexploits (Wash-Trading zwischen eigenen Accounts)
6. Erst zukünftig (Roadmap Phase 1–4, SignalR-Echtzeitclient): Bewegungs-/
   Input-Exploits, sobald ein Client eigene Zustände (Position, Encounter)
   an den Server meldet

Wichtiger Leitsatz für den künftigen 2D-Client: **server-authoritative
bleibt Pflicht**. Der Client darf immer nur Absichten senden ("bewege dich
nach X", "greife an"), niemals Ergebnisse ("ich bin jetzt bei X", "ich habe
gewonnen"). Damit bleibt das bestehende Modell konsistent und ein Großteil
der hier beschriebenen Maßnahmen (Server-Validierung, Rate-Limits,
Audit-Log) trägt auch die Echtzeit-Phase.

## Bestandsaufnahme: was schon gut ist

- Serverseitige Zeitberechnung überall (`DateTime.UtcNow`), keine
  client-gelieferten Timestamps für Bau-/Forschungs-/Flottenzeiten
- `[ValidateAntiForgeryToken]` konsequent auf allen mutierenden Actions
- Passwort-Hashing mit PBKDF2 (100.000 Iterationen) + Salt +
  `CryptographicOperations.FixedTimeEquals` (`PasswordService`)
- `PlayerProtectionStatus` als Grundgerüst für Anti-Griefing (Schutzzeit
  für neue Accounts)
- `PlanetMarketService` validiert Handelbarkeit, Ablaufzeit, verbietet
  Käufe am eigenen Angebot

## Konkrete Lücken (Stand heute)

| # | Lücke | Fundstelle | Risiko |
|---|---|---|---|
| 1 | Kein Rate-Limiting/Lockout auf `Login` | `AccountController.Login` | Credential-Stuffing, Brute-Force |
| 2 | Kein Throttling auf irgendeiner Spielaktion | `GameController` (~50 `[HttpPost]`-Actions) | Skript-Bots spielen 24/7 ohne Pause, Server-Last |
| 3 | Keine DB-Transaktion/Optimistic Concurrency bei Ressourcenausgabe | z. B. `BuildQueueService.StartBuild`, `ShipyardService`, `PlanetMarketService` + `_db.SaveChanges()` am Ende der Controller-Action | Zwei gleichzeitige Requests (2 Tabs, Skript) können denselben Ressourcenstand doppelt verausgaben (read-modify-write ohne Sperre) |
| 4 | Keine Mehrfachaccount-Erkennung | Registrierung (`RegistrationService`) loggt weder IP noch Gerätemerkmale | Alt-Accounts für Sektor-Claiming, Wash-Trading, Spionage-Ringtausch |
| 5 | Keine Volumen-/Cooldown-Grenzen im Marktplatz über Zeit | `PlanetMarketService` | Wash-Trading zwischen eigenen Accounts zum Ressourcen-Waschen |
| 6 | Kein Audit-Log für sicherheitsrelevante Aktionen | – | Kein Nachweis bei Streitfällen/Bans, kein Anomalie-Monitoring |
| 7 | Kein Report-System für Spieler | – | Community kann Verdachtsfälle nicht melden |
| 8 | Legacy-SHA1-Fallback in `PasswordService.Verify` | `PasswordService.cs:41` | Absichtlich für Altbestände, sollte mit Ablaufdatum/Migration versehen werden |

## Lösungsvorschlag: Phasenmodell

### Phase A — Fundament (jetzt umsetzbar, unabhängig vom Echtzeit-Rollout)

- **Login-Schutz:** Fehlversuchszähler pro Account + IP, exponentielles
  Lockout (z. B. nach 5 Fehlversuchen 1 Minute, danach steigend), generische
  Fehlermeldung (kein "Account existiert nicht" vs. "falsches Passwort").
- **Generisches Action-Throttling:** `IAsyncActionFilter`/Attribut
  (`[Throttle(seconds: n)]`), das pro `UserId` + Aktion ein Mindestintervall
  serverseitig erzwingt (z. B. über eine kleine In-Memory- oder DB-gestützte
  Zähler-Tabelle). Deckt Bot-Skripte ab, ohne normales Spielen zu stören.
- **Transaktionssicherheit:** Kritische Ressourcen-Services (Bau, Schiffbau,
  Forschung, Marktplatz, Flottenversand) in `_db.Database.BeginTransaction()`
  bzw. EF-Core-Concurrency-Token (`[Timestamp]`/`RowVersion` auf
  `PlayerBase`/`Resources`) absichern, damit parallele Requests nicht
  denselben Zustand doppelt verändern können.
- **Multi-Account-Heuristik:** Registrierung und Login loggen IP-Hash (kein
  Klartext, DSGVO-freundlich) und Zeitstempel. Einfache Admin-Ansicht:
  "Accounts mit gleicher IP auf demselben Server" als Signal, kein
  Auto-Ban — Menschen entscheiden.
- **Markt-Cooldowns:** Handelsvolumen pro Spieler/Zeitraum begrenzen bzw.
  Trades zwischen Accounts mit auffälligem IP-/Zeit-Muster markieren.
- **Audit-Log:** Neue Tabelle `AuditLogEntry` (UserId, Aktion, Zeitpunkt,
  Kontext-JSON) für Login, Passwortänderung, Handel, Admin-Aktionen.
  Grundlage für spätere Anomalie-Erkennung und Beweissicherung bei Bans.
- **Report-System:** Einfache "Spieler melden"-Funktion (Zielaccount,
  Freitext, Kontext) mit Admin-Queue analog zum bestehenden
  `AdminController`.

### Phase B — Begleitend zum Echtzeit-Client (Roadmap Phase 1–4)

- SignalR-Hub-Verbindungen an die bestehende Session/Auth binden, keine
  anonymen Hub-Verbindungen.
- Client sendet ausschließlich Intents (Bewegungsrichtung, Aktion), Server
  simuliert Position/Cooldowns/Kollision serverseitig und tickt autoritativ.
- Plausibilitätsprüfung pro Tick (max. Distanz pro Zeiteinheit anhand
  Charakter-/Schiffswerten) als einfache Anti-Speedhack-/Anti-Teleport-Maßnahme.
- Pro-Verbindung Rate-Limits auf Hub-Methoden (analog zum Action-Throttling
  aus Phase A).

### Phase C — Betrieb & Skalierung (Roadmap Phase 6)

- Monitoring/Alerting auf statistische Ausreißer (Ressourcenproduktion pro
  Zeit, Gewinnraten in Kämpfen, Session-Länge ohne Pause) auf Basis des
  Audit-Logs aus Phase A.
- Moderationswerkzeuge: Verwarnen/Muten/Bannen mit Verlaufsprotokoll pro
  Account, sichtbar im Admin-Bereich.
- Migration weg vom SHA1-Legacy-Pfad in `PasswordService` (erzwungenes
  Passwort-Reset für Alt-Accounts nach Ankündigungsfrist).
- Bei echtem PvP mit dediziertem Client (falls es so weit kommt, siehe
  Steam-Release-Überlegungen in der Roadmap): erneute Prüfung, ob ein
  client-seitiges Anti-Cheat-SDK überhaupt nötig ist — bei weiterhin
  serverautoritativer Architektur ist das in der Regel verzichtbar.

## Vorgeschlagene Umsetzungsreihenfolge

1. Login-Rate-Limiting/Lockout (kleiner, isolierter Fix, hoher Sicherheitsgewinn)
2. Generisches Action-Throttling für `GameController`-Endpunkte
3. Transaktions-/Concurrency-Schutz um Ressourcen-kritische Services
4. Audit-Log-Tabelle + Report-Funktion
5. IP-Logging + Multi-Account-Heuristik im Admin-Bereich
6. Markt-Cooldowns/Volumengrenzen

Phase B (Echtzeit-Anti-Cheat) folgt erst mit dem SignalR-Vertical-Slice aus
`ROADMAP.md` Phase 1 und sollte von Anfang an mitgeplant statt nachträglich
angeflanscht werden.
