# Stargate Worlds 3D — Vertical Slice

Separater Java/jMonkeyEngine-Prototyp, der prüft, ob ein 3D-Client im Stil
des nie veröffentlichten "Stargate Worlds" (Cheyenne Mountain
Entertainment, 2008 eingestellt) technisch machbar ist. Läuft komplett
unabhängig vom bestehenden ASP.NET-Core-Spiel in
`StargateGalacticCommand.*` — dort wurde nichts verändert oder entfernt.

## Status

Rendering- und Flow-Machbarkeitsstudie, kein Spiel:

- Bildschirm-Flow als jME-AppStates: Hauptmenü → Login → Server-Auswahl →
  Charakter-Auswahl/-Erstellung → Mission, jeweils mit Tastatur- (Pfeile/WASD,
  Enter, Esc, Tab) und Mausbedienung (Hover-Highlight, Klick)
- Login/Charaktername sind echte Texteingabefelder (`TextField.java`, per
  `RawInputListener` blinkender Cursor) — Login akzeptiert aktuell jeden
  nicht-leeren Namen, keine echte Authentifizierung
- Server-Auswahl zeigt drei feste Test-Welten (eine davon "pausiert", analog
  zum Admin-Pause-Feature im Hauptprojekt)
- Charaktererstellung: Name, Fraktion (unsere vier Startfraktionen, nicht
  SGWs Stargate-Union/Praxis), Rolle (Militär/Wissenschaft/Diplomatie, wie
  im bestehenden Skilltree) — kein visueller Charakter-Editor wie im
  SGW-Original, dafür fehlen 3D-Charakter-Assets
- Mission: eine Planetenoberfläche (Platzhalter für "Beta Site" aus der
  SGW-Planetenliste), ein Stargate mit 9 Chevrons und schimmerndem Event
  Horizon (Kreiszahl an den bestehenden Web-Client angelehnt, siehe
  `stargate-dial.js`/`site.css` im Hauptprojekt), ein Away-Team-Avatar
  (WASD-Bewegung, Third-Person-Kamera), ein paar Deckungsobjekte als
  Platzhalter für SGWs Deckungs-Shooter-Kampfkonzept

Kein Netzwerk-Code, keine Anbindung ans bestehende Backend, keine echten
3D-Assets (nur Boxen/Torus/Zylinder-Primitive), keine Persistenz (Charaktere
existieren nur für die laufende Sitzung) — das kommt erst, wenn die Richtung
bestätigt ist.

## Bekannte SGW-Eckdaten, an denen sich das orientiert

(Quelle: erhaltene Previews/Wiki-Einträge, da das Spiel nie erschienen ist)

- Zwei Bündnisse: Stargate Union (Tau'ri, Freie Jaffa, Asgard) vs. Praxis
  (OP-Core, Jaffa, New Mind Goa'uld)
- Klassen/Skill-Rollen: Archaeologist, Scientist, Soldier, Commando +
  Research/Combat Marine/Medical/Scientific/Diplomatic/Engineering/Exploration
- 10 spielbare Planeten geplant (u. a. Beta Site, Dakara, Tollana, SGC als Hub)
- Echtzeit-Deckungs-Shooter-Kampf, Squad-Leader steuert Trupp-Ziele

Zu Wirtschaft, Basisbau, Forschung, Markt, Diplomatie etc. gibt es aus SGW
keine dokumentierten Vorgaben — diese Systeme existieren im Hauptprojekt
weiter und werden hier bewusst nicht angefasst, bis entschieden ist, ob sie
übernommen, ersetzt oder verworfen werden.

## Bauen & starten

Voraussetzung: JDK 17+, Maven.

```bash
cd stargate-worlds-3d
mvn -q compile
mvn -q exec:java
```

Auf einem Rechner ohne Fenstersystem (reines Server-/CI-Setup) lässt sich
mit `--screenshot-and-exit` ein einzelner Screenshot des Hauptmenüs rendern
und die Anwendung beendet sich danach automatisch, z. B. unter Xvfb:

```bash
xvfb-run -a mvn -q exec:java -Dexec.args="--screenshot-and-exit"
```

`--screenshot-mission-and-exit` überspringt Menü/Login/Server/Charakter und
screenshottet direkt die Missionsszene mit einem Test-Charakter (für
Regressionschecks am Rendering selbst). Während der Laufzeit sichert F12
jederzeit einen nummerierten Screenshot (`sgw3d-liveNNNN.png`), unabhängig
vom aktiven Bildschirm.

## Offene nächste Schritte

1. Entscheidung: Fraktionen/Klassen/Planeten 1:1 an SGW anlehnen oder an
   die bestehende Spiellore (Tau'ri/SGC, Freie Jaffa, Tok'ra, Lucian
   Alliance) anpassen — inkl. Dakara-Sperre aus `ROADMAP.md`. Aktuell nutzt
   die Charaktererstellung unsere vier Startfraktionen.
2. Anbindung ans bestehende Backend: echte Anmeldung (Login-Screen ist
   aktuell reine Attrappe), echte Serverliste statt der drei Test-Welten,
   Charakterpersistenz statt In-Memory-`GameSession`, REST für Meta-Daten,
   SignalR-Java-Client für Echtzeit-Encounter.
3. Echtes Deckungs-/Kampfsystem statt reiner Optik.
4. Reale 3D-Assets statt Primitiven, inkl. eines visuellen Charakter-Editors
   näher am SGW-Original (Aussehen-Slider, rotierbare 3D-Vorschau vor dem Gate).

## Architekturnotiz: Bildschirm-Flow

Jeder Bildschirm ist ein eigener `com.jme3.app.state.BaseAppState`
(`MainMenuState`, `LoginState`, `ServerSelectState`, `CharacterSelectState`,
`CharacterCreateState`, `MissionState`). Vorwärtsnavigation hängt i. d. R.
eine neue Instanz des nächsten Screens an und deaktiviert (nicht: entfernt)
den aktuellen; Esc/Zurück reaktiviert die noch angehängte vorherige Instanz
und entfernt die aktuelle. `ServerSelectState`/`CharacterSelectState` teilen
sich die Basisklasse `ListMenuState`; da mehrere Instanzen unterschiedlicher
Unterklassen gleichzeitig angehängt sein können, verwendet jede Instanz
eindeutige (klassen- und objektbezogene) Input-Mapping-Namen, statt sich
literale Namen zu teilen — sonst überschreiben sich `addMapping`/
`deleteMapping`-Aufrufe verschiedener Instanzen gegenseitig. Eine
`GameSession`-Instanz wird von `Sgw3dPrototype` erzeugt und durch alle
Screens weitergereicht (Login-Name, gewählter Server, erstellte Charaktere).
