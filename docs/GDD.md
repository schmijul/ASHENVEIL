# ASHENVEIL — Demo GDD (v1.0, clean rebuild 2026-07-05)

## Vision
Gothic RPG demo in the spirit of **Gothic / The Witcher 3 / Skyrim**. Third-person, dense
Mischwald (kein Nebel!), nordisches Dorf, düstere Low-Fantasy-Welt. Grafik-Ziel: Gothic 4 /
Witcher 3-Level (Referenzen: The Dark Mod, OpenMW, Veloren). Realistische Tierproportionen,
Äther-Partikeleffekte.

## Lore — Der Ätherfluss
Der Ätherfluss war ein Netzwerk sichtbarer Energie, das die drei Reiche verband und Magie,
Kommunikation und sogar Jahreszeiten steuerte. Vor 200 Jahren durch die **Auslöschung**
zerschlagen; seitdem giftig: kristallisiert in der Landschaft, mutiert Tiere, korrumpiert
Menschen. Wer Äther berührt, wird normalerweise wahnsinnig oder mutiert — der Spielercharakter
kann das ohne Schaden, was ihn/sie besonders macht.

Mechanisch: Äther ist Hauptressource für Crafting/Magie, mit eingebautem Risiko —
zu viel Nutzung ⇒ **Korruptions-Mechanik**, Charakter mutiert langsam.

## Demo-Ablauf (8 Phasen — der gesamte Scope)
1. **Aufwachen im Wald** — dichter Mischwald, kein Nebel; Spieler läuft Richtung Dorf.
2. **Jagd** — Tiere jagen (Reh/Hase/Wildschwein), Fleisch/Felle looten. Tutorial für Movement + Combat.
3. **Dorf betreten** — nordisch, Strohdächer, Holzwände. Verkaufen, einkaufen. Tutorial Handel/Inventar.
4. **NPC-Dialoge** — 3–4 NPCs, kleine Nebenquests (Kräuter sammeln, verlorenes Werkzeug).
5. **Tiefer Wald** — Ätherkristall finden; Hände leuchten beim Berühren (erster Hinweis auf Besonderheit).
6. **Boss-Fight** — mutierter Wolf, **nur mit Äther besiegbar** (normale Angriffe fast wirkungslos).
7. **Rückkehr ins Dorf → Zerstörung** — Dorf brennt (Äthersturm / Kernwall-Angriff).
8. **Flucht + Richtungswahl** — Kernwall / Flimmermoor / Hohensang → Fade-to-Black, „Demo Ende".

## Kernsysteme
- **Movement/Camera**: Third-person (walk/run/sprint/jump/dodge), freie Kamera, Stamina.
- **Combat**: Melee (leicht/schwer/block/dodge), Waffen-Hitboxen, IDamageable überall.
- **Wildlife**: Wander/Flee/Aggro-Verhalten je Spezies; Loot (Fleisch, Felle) beim Erlegen.
- **Inventar & Handel**: Items mit Gewicht/Wert, Gold, Händler-UI (kaufen/verkaufen).
- **Dialog & Quests**: Verzweigte Dialoge, Questlog/Journal, 2 Nebenquests + Haupt-Questkette (Demo-Phasen).
- **Äther**: AetherCharge-Ressource durch Kristall; Äther-verstärkte Angriffe (Partikel an Händen);
  **Korruption** steigt bei Nutzung (visuelles Feedback, Debuff-Schwellen; in der Demo: Anzeige + 1 Schwelle).
- **DemoFlow**: Phasen-Statemachine, steuert Tutorials, Trigger, Boss-Gate, Dorf-Zerstörung, Ende.
- **UI**: HUD (HP/Stamina/Äther/Korruption), Inventar, Händler, Dialog, Journal, Tutorial-Prompts,
  Endscreen mit Richtungswahl. Sprache der UI-Texte: **Deutsch**.

## Grafik-Stack
Unity 6000.3 + URP: TAA, ACES-Tonemapping, Volume-Profile (Wald hell/klar — KEIN Nebel; Dorf warm;
tiefer Wald kühl+Ätherglühen; brennendes Dorf orange/rauchig), realtime Schatten, GPU-Instancing
für Vegetation. Assets aus lokalem Asset-Store-Cache (Viking Village URP fürs Dorf, Forest-Packs,
Rocks, Waffen, Mecanim-Animationen, modularer Human-Charakter).

## Nicht im Scope der Demo
Speichersystem, Skilltrees, Crafting-Bench, offene Weltkarte, spielbare Gebiete nach der Richtungswahl.
