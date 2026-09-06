# Bilder

Dieser Ordner ist für Grafiken vorgesehen, die der Besitzer selbst ablegt. Ohne eine
einzige echte Datei sieht das Spiel trotzdem vollständig aus - fehlende Bilder fallen
automatisch auf eine nach Fellfarbe eingefärbte Silhouette zurück (siehe
`Web/Bilder/HorseArt.cs`).

## Format

- **PNG mit Transparenz** (Alphakanal), keine feste Hintergrundfarbe.
- Empfohlene Kantenlänge: **512 × 512** Pixel.
- Bildausschnitt: **Porträt, Kopf und Hals**, Blickrichtung nach **links**.

## Ordnerstruktur

```
horses/
  base/          {rassenschlüssel}.png    Grundbild je Rasse
  coats/         {rasse}_{fellfarbe}.png  optional, geht vor base
  uniques/       {schlüssel}.png          einzigartige Pferde
  _placeholder.svg                        mitgeliefert, letzter Rückfall
scene/
  sky.png  hills.png  barn.png  fence.png
  grass_far.png  grass_near.png
ui/
  horse_run.png                           Sprite-Streifen, 8 Bilder nebeneinander
  icons/
```

## Auflösungsreihenfolge

Für jedes Pferd probiert `HorseArt.Kandidaten(...)` der Reihe nach:

1. Einzigartiges Pferd → `horses/uniques/{schlüssel}.png`
2. Fellfarbe → `horses/coats/{rasse}_{fellfarbe}.png`
3. Rassen-Grundbild → `horses/base/{rasse}.png`
4. Eine per Fellfarbe eingefärbte Silhouette (kein Dateizugriff, kann nicht fehlschlagen)

Die Komponente `PferdBild.razor` reicht diese Liste als `<img>` mit einem `onerror`
durch, das bei jedem Fehlschlag zum nächsten Kandidaten wechselt - komplett im Browser,
ohne .NET-Umweg. Der letzte Kandidat ist ein `data:image/svg+xml`-Bild und kann darum
nie fehlschlagen: es bleibt garantiert nie eine Lücke im Layout, egal wie viele echte
Grafiken noch fehlen.

## Erwartete Dateien

Automatisch aus den Rassendaten erzeugt (`Sim/Bilddateien.cs` - bei Änderungen an
`Core/Data/rassen.json` neu ausführen und diese Liste ersetzen):

### Grundbilder (`horses/base/`)

```
araber.png
kaltblut.png
pony.png
vollblut.png
warmblut.png
```

### Fellfarben-Varianten, optional (`horses/coats/`)

```
araber_fuchs.png
araber_schimmel.png
araber_rappe.png
kaltblut_braun.png
kaltblut_rappe.png
kaltblut_fuchs.png
pony_braun.png
pony_falbe.png
pony_schecke.png
pony_rappe.png
vollblut_fuchs.png
vollblut_braun.png
vollblut_rappe.png
vollblut_schimmel.png
warmblut_braun.png
warmblut_fuchs.png
warmblut_rappe.png
warmblut_schimmel.png
warmblut_falbe.png
```

### Einzigartige Pferde (`horses/uniques/`)

Noch keine definiert - kommt mit dem Inhalt für einzigartige Pferde.

### Szene und Oberfläche (`scene/`, `ui/`)

Werden erst mit dem Ambiente aus Block 4 der Phase-4-Vorgabe genutzt
(geschichteter Hintergrund, Trainingsbalken-Sprite). Ordner liegen schon bereit.
