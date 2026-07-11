# Konzept: Ingame-Shop

Dieses Dokument skizziert, wie ein Ingame-Shop in Stargate Galactic Command
integriert werden könnte und was dort sinnvoll verkauft werden kann. Es
ergänzt `ROADMAP.md` (Phase 6 nennt "Preismodell (F2P/Buy-to-play/Abo)" bereits
als Voraussetzung für einen Steam-Release) um ein konkretes Konzept, und hält
sich an dieselben Lore-Leitplanken sowie an das Balancing-Prinzip aus
`GAMEPLAY_IDEAS.md` ("Boni als Bonus obendrauf, nicht als Ersatz für
Kernwirtschaft").

## 1. Ist-Zustand

- Keine Premium-Währung, kein Shop, keine Zahlungsanbindung (kein
  Stripe/PayPal/Play Billing/Apple IAP) im Code vorhanden.
- Ressourcen sind ausschließlich Soft-Currency: Naquadah, Trinium, Supplies,
  Energy, Personnel, Intel (`ResourceStock.cs`), erspielbar über Wirtschaft,
  Gate-Missionen, Handel.
- Einziger bestehender "Markt" ist der Spieler-zu-Spieler-Tauschmarkt
  (`PlanetMarketService`/`Market.cshtml`) — kein Cash-Shop.
- UI-Muster für listenartige Kauf-/Aktionsseiten existiert bereits
  (`Shipyard.cshtml`, `Market.cshtml`: Tabelle mit Kosten + POST-Formular +
  Antiforgery-Token), darauf lässt sich eine Shop-Seite direkt aufbauen.

## 2. Designprinzipien

1. **Kein Pay-to-Win.** In einem PvP-lastigen Wirtschafts-/Kampfspiel darf der
   Shop keine direkte Kampf- oder Wirtschaftsüberlegenheit verkaufen, die
   zahlende Spieler dauerhaft über nicht-zahlende stellt. Das deckt sich mit
   dem bestehenden Balancing-Grundsatz aus `GAMEPLAY_IDEAS.md`.
2. **Lore-konform.** Ancient-, Asgard- und ZPM-Technologie bleibt laut
   `ROADMAP.md` bewusst selten — sie darf nicht käuflich sein. Auch keine
   Items, die die gesperrten Planeten (Erde, Atlantis, Dakara) umgehen.
3. **Zeit statt Macht verkaufen.** Komfort- und Zeitersparnis-Angebote sind
   vertretbar (jeder Spieler hätte das Ergebnis auch durch Spielzeit
   erreicht), reine Stärke-Multiplikatoren nicht.
4. **Kosmetik ist der sichere Kern.** Skins, Embleme, Titel — kein
   Balancing-Risiko, passt zum bereits etablierten Kodex-/Prestige-Ansatz
   (Ascension-Abzeichen, Kodex-Einträge).
5. **Erspielbar bleibt Standard.** Der Shop ergänzt die Kernwirtschaft, ersetzt
   sie nicht — alles, was aktuell nur durch Spielzeit erreichbar ist, bleibt
   das auch weiterhin für Nicht-Zahler.

## 3. Neue Premium-Währung: "Chevron-Kristalle"

- Käufliche, nicht erspielbare(!) Zusatzwährung, thematisch an die
  7-Chevron-Gate-Adressierung angelehnt statt an geschützte Lore-Objekte
  (ZPM etc.).
- Kleine Mengen optional auch als seltene Belohnung für besondere
  Meilensteine (z. B. Weltevent-Sieg, Achievement-Kodex-Abschluss)
  ausschüttbar, um Nicht-Zahlern Zugang zu rein kosmetischen Items zu geben
  — aber deutlich langsamer als Kauf, damit der Shop wirtschaftlich sinnvoll
  bleibt.
- Datenmodell: neues Feld `ChevronCrystals` auf `User` (analog zu
  `AscensionCount`), kein separates Wallet-Modell nötig für den Start.

## 4. Was verkaufen

### 4.1 Zeit & Komfort (Kernangebot, geringes P2W-Risiko)
- Bauauftrag/Forschung/Werft sofort fertigstellen (Preis skaliert mit
  Restzeit) — analog zu Zeitbeschleunigern in vergleichbaren Browser-MMOs.
- Zusätzliche Bau-/Werft-/Forschungs-Warteschlangenplätze (dauerhaft
  freischaltbar, einmalig).
- Zusätzlicher Basen-Slot / früherer Zugriff auf Kolonie-Gründung.
- Zusätzliche Handelsrouten über `MaxActiveRoutesPerUser` (5) hinaus.
- Sofortiger Abschluss des Präsenz-Cooldowns bei Sektorkontrolle.

### 4.2 Kosmetik (kein Balancing-Risiko)
- Schiffs-Skins (visuelle Varianten der bestehenden `ShipType`s, keine
  Statsänderung).
- Fraktions-Embleme/Flaggen für Basen und Flottenberichte.
- Exklusive Profiltitel, Rangliste-Badges (zusätzlich zu erspielten
  Ascension-/Kodex-Abzeichen, nicht als Ersatz).
- Namensänderung der Basis/des Accounts.

### 4.3 Bündel / Starterpakete
- "Neuling-Paket": kleine Menge Ressourcen + 1–2 Zeitkarten + kosmetisches
  Item, einmalig kaufbar — senkt Einstiegshürde ohne Dauervorteil.
- Saisonale Bündel passend zu Season-Events (2.2 aus `GAMEPLAY_IDEAS.md`),
  ausschließlich kosmetisch oder Zeitersparnis, keine exklusiven
  Missions-Boni.

### 4.4 Abo-Modell ("Kommando-Zugang", optional, Phase 2)
- Monatliches Abo mit moderaten Komfortvorteilen: z. B. +1 Warteschlangenplatz
  auf Zeit, schnellere Offline-Produktionsberechnung, exklusive kosmetische
  Abo-Icons. Bewusst **kein** Ressourcenproduktions-Multiplikator, um die
  P2W-Grenze nicht zu verwischen.

### Explizit nicht verkaufen
- Keine direkten Kampfschiffe/-flotten oder Truppen.
- Keine Ressourcenpakete in wettbewerbsrelevanter Menge (allenfalls sehr
  kleine "Anschub"-Mengen im Neuling-Paket).
- Keine Ancient-/Asgard-/ZPM-Technologie oder Zugriff auf gesperrte Planeten.
- Keine Erfolgschance-Boosts für Spionage/Kampf/Gate-Missionen (würde
  Skilltree- und Fraktionsboni-System entwerten).

## 5. Technische Umsetzung (Skizze)

- **Datenmodell:** `User.ChevronCrystals` (int) für den Kristallbestand;
  neues `ShopItemDefinition` als statischer Katalog (Muster wie
  `ContractDefinition`/`AchievementService` — keine DB-Tabelle nötig für den
  Katalog selbst); `ShopPurchase`-Tabelle nur für den Kaufverlauf/Cosmetics,
  die dauerhaft freigeschaltet bleiben müssen (z. B. Skins), analog zu
  `AchievementProgress`.
- **Controller/View:** neue `GameController.Shop()`-Aktion +
  `Views/Game/Shop.cshtml` nach dem `Shipyard.cshtml`-Muster (Tabelle:
  Item, Preis in Kristallen, Kauf-Button mit `[ValidateAntiForgeryToken]`).
- **Kristall-Aufladung (echtes Geld):** separate Seite "Kristalle kaufen"
  mit festen Paketgrößen; Zahlungsanbieter zunächst nicht anbinden, sondern
  Interface/Service (`IPaymentProvider`) vorsehen, damit später Stripe o. Ä.
  eingehängt werden kann, ohne den Shop-Kern anzufassen.
- **Zeitkarten-Logik:** bestehende Restzeit-Berechnung in
  `BuildQueueService`/`ResearchQueueService`/`ShipyardService` wiederverwenden
  — "sofort fertigstellen" setzt nur den bestehenden Fälligkeitszeitpunkt auf
  `now`, keine neue Zeitlogik nötig.

## 6. Offene Punkte / Voraussetzungen

- **Zahlungsanbieter:** Auswahl (Stripe, Paddle, Steam-Wallet ab Phase 6)
  hängt von Rechtsform/Geschäftskonto ab — nicht Teil dieses Konzepts,
  gehört in ein separates Umsetzungsticket.
- **Rechtliches:** AGB/Widerrufsrecht für digitale Güter, Jugendschutz
  (Lootbox-artige Mechaniken bewusst vermeiden — alle Shop-Items sind direkt
  sichtbar kaufbar, keine Zufallsboxen), Preisauszeichnung.
- **Balancing-Review:** Zeitkarten-Preise sollten so kalibriert werden, dass
  sie grob dem "Wert" der eingesparten Wartezeit entsprechen, nicht einer
  Abkürzung zu spürbarem Wirtschaftsvorteil gegenüber aktiven Spielern.

## 7. Priorisierungsvorschlag

Kein Umbau des Datenmodells nötig, sofort startbar:
1. Kristall-Feld auf `User`, Shop-Seite mit rein kosmetischen Items (Skins,
   Titel, Embleme) — kleinster Scope, kein P2W-Risiko, testet die
   UI/Kauf-Mechanik.

Mittlerer Aufwand:
2. Zeitkarten (Bau/Forschung/Werft sofort fertigstellen).
3. Zusätzliche Warteschlangenplätze/Basen-Slots (dauerhafte Freischaltung).

Größerer Aufwand, braucht externe Anbindung:
4. Echte Kristall-Aufladung via Zahlungsanbieter (Stripe o. Ä.).
5. Abo-Modell "Kommando-Zugang".
