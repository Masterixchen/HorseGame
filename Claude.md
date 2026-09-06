# Pferdegestüt — Projektanweisung

Diese Datei liegt im Wurzelverzeichnis und wird zu Beginn jeder Sitzung gelesen.
Sie beschreibt, was gebaut wird und wie gearbeitet wird.

## Was gebaut wird

Ein ruhiges Einzelspieler-Sammelspiel um Pferde. Man züchtet, sammelt, bildet aus
und lässt antreten. Der Reiz liegt darin, dass jedes Pferd wie ein Gegenstand aus
Diablo oder Path of Exile aufgebaut ist: eine Basis, darauf zufällig gewürfelte
Merkmale in Stufen und Wertebereichen, und ganz selten einzigartige Pferde,
die eigene Regeln brechen.

Die Zeit läuft in Echtzeit weiter, auch wenn das Spiel geschlossen ist.
Kein Rundensystem, kein Server, keine Datenbank, kein Mehrspielermodus.

## Designgrundsätze

Diese fünf Sätze stehen über allen Einzelentscheidungen. Wenn ein Vorschlag
einem davon widerspricht, ist der Vorschlag falsch.

1. **Das Spiel bestraft Abwesenheit nicht.** Nichts verfällt, nichts verhungert,
   nichts läuft ab. Wer drei Wochen wegbleibt, findet fertiges Training vor.
2. **Mehr Spielen wird belohnt, mehr Einloggen nicht.** Zeitgebundene Ressourcen
   sammeln sich offline bis zu einer Obergrenze von sieben Tagen an. Der Unterschied
   zwischen viel und wenig Spielen entsteht durch bessere Entscheidungen,
   nicht durch Anwesenheit.
3. **Gewürfelt wird bei der Zucht, nie am lebenden Pferd.** Ein geborenes Pferd ist
   permanent. Es gibt keine Mechanik, die ein vorhandenes Pferd verschlechtert,
   verbraucht oder zerstört. Am lebenden Tier gibt es nur Training, Ausrüstung und Pflege.
4. **Kein Verkaufsdruck.** Keine Echtgeldwährung, keine Wartezeiten, die man
   abkürzen kann, keine künstliche Knappheit.
5. **Die Sammlung veraltet nicht.** Ein neues Pferd darf ein altes nie schlicht ersetzen.

## Wer daran arbeitet

Der Besitzer kann C# und etwas Unity, hat aber keine Erfahrung mit Webentwicklung
oder Datenbanken. Er will den Code lesen und selbst weiterentwickeln können.

Daraus folgen harte Regeln:

- **Keine NuGet-Pakete** außer den unten genannten. Wenn du eines brauchst: erst fragen.
- **Keine Abstraktion auf Vorrat.** Keine Interfaces mit einer Implementierung,
  keine Repository-Muster, kein Dependency-Injection-Geflecht, keine Events,
  wo ein Methodenaufruf reicht.
- **Kurze Dateien.** Über 300 Zeilen wird aufgeteilt.
- **Kommentare auf Deutsch** und nur dort, wo das *Warum* nicht offensichtlich ist.
- Nach jeder abgeschlossenen Aufgabe in zwei, drei Sätzen erklären, was sich geändert
  hat und wo man es findet.
- Bei Unklarheiten im Spieldesign nachfragen statt raten. Balancing-Zahlen sind
  Vorschläge, keine Vorgaben.

## Technischer Rahmen

- .NET 8, C#
- **`Core`** — Klassenbibliothek, gesamte Spiellogik. Darf nichts über die Oberfläche
  wissen: kein `Console`, kein direkter Dateizugriff außerhalb der Speicherklasse.
  Grund: Die Bibliothek soll später unverändert unter einer anderen Oberfläche laufen.
- **`Web`** — Blazor WebAssembly, die Oberfläche. Wird zu statischen Dateien kompiliert
  und braucht keinen Server. Spielstand als JSON im Browser-Speicher, dazu Export
  und Import als Datei.
- **`Sim`** — Konsolenanwendung zum Durchrechnen der Balance.
- **`Tests`** — xUnit, nur für Formeln, Vererbung und Wahrscheinlichkeiten.

Der gesamte Zufall läuft über **eine** Instanz von `GameRandom` mit gespeichertem
Startwert, niemals über `Random.Shared` verstreut im Code. Damit lassen sich
Spielverläufe reproduzieren und Fehler nachstellen.

