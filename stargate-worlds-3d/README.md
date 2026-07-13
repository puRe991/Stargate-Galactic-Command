# Stargate Worlds 3D — Vertical Slice

Separater Java/jMonkeyEngine-Prototyp, der prüft, ob ein 3D-Client im Stil
des nie veröffentlichten "Stargate Worlds" (Cheyenne Mountain
Entertainment, 2008 eingestellt) technisch machbar ist. Läuft komplett
unabhängig vom bestehenden ASP.NET-Core-Spiel in
`StargateGalacticCommand.*` — dort wurde nichts verändert oder entfernt.

## Status

Reine Rendering-Machbarkeitsstudie, kein Spiel:

- Eine Planetenoberfläche (Platzhalter für "Beta Site" aus der SGW-Planetenliste)
- Ein Stargate mit 9 Chevrons und schimmerndem Event Horizon (Kreiszahl an
  den bestehenden Web-Client angelehnt, siehe `stargate-dial.js`/`site.css`
  im Hauptprojekt)
- Ein Away-Team-Avatar (Platzhalter, WASD-Bewegung, Third-Person-Kamera)
- Ein paar Deckungsobjekte als Platzhalter für SGWs Deckungs-Shooter-Kampfkonzept

Kein Netzwerk-Code, keine Anbindung ans bestehende Backend, keine echten
3D-Assets (nur Boxen/Torus/Zylinder-Primitive) — das kommt erst, wenn die
Richtung bestätigt ist.

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
mit `--screenshot-and-exit` ein einzelner Screenshot rendern und die
Anwendung beendet sich danach automatisch, z. B. unter Xvfb:

```bash
xvfb-run -a mvn -q exec:java -Dexec.args="--screenshot-and-exit"
```

## Offene nächste Schritte

1. Entscheidung: Fraktionen/Klassen/Planeten 1:1 an SGW anlehnen oder an
   die bestehende Spiellore (Tau'ri/SGC, Freie Jaffa, Tok'ra, Lucian
   Alliance) anpassen — inkl. Dakara-Sperre aus `ROADMAP.md`.
2. Anbindung ans bestehende Backend (REST für Meta-Daten, SignalR-Java-Client
   für Echtzeit-Encounter) statt lokaler Platzhalterdaten.
3. Echtes Deckungs-/Kampfsystem statt reiner Optik.
4. Reale 3D-Assets statt Primitiven.
