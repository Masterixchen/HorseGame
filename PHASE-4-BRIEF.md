# Phase 4 — Umbau: Zeitsystem, Hof, Bilder, Spielgefühl

Auftrag nach dem Abschluss von Phase 3. Vier Blöcke, in dieser Reihenfolge
abzuarbeiten, weil die späteren auf dem ersten aufbauen.

**Wichtig:** Block 1 ersetzt die Zeitangaben in `CLAUDE.md`. Nach Abschluss den
Abschnitt „Zeitmodell" dort entsprechend aktualisieren, damit die Datei nicht
veraltete Vorgaben enthält.

---

# Block 1 — Zeitsystem grundlegend umbauen

## Was heute falsch ist

Alles dauert Stunden bis Tage. Man startet drei Trainings und hat dann nichts mehr
zu tun. Das Spiel schickt den Spieler weg, statt ihn spielen zu lassen.
Die ursprüngliche Vorgabe „Training vier bis acht Stunden" war ein Fehler.

## Das neue Prinzip

**Nicht die Uhr begrenzt, sondern die Kondition.**

Ein Pferd hat Kondition von 0 bis 100. Eine Trainingseinheit kostet Kondition und
dauert nur wenige Minuten. Man kann dasselbe Pferd mehrmals hintereinander
trainieren, bis es müde ist. Kondition regeneriert über die Zeit.

Daraus ergibt sich der gewünschte Verlauf von allein:

- **Anfang:** wenige Pferde, aber jedes kann mehrere Einheiten am Stück.
  Eine erste Sitzung ist durchgehend beschäftigt.
- **Später:** viele Pferde, immer sind einige frisch. Es gibt immer etwas zu tun,
  aber nie etwas, das man tun *muss*.
- **Ende einer Sitzung:** wenn alles müde ist, hört man von selbst auf.
  Das Spiel sagt nicht „komm später wieder", es geht einem einfach aus.

Grundsatz 1 und 2 aus `CLAUDE.md` gelten unverändert: Nichts verfällt, und wer
selten spielt, verliert nichts. Mehr Aktivität soll **schneller** voranbringen,
nicht mehr Ertrag pro Aktion geben.

## Neue Dauern

Alles deutlich kürzer. Richtwerte, zum Tunen gedacht:

| Vorgang | Dauer |
|---|---|
| Trainingseinheit | 5–12 Minuten, je nach Intensität |
| Kondition vollständig zurück | ca. 4 Stunden, sinkt mit Weide-Ausbau |
| Wettkampf Stufe 1 | startet alle 20 Minuten |
| Wettkampf Stufe 2 | alle 45 Minuten |
| Wettkampf Stufe 3 | alle 2 Stunden |
| Wettkampf Stufe 4 | alle 6 Stunden |
| Wettkampf Stufe 5 | einmal täglich |
| Trächtigkeit | 4 Stunden, mit Zuchtstall bis auf 2,5 Stunden |
| Fohlen: Merkmale enthüllen sich | über 8 Stunden in mehreren Schritten |
| Gebäudeausbau | 15 Minuten bis 4 Stunden je nach Stufe |
| Materialerzeugung | alle 30 Minuten ein Zuwachs, Deckel bei 7 Tagen |

**Alterung:** Ein Pferd soll etwa vier bis sechs Wochen Echtzeit lang
konkurrenzfähig sein. Danach bleibt es als Elterntier wertvoll — Grundsatz 5.
Den Umrechnungsfaktor Spieljahr zu Echtzeit daraus ableiten und in einer Konstante
zentral ablegen, damit er sich einfach nachjustieren lässt.

## Warteschlangen — der Kern der Sache

Jedes Pferd hat eine **Auftragsliste**, die man mit mehreren Einheiten füllen kann.
Sie arbeitet sich selbstständig ab, auch wenn das Spiel geschlossen ist, und pausiert
automatisch, wenn die Kondition zu niedrig wird.

Das ist der Mechanismus, der „kann, muss nicht" umsetzt:

- Wer zehn Minuten Zeit hat, füllt alle Warteschlangen und geht.
- Wer eine Stunde bleibt, greift laufend ein, reagiert auf Kondition und Stimmung
  und legt Wettkämpfe passend dazwischen. Das bringt spürbar mehr — aber durch
  bessere Entscheidungen, nicht durch mehr Klicks.

