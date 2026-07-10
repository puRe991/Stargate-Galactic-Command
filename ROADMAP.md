# Roadmap: Richtung "Stargate Worlds"

Dieses Dokument beschreibt den geplanten Ausbau von Stargate Galactic Command
von einem browserbasierten Text-/Strategie-MMO im OGame-Stil hin zu einem
Spielgefühl, das sich an "Stargate Worlds" (das 2008 eingestellte 3D-MMORPG
von Cheyenne Mountain Entertainment) orientiert: Away-Teams, die durchs
Gate auf fremde Welten reisen, dort in Echtzeit erkunden und kämpfen,
eingebettet in Fraktionspolitik und Basisausbau.

## Architekturentscheidung

**Hybrid-Ansatz:** Das bestehende ASP.NET Core MVC/Razor-Backend bleibt der
Kern für die "Meta-Ebene" (Basisverwaltung, Wirtschaft, Forschung,
Flottenbau, Diplomatie, Marktplatz, Ranglisten). Zusätzlich entsteht ein
leichter 2D-Top-Down-Client (Phaser/PixiJS), der über SignalR in Echtzeit
mit dem Server spricht und die Away-Team-Erkundung sowie Live-Encounter
abbildet. Kein Wechsel auf einen 3D-Engine-Client (Unity/Godot) in
absehbarer Zeit.

## Phase 1 — Fundament & Vertical Slice

- SignalR-Hub für Echtzeitkommunikation Server ↔ Client
- Neues `Character`/Avatar-Modell pro User (Aussehen, Rolle: Militär /
  Wissenschaft / Diplomatie), getrennt von `BaseSector`
- Minimaler 2D-Client (Phaser oder PixiJS), eingebettet als neue
  Razor-View/Route (z. B. `/Game/Mission/{id}/Explore`)
- Eine bestehende Gate-Mission als Vertikal-Slice umbauen: Team spawnt am
  Gate-Icon, bewegt sich frei/tile-basiert zum Missionsziel, einfacher
  Encounter, Ergebnis fließt in die bestehende `GateMissionReport`-Pipeline
  zurück
- Alte Text-Auflösung bleibt als Fallback bestehen

## Phase 2 — Kern-Erkundungsloop

- Mehrere Planeten mit begehbaren Top-Down-Karten (handgebaut oder leicht
  prozedural)
- Einfache Echtzeit-/Semi-Echtzeit-Encounter (Click-to-move, Auto-Attack,
  Cooldowns — bewusst simpel, kein Action-Combat)
- Gegnerfraktionen aus der Lore (Jaffa-Patrouillen, Goa'uld-Wachen,
  Replikatoren, Ori-Kult, Unas, …)
- Loot-/Inventarsystem, das in Ausrüstung/Basiswerte zurückfließt
- Koop: 2–4 Spieler gemeinsam auf derselben Missionskarte via
  SignalR-Gruppen

## Phase 3 — Fraktionskrieg & Präsenz

- Contested Sectors bekommen eine echte Live-Skirmish-Karte statt nur
  abstrakter `SectorControl`-Berechnung — Angreifer/Verteidiger können live
  mitspielen
- Allianz-Stützpunkte als visualisierter Hub
- PvP-Arenen im 2D-Client

## Phase 4 — Welt-Tiefe

- Storymissionen/Questreihen über Gate-Adressen, NPC-Dialoge
- Charakterprogression (Skilltrees je Rolle), wirkt sich auf Missionsboni
  aus
- Eigener Basissektor als begehbare/anpassbare 2D-Szene statt reiner
  Statistik-Ansicht
- Live-Ressourcensammlung während der Erkundung

## Phase 5 — Technik & Skalierung

- SQLite → PostgreSQL für Mehrspieler-Echtzeitlast
- Lasttests für SignalR-Hubs, Instanzierung pro Mission
- Performance-Tuning des 2D-Clients, Responsive/Mobile

## Phase 6 — Steam-Release-Fähigkeit

Voraussetzung für einen Release auf Steam als MMO. Baut auf einer
abgeschlossenen Phase 5 auf (Echtzeit-Grundlage + PostgreSQL müssen stehen,
bevor produktiv gehostet wird).

- **Hosting & Betrieb:** Containerisierung (Dockerfile), CI/CD-Pipeline,
  produktives Hosting mit PostgreSQL statt SQLite, Monitoring/Logging,
  Backup-Strategie
- **Steamworks-Integration:** Steam-App-ID, Steamworks SDK-Anbindung,
  Steam-Login/Auth statt (oder zusätzlich zu) eigenem Account-System,
  Steam-Achievements & -Leaderboards, SteamPipe-Build-Pipeline für Uploads
- **Eigenständiger Client:** Der 2D-Client aus Phase 1–4 muss als
  installierbare Anwendung (z. B. Electron-Wrapper oder natives Build)
  lauffähig sein, nicht nur als Browser-Route
- **Sicherheit & Anti-Abuse:** Rate-Limiting und Lockout auf Login/Auth,
  Schutz gegen Ressourcen-/Kampf-Exploits, Moderationswerkzeuge für Chat
  und Spielerverhalten, Reporting-System
- **Fehlende Grundsysteme für MMO-Anspruch:** Live-Chat (aktuell nur
  asynchrone Mailbox über `MessageService`), Diplomatie-Layer zwischen
  Allianzen, Mentoring/Onboarding für neue Spieler
- **Store-Vorbereitung:** Store-Page-Assets, Alterskennzeichnung,
  Lokalisierung (mind. Englisch, aktuell nur Deutsch), Preismodell
  (F2P/Buy-to-play/Abo) und ggf. Anti-Cheat-Lösung für PvP-Bereiche

## Lore-Leitplanken (unverändert)

- Das Stargate-Programm bleibt geheim.
- Erde, Atlantis und Dakara sind als normale Spielerplaneten gesperrt.
- Stargates transportieren Teams und kleine Ausrüstung, keine Großflotten.
- Große Schiffe reisen per Hyperraum.
- Ancient-, Asgard- und ZPM-Technologie bleibt selten.
- Startfraktionen: Tau'ri/SGC, Freie Jaffa, Tok'ra und Lucian Alliance.