Thematische Texte — Rassennamen, Merkmalsnamen, Beschreibungen, Namenslisten —
stehen in JSON-Dateien unter `Core/Data`, nicht im Code.

## Pferde als Gegenstände

Das ist das Herzstück. Ein Pferd besteht aus:

```
Basis (Rasse)        gibt Grundwerte und bestimmt, welche Merkmale rollen können
Implizites Merkmal   jede Rasse hat genau eines, immer vorhanden
Präfixe              körperliche Anlagen, bis zu 3
Suffixe              Wesen und Gesundheit, bis zu 3
```

**Seltenheit begrenzt die Zahl der Merkmale:**

| Stufe | Präfixe | Suffixe |
|---|---|---|
| Gewöhnlich | 0 | 0 |
| Solide | bis 1 | bis 1 |
| Selten | bis 2 | bis 2 |
| Elite | bis 3 | bis 3 |
| Einzigartig | feste, benannte Pferde mit eigenen Regeln |

**Merkmale haben Stufen und Wertebereiche.** Nicht „Windläufer", sondern
„Windläufer III (+11 % Tempo)" aus dem Bereich +8 bis +16. Genau das erzeugt die
Jagd nach dem fast perfekten Exemplar. Die Anzeige zeigt die Position im Bereich an,
damit man sieht, wie gut der Wurf war.

**Präfixe** wirken auf Werte und Potenzial: Tempo, Ausdauer, Sprungkraft,
Rittigkeit, Wachstumsgeschwindigkeit.

**Suffixe** wirken auf alles andere: Trainingsgeschwindigkeit, Verletzungsanfälligkeit,
Erholung, Stimmung, Verhalten im Wettkampf, Futterbedarf, Vererbungsstärke.

Ein Teil der Suffixe ist **zweischneidig**: ein starker Bonus mit echtem Preis.
Solche Merkmale sind interessanter als reine Boni und sollen häufiger vorkommen
als in den meisten Spielen.

**Blutlinienstufe** ist die Entsprechung zum Gegenstandslevel. Sie ergibt sich aus
den Eltern und den verwendeten Materialien und legt fest, welche Merkmalsstufen
überhaupt rollen können. Ein Spitzenmerkmal aus einer schwachen Linie ist unmöglich —
das ist der Grund, über Generationen zu züchten.

## Die Zuchtplanung

Hier sitzt das gesamte Glücksspiel. Ablauf:

1. Zwei Elterntiere wählen
2. Materialien einlegen, die die Wahrscheinlichkeiten verschieben
3. Die Vorschau zeigt **ehrliche Wahrscheinlichkeiten** an: Chance auf jede
   Seltenheitsstufe, mögliche Merkmalsfamilien, Bereich der Blutlinienstufe
4. Bestätigen, Trächtigkeit läuft, Fohlen kommt

Die Vorschau lügt nie und rundet nicht beschönigend. Wer 4 Prozent liest, hat
4 Prozent.

**Materialien** und was sie tun:

| Material | Wirkung |
|---|---|
| Kraftfutter | hebt die Blutlinienstufe leicht an |
| Ahnentafel | garantiert eine Mindestzahl an Merkmalen |
| Fremdblut | garantiert ein Merkmal aus einer gewählten Familie, Rest zufällig |
| Spezialistenbetreuung | fügt gezielt ein bestimmtes Merkmal auf niedriger Stufe hinzu |
| Seltene Linie | hebt die maximal mögliche Merkmalsstufe deutlich an |
| Wagnis | hohe Streuung nach oben und unten, nicht kombinierbar mit Garantien |

Materialien kommen aus Wettkampfpreisen, aus der laufenden Erzeugung des Hofes
(mit Wochendeckel) und als Nebenprodukt von Verkäufen. Sie werden nie mit Geld
gekauft, sonst wird Geld zur einzigen Ressource.

## Unbekannte Merkmale

Ein Fohlen zeigt anfangs nur Rasse, Geschlecht und Aussehen. Die Merkmale enthüllen
sich nach und nach beim Aufwachsen, oder sofort gegen eine tierärztliche Untersuchung,
deren Kosten mit dem geschätzten Wert steigen.

Das ist der unidentifizierte Gegenstand: Der Moment, in dem sich zeigt, was man
gezogen hat, ist der emotionale Höhepunkt der Schleife und muss entsprechend
inszeniert werden.

## Einzigartige Pferde

Etwa zwölf bis zwanzig fest definierte, benannte Pferde. Jedes hat feste Merkmale
und **bricht eine Regel des Systems**. Beispiele für die Art von Effekt:

- ein Pferd ohne Potenzialobergrenze in genau einem Wert, dafür ohne jede Begabung in den anderen
- ein Pferd, das seine Merkmale zu 100 Prozent vererbt, aber selbst nie antreten kann
- ein Pferd, dessen Werte mit dem Alter steigen statt zu fallen

Einzigartige Pferde sind **findbar, nicht herstellbar**. Sie tauchen bei Auktionen,
seltenen Ereignissen und mit sehr geringer Wahrscheinlichkeit bei der Zucht auf.
Jedes hat einen kurzen Text, der erklärt, woher es kommt.

## Entwicklung am lebenden Pferd

Alles hier ist verlustfrei und planbar. Kein Zufall, keine Rückschläge.

- **Training** läuft über eine Warteschlange aus mehreren Einheiten, die sich von
  selbst abarbeitet, auch während das Spiel geschlossen ist. Nicht die Uhr begrenzt
  das Training, sondern die Kondition: jede Einheit kostet Kondition und dauert nur
  wenige Minuten (5 bis 12, je nach gewählter Intensität), nähert die Werte dem
  Potenzial an mit abnehmendem Ertrag. Reicht die Kondition nicht, pausiert die
  Warteschlange von selbst und läuft weiter, sobald sich das Pferd erholt hat.
- **Ausrüstung** — Sattel, Zaumzeug, Beschlag, Decken — gibt Boni, die man zwischen
  Pferden umhängen kann. Ausrüstung ist die zweite Sammelachse und darf ruhig
  eigene Seltenheitsstufen haben.
- **Pflege und Unterbringung** wirken auf Erholung und Stimmung.

Verletzungen entstehen aus Überlastung, nicht aus Zufallsstrafen, und heilen immer
vollständig aus. Ein Pferd wird durch nichts dauerhaft schlechter, außer durch Alter.

## Wettkämpfe

Mehrere Stufen, freigeschaltet über Ansehen. Jeder Wettkampf hat eine Disziplin,
die die Grundwerte unterschiedlich gewichtet. Ausgangsformel, zum Tunen gedacht:

```
Wertung = Summe(Wert * Gewicht)
        * Altersfaktor
        * (0,62 + 0,38 * Kondition/100)
        * (0,82 + 0,30 * Stimmung/100)
        * Merkmalsmodifikatoren
        * Ausrüstungsmodifikatoren
        * Zufall 0,85 bis 1,15
```

Gegner werden um einen stufenabhängigen Basiswert gestreut. Preisgeld für die ersten
drei, kleines Antrittsgeld bis Platz sechs, Materialien als Zusatzpreis.

Wettkämpfe finden in festen Intervallen statt, nicht zu Uhrzeiten (Stufe 1 alle
20 Minuten, bis Stufe 5 einmal täglich). Man meldet vorher an — auch für mehrere
kommende Termine auf einmal — und sieht das Ergebnis beim nächsten Reinschauen.
Es gibt keinen verpassbaren Termin: Anwesenheit zum Zeitpunkt ist nicht nötig,
wer nicht da ist, verliert nichts.

## Damit die Sammlung nicht veraltet

Konstruktionsvorgabe, keine Feinheit:

- Ausgereizte Pferde sind wertvolle Elterntiere. Ein Pferd mit perfekten Merkmalen
  ist auch mit zwanzig Jahren noch der Schlüssel zur nächsten Generation.
- Disziplinen sind getrennt. Ein Dressurspezialist ist im Springen nutzlos.
- Manche Wettkämpfe haben Beschränkungen: Altersgrenzen, nur eine Rasse,
  nur Eigenzucht, nur Pferde ohne Elite-Merkmale.
- Zweischneidige Merkmale machen Pferde situativ stark statt allgemein besser.

## Zeitmodell

Es gibt **keine tickende Schleife**. Alles wird aus Zeitstempeln berechnet.

```csharp
public void Advance(DateTime now)
```

Wird beim Laden und vor jeder Spieleraktion aufgerufen, arbeitet in Schritten von
höchstens einer Stunde, damit sich Zustände nicht überholen. Je Schritt: fällige
Trächtigkeiten und Wettkampf-Anmeldungen auswerten, Kosten anteilig abbuchen,
Materialerzeugung gutschreiben. Innerhalb dieser Stundenschritte geht jedes Pferd
sein eigenes, feineres Tempo: seine Trainings-Warteschlange arbeitet sich in
Minutenschritten ab, solange Zeit und Kondition reichen, und pausiert von selbst,
sobald die Kondition für die nächste Einheit nicht mehr ausreicht.