Dazu ein **Stallplan**: eine Ansicht, in der man allen Pferden auf einmal
Warteschlangen zuweist, mit Vorlagen wie „aufbauen", „auf Wettkampf vorbereiten",
„schonen".

## Wettkämpfe

Wettkämpfe laufen in festen Intervallen, nicht zu Uhrzeiten. Man meldet an, der
Wettkampf läuft zum nächsten Termin, das Ergebnis liegt beim nächsten Reinschauen vor.
Man kann für mehrere kommende Termine im Voraus anmelden.

Es darf **keinen verpassbaren Termin** geben. Wer nicht da ist, verliert nichts.

## Frühe Spielphase verdichten

Was den Anfang beschäftigt hält, sind nicht mehr Klicks, sondern **neue Systeme, die
sich nacheinander öffnen**. Vorschlag für die ersten ein bis zwei Stunden:
alle paar Minuten schaltet etwas frei — erst Training, dann der erste Wettkampf,
dann Markt, dann Ausrüstung, dann der Hof, dann Zucht, dann Materialien.

Jede Freischaltung ist ein kurzer Moment mit eigener Einblendung. Keine Tutorial-Texte,
die man wegklickt, sondern Dinge, die auftauchen und selbsterklärend sind.

## Was dabei nicht passieren darf

- Keine Belohnung fürs Einloggen, keine Tagesboni, keine Serien.
- Kein Zeitfenster, das man verpassen kann.
- Keine Aktion, die man nur deshalb macht, weil ein Zähler abgelaufen ist.
  Jede Aktion muss eine Entscheidung enthalten.
- Keine Abkürzung gegen Bezahlung, auch nicht gegen Spielwährung.

Wenn eine Änderung das Spiel in Richtung Mobilspiel-Pflichtprogramm schiebt,
ist sie falsch — auch wenn sie „mehr zu tun" erzeugt.

## Abnahme

Fertig, wenn: In der ersten Spielstunde gibt es durchgehend etwas Sinnvolles zu tun,
ohne dass man auf einen Timer starrt. Nach einer Woche Spielzeit kann man in zehn
Minuten alles Nötige erledigen oder eine Stunde sinnvoll investieren, und der
Unterschied zeigt sich im Fortschritt, nicht im Ertrag pro Aktion.

---

# Block 2 — Der Hof als eigener Bereich

## Was sich ändert

Der Ausbau wird aus dem Markt herausgelöst und bekommt einen eigenen Reiter „Hof".
Der Markt behält nur noch Kauf, Verkauf und Auktionen.

Der Hof ist keine Liste von Gebäuden mit Ausbaubutton, sondern eine **Ansicht des
Geländes**, in der die Gebäude stehen und anklickbar sind. Gebäude im Bau sind als
Baustelle sichtbar, mit Fortschrittsbalken. Pferde stehen sichtbar auf der Weide
(siehe Block 4).

## Der Hof wirkt auf die Zucht

Das ist die eigentliche inhaltliche Änderung. Bisher ist der Ausbau ein Geldfresser
mit kleinen Boni. Er soll stattdessen der **Multiplikator auf das Zuchtsystem** werden —
also auf das, was den Reiz des Spiels ausmacht.

| Gebäude | Wirkung auf die Zucht |
|---|---|
| **Zuchtstall** | hebt die maximal erreichbare Blutlinienstufe an. Ohne Ausbau sind die hohen Merkmalsstufen schlicht unerreichbar. |
| **Fohlenaufzucht** | hebt die Untergrenze des Wurfs an. Ein gut aufgezogenes Fohlen verliert nichts von seiner Anlage, ein schlecht aufgezogenes schon. |
| **Genetiklabor** | zeigt mehr Details in der Wahrscheinlichkeitsvorschau und enthüllt Merkmale früher. |
| **Weide** | Kondition und Stimmung, und damit indirekt Trächtigkeitsdauer und Wurfqualität. |
| **Deckstation** | erlaubt mehrere Trächtigkeiten gleichzeitig und senkt die Materialkosten. |
| **Tierarztstation** | kürzere Heilung, weniger Überlastungsverletzungen. |
| **Futterlager und Verwaltung** | laufende Kosten und Marktangebote, wie bisher. |

