# Bilder

Dieser Ordner ist für Grafiken vorgesehen, die der Besitzer selbst ablegt. Ohne eine
einzige echte Datei sieht das Spiel trotzdem vollständig aus - fehlende Bilder fallen
automatisch auf eine nach Fellfarbe eingefärbte Silhouette zurück (siehe
`Web/Bilder/HorseArt.cs`).

## Format

- **JPEG**, keine Transparenz nötig (die gelieferten Zeichnungen haben weißen
  Hintergrund; ursprünglich war PNG mit Alphakanal vorgesehen, aber die ersten echten
  Bilder kamen als JPEG - `HorseArt.cs` liest darum `.jpg` für `base/` und `coats/`).
- Bildausschnitt: **Porträt, Kopf und Hals**, Blickrichtung nach **links**.

## Ordnerstruktur

```
horses/
  base/          {rassenschlüssel}.jpg    Grundbild je Rasse
  coats/         {rasse}_{fellfarbe}.jpg  optional, geht vor base
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
2. Fellfarbe → `horses/coats/{rasse}_{fellfarbe}.jpg`
3. Rassen-Grundbild → `horses/base/{rasse}.jpg`
4. Eine per Fellfarbe eingefärbte Silhouette (kein Dateizugriff, kann nicht fehlschlagen)

Die Komponente `PferdBild.razor` reicht diese Liste als `<img>` mit einem `onerror`
durch, das bei jedem Fehlschlag zum nächsten Kandidaten wechselt - komplett im Browser,
ohne .NET-Umweg. Der letzte Kandidat ist ein `data:image/svg+xml`-Bild und kann darum
nie fehlschlagen: es bleibt garantiert nie eine Lücke im Layout, egal wie viele echte
Grafiken noch fehlen.

## Vorhandene Dateien

Liste unten entspricht dem, was aus den Rassendaten gebraucht wird (`Sim/Bilddateien.cs`
- bei Änderungen an `Core/Data/rassen.json` neu ausführen und diese Liste abgleichen).
Alle 24 Dateien sind seit den ersten echten Zeichnungen vorhanden. Da die Zeichnungen
Kopfporträts ohne rassetypische Merkmale sind, teilen sich mehrere Rassen dieselbe
Fellfarben-Zeichnung (z. B. dieselbe Rappe-Datei für alle fünf Rassen); Fuchs und
Schimmel gibt es je in einer helleren Variante (Araber) und einer kräftigeren (übrige
Rassen).

### Grundbilder (`horses/base/`)

```
araber.jpg
kaltblut.jpg
pony.jpg
vollblut.jpg
warmblut.jpg
```

### Fellfarben-Varianten, optional (`horses/coats/`)

```
araber_fuchs.jpg
araber_schimmel.jpg
araber_rappe.jpg
kaltblut_braun.jpg
kaltblut_rappe.jpg
kaltblut_fuchs.jpg
pony_braun.jpg
pony_falbe.jpg
pony_schecke.jpg
pony_rappe.jpg
vollblut_fuchs.jpg
vollblut_braun.jpg
vollblut_rappe.jpg
vollblut_schimmel.jpg
warmblut_braun.jpg
warmblut_fuchs.jpg
warmblut_rappe.jpg
warmblut_schimmel.jpg
warmblut_falbe.jpg
```

### Einzigartige Pferde (`horses/uniques/`)

Noch keine definiert - kommt mit dem Inhalt für einzigartige Pferde.

### Szene und Oberfläche (`scene/`, `ui/`)

Werden erst mit dem Ambiente aus Block 4 der Phase-4-Vorgabe genutzt
(geschichteter Hintergrund, Trainingsbalken-Sprite). Ordner liegen schon bereit.