**Nicht die Uhr begrenzt das Spielen, sondern die Kondition.** Ein Pferd kann
mehrere Trainingseinheiten hintereinander abarbeiten, bis es müde ist; Kondition
regeneriert danach über die Zeit (Basis: vollständig in ca. vier Stunden, schneller
mit besserer Unterbringung). Das erzeugt von allein den gewünschten Verlauf: am
Anfang ist ein Pferd schnell erschöpft und eine erste Sitzung durchgehend
beschäftigt, später mit vielen Pferden ist immer eines frisch. Mehr Aktivität
bringt schnelleren Fortschritt, nicht mehr Ertrag pro Aktion.

Zeitgebundene Ressourcen (Materialerzeugung) sammeln sich offline bis zu einer
Obergrenze von sieben Tagen an (Designgrundsatz 2) — der Deckel wird einmalig gegen
das tatsächliche Ziel von `Advance` geprüft, nicht gegen jeden Stundenschritt für
sich, sonst würde er nie greifen.

Bei langer Abwesenheit werden Schritte zusammengefasst, aber terminierte Ereignisse
müssen in richtiger Reihenfolge auslösen. Beim Start bekommt der Spieler eine
Zusammenfassung dessen, was seit dem letzten Besuch passiert ist.

**Alterung** läuft beschleunigt gegenüber der Echtzeit (siehe
`Zeitkonstanten.AlterungsFaktor`), damit ein Pferd rund vier bis sechs Wochen
Echtzeit konkurrenzfähig bleibt und danach als Elterntier wertvoll wird
(Designgrundsatz 5). Nur die Alters*anzeige* und darauf basierende Regeln sind
davon betroffen, keine anderen Zeitabläufe.

## Phasen

Jede Phase endet mit etwas Spielbarem. Nicht mit der nächsten anfangen, bevor die
vorige läuft und von Hand ausprobiert wurde.

**Phase 1 — Pferde und Merkmale**
Datenmodell, Merkmalssystem mit Stufen und Bereichen, Rassen, Zufallserzeugung,
Speichern und Laden, `Advance` mit Training und Erholung. Oberfläche: Stallübersicht
als Kartenraster, Detailansicht eines Pferdes, Training starten.
*Fertig, wenn:* man ein Pferd trainieren, das Fenster schließen, später öffnen kann
und der Fortschritt korrekt nachgerechnet ist — und die Karten gut aussehen.

**Phase 2 — Zucht und Materialien**
Zuchtplanung mit ehrlicher Wahrscheinlichkeitsvorschau, Materialien, Trächtigkeit,
unbekannte Merkmale und ihre Enthüllung, Blutlinienstufe.
*Fertig, wenn:* eine über mehrere Generationen gezüchtete Linie nachweisbar besser
ist als alles Gekaufte.

**Phase 3 — Wirtschaft und Wettkämpfe**
Geld, laufende Kosten, Markt, Wettkämpfe, Ansehen, Ausbau des Hofes, Ausrüstung.
*Fertig, wenn:* die Balance über mehrere Simulationsjahre stabil ist.

**Phase 4 — Einzigartige Pferde, Aussehen, Feinschliff**
Einzigartige Pferde, Fellfarben und Abzeichen als Bildebenen, Zufallsereignisse,
Zusammenfassung nach Abwesenheit.

## Balance-Werkzeug

Spätestens in Phase 2 anlegen: `Sim` rechnet mit einer einfachen Strategie mehrere
Spieljahre in Sekunden durch und gibt pro Jahr Kontostand, Ansehen, Bestandsgröße,
Materialvorräte und das beste Pferd aus.

Ohne dieses Werkzeug ist Balancing Raten. In einem früheren Prototyp kippte die
Wirtschaft still in eine Abwärtsspirale, die beim normalen Spielen erst nach
Stunden aufgefallen wäre.

Zusätzlich soll `Sim` **Merkmalsverteilungen** über zehntausende Würfe ausgeben,
damit man sieht, ob die seltenen Ergebnisse wirklich so selten sind wie gedacht.

## Was ausdrücklich nicht gebaut wird

Kein Onlinemodus, kein Handel zwischen Spielern, keine Accounts, keine Datenbank,
kein serverseitiger Code, keine Echtgeldwährung, kein Mobilexport.
Eine reine Bestenliste zum Vergleich mit Freunden ist später denkbar, wird aber
jetzt nicht vorbereitet.