So wird der Ausbau zu einer echten Fortschrittsachse: Man züchtet nicht besser, weil
man mehr Geld hat, sondern weil der Hof mehr zulässt. Das gibt dem Geld eine klare
Aufgabe und dem Ausbau eine spürbare Belohnung.

Die Wirkung jedes Gebäudes muss **in der Zuchtvorschau sichtbar** sein: „Blutlinienstufe
maximal 34 (Zuchtstall Stufe 2 — Ausbau hebt auf 41)". Der Spieler soll jederzeit sehen,
was ihm gerade fehlt.

## Abnahme

Fertig, wenn: Man kann in der Zuchtplanung ablesen, welches Gebäude gerade der
begrenzende Faktor ist, und der Ausbau dieses Gebäudes verändert die Vorschau sichtbar.

---

# Block 3 — Bilder und Ordnerstruktur

Der Besitzer legt die Bilder später selbst ab. Gebraucht wird die Struktur, die
Auflösungslogik und ein Platzhalter, der nie kaputt aussieht.

## Ordner

```
wwwroot/img/
  horses/
    base/          {rassenschlüssel}.png        — Grundbild je Rasse
    coats/         {rasse}_{fellfarbe}.png      — optional, geht vor base
    uniques/       {schlüssel}.png              — einzigartige Pferde
    _placeholder.svg                            — mitgeliefert
  scene/
    sky.png  hills.png  barn.png  fence.png
    grass_far.png  grass_near.png
  ui/
    horse_run.png                               — Sprite-Streifen, 8 Bilder
    icons/
```

## Auflösungsreihenfolge

Eine Klasse `HorseArt` mit einer Methode, die für ein Pferd den Bildpfad liefert:

1. einzigartiges Pferd → `uniques/{schlüssel}.png`
2. sonst `coats/{rasse}_{fellfarbe}.png`
3. sonst `base/{rasse}.png`
4. sonst `_placeholder.svg`

Zusätzlich im `<img>` ein `onerror`, das auf den Platzhalter zurückfällt. Fehlende
Dateien dürfen weder die Konsole zumüllen noch eine Lücke im Layout hinterlassen.

## Mitgelieferte Platzhalter

Für jede Rasse eine schlichte SVG-Silhouette erzeugen, eingefärbt nach der Fellfarbe
des Pferdes. Damit sieht das Spiel auch ohne eine einzige echte Grafik vollständig aus,
und man erkennt die Pferde trotzdem auseinander.

## Dokumentation

Eine `wwwroot/img/README.md` anlegen, in der steht: erwartetes Format (PNG mit
Transparenz), empfohlene Kantenlänge (512 × 512), Bildausschnitt (Porträt, Kopf und
Hals, Blickrichtung nach links), und die vollständige Liste der Dateinamen, die das
Spiel derzeit sucht. Die Liste wird aus den Rassendaten erzeugt, damit sie nicht
veraltet.

---

# Block 4 — Spielgefühl

Das Spiel wirkt statisch. Es fehlt nicht an Inhalt, sondern an Rückmeldung und Leben.

## Grundregel für alle Bewegung

Zwei Arten von Bewegung, klar getrennt:

- **Umgebung** bewegt sich langsam, weich und endlos. Gräser, Wolken, ein Pferd,
  das den Kopf hebt. Das darf man nicht bewusst wahrnehmen.
- **Rückmeldung** ist schnell und kurz, 150 bis 300 Millisekunden. Ein Wert steigt,
  eine Karte dreht sich um, ein Balken füllt sich.

Was nie passieren darf: blinkende Elemente, dauerhaft pulsierende Buttons, alles was
Aufmerksamkeit einfordert. Das Spiel soll ruhig sein — lebendig ist nicht dasselbe wie
aufdringlich.

`prefers-reduced-motion` respektieren: Umgebungsanimationen abschalten,
Rückmeldungsanimationen auf sofortige Zustandswechsel reduzieren.

## Seltenheit sichtbar machen

Der Unterschied zwischen einem gewöhnlichen und einem einzigartigen Pferd muss im
Kartenraster auf einen Blick erkennbar sein. Steigerung über fünf Stufen:

| Stufe | Behandlung |
|---|---|
| Gewöhnlich | schlichte Karte, gedämpfte Farben, dünner Rahmen |
| Solide | farbiger Rahmen, leicht kräftigere Farben |
| Selten | Rahmen plus zarter Farbverlauf im Hintergrund |
| Elite | zusätzlich ein langsam wandernder Lichtschimmer über die Karte, alle paar Sekunden |
| Einzigartig | Goldrahmen, eigener Kartenhintergrund, wenige langsam aufsteigende Lichtpunkte, kräftigste Farben |

Wichtig: Die Sättigung steigt mit der Seltenheit. Gewöhnliche Pferde sollen bewusst
matter aussehen, damit die guten strahlen. Das ist billiger und wirkungsvoller als
jede zusätzliche Animation.

## Hintergrund und Ambiente

Eine geschichtete Szene hinter der Oberfläche: Himmel, Hügel, Stallgebäude, Zaun,
Weide im Vordergrund. Beim Scrollen bewegen sich die Ebenen unterschiedlich schnell.

Dazu zwei Dinge, die viel Wirkung für wenig Aufwand bringen:

- **Tageszeit** aus der echten Uhr: Der Farbton der Szene wandert von Morgenlicht über
  Tag und Abendrot zu Nacht. Vier bis sechs Farbstimmungen genügen, dazwischen wird
  überblendet.
- **Wetter** gelegentlich: leichter Regen, Nebel, klarer Tag. Rein optisch, ohne
  Spielwirkung — sonst wird daraus wieder eine Mechanik, die Anwesenheit belohnt.

In der Hofansicht stehen die eigenen Pferde auf der Weide, als kleine Figuren.
Wer gerade trainiert, ist nicht da. Wer verletzt ist, steht am Stall. Das ist der
stärkste einzelne Hebel gegen den Eindruck, das Spiel sei tot.

## Fortschritt zeigen statt nennen

Überall, wo bisher „fertig am 14.03. um 19:42" steht, kommt ein Balken mit
Restzeit und Prozentwert, der sich live füllt. Betrifft: Training, Trächtigkeit,
Heilung, Bauvorhaben, Kondition, Aufwachsen des Fohlens, Zeit bis zum nächsten
Wettkampf, Materialerzeugung.

Der Trainingsbalken bekommt ein **galoppierendes Pferd**, das entlang des Balkens
läuft. Dasselbe Sprite dient als Ladeanzeige.

Bei kurzen Vorgängen (unter einer Minute) läuft der Balken sichtbar durch, statt
zu springen — das ist der Unterschied zwischen „es passiert etwas" und „eine Zahl
hat sich geändert".

## Rückmeldung auf Aktionen

- Steigt ein Wert, schwebt die Differenz kurz nach oben: `+2 Tempo`.
- Enthüllt sich ein Merkmal am Fohlen, dreht sich die Karte um. Bei seltenen
  Merkmalen länger und mit mehr Aufwand — der Moment ist der emotionale Höhepunkt
  des Spiels und darf sich Zeit lassen.
- Wettkampfergebnisse laufen als kurze Sequenz ab: Feld erscheint, Platzierungen
  zählen sich auf, das eigene Pferd wird hervorgehoben. Nicht als fertige Tabelle.
- Ereignisse während der Abwesenheit erscheinen beim Start nacheinander als kurze
  Einblendungen, nicht als Textwand.
- Optional und niedrig priorisiert: leise Umgebungsgeräusche und ein Wiehern bei
  seltenen Funden. Standardmäßig aus, abschaltbar.

## Abnahme

Fertig, wenn: Ein Bildschirmfoto der Stallübersicht zeigt sofort, welches Pferd das
beste ist. Und wenn man das Spiel eine Minute offen liegen lässt, bewegt sich etwas,
ohne dass es stört.

---

# Reihenfolge und Vorgehen

Block 1, dann 2, dann 3, dann 4. Nach jedem Block anhalten und den Stand vorführen,
bevor der nächste beginnt.

Block 1 ist eine Änderung an den Regeln, nicht nur an Zahlen — Warteschlangen und
Konditionsbegrenzung greifen in `Advance` ein. Vorher kurz beschreiben, was sich
im Zeitmodell ändert, und abstimmen, bevor die Umsetzung beginnt.

Nach Block 1 die Simulation in `Sim` an die neuen Dauern anpassen und die Balance
erneut über mehrere Spielmonate durchrechnen. Die alten Werte waren auf tagelange
Vorgänge ausgelegt und stimmen danach nicht mehr.
