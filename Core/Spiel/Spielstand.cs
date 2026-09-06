namespace Core;

/// <summary>Der gesamte Spielstand: Zeit, Zufallsquelle, alle Pferde und die Wirtschaft. Wird als
/// Ganzes gespeichert und geladen (siehe Speicher/SpielstandService). Auf mehrere Dateien
/// aufgeteilt (Spielstand.Zucht.cs, .Wettkampf.cs, .Markt.cs, .Hof.cs), weil sonst eine einzelne
/// Datei zu groß würde - es bleibt eine einzige Klasse.</summary>
public partial class Spielstand
{
    public DateTime ZuletztAktualisiert { get; set; }
    public GameRandom Zufall { get; set; } = new();
    public List<Pferd> Pferde { get; set; } = new();

    // Für die Früh-Freischaltung neuer Systeme in der Oberfläche (Markt, Ausrüstung, Hof, Zucht,
    // Materialien schalten sich in den ersten Spielminuten nacheinander frei) - reine Anzeigesache,
    // keine Spielregel hängt daran.
    public DateTime Spielbeginn { get; set; }

    public int Guthaben { get; set; }
    public int Ansehen { get; set; }
    public int Hofstufe { get; set; } = 1;
    public DateTime? HofAusbauFertig { get; set; }

    // Getrennte Termine, weil nur die Materialproduktion ohne Inhaltsdatenbank auskommt und darum
    // in Advance(DateTime) laufen darf - der Marktnachschub braucht Rassen-Daten und wird darum
    // separat von der Oberfläche angestoßen (siehe Spielstand.Markt.cs).
    public DateTime NaechsteMaterialproduktion { get; set; }
    public DateTime NaechsteMarktAktualisierung { get; set; }

    public Dictionary<MaterialTyp, int> Materialbestand { get; set; } = new();
    public List<Pferd> MarktPferde { get; set; } = new();
    public List<Ausruestung> Ausruestungen { get; set; } = new();
    public List<WettkampfErgebnis> WettkampfErgebnisse { get; set; } = new();

    /// <summary>Rechnet die Zeit seit dem letzten Besuch in Schritten von höchstens einer Stunde
    /// nach, damit sich fällige Ereignisse nicht überholen (siehe Zeitmodell in der
    /// Projektanweisung). Wird beim Laden und vor jeder Spieleraktion aufgerufen - es gibt keine
    /// tickende Schleife im Hintergrund. Die feinere Minutentaktung von Training und Wettkämpfen
    /// steckt in Pferd.Advance selbst, das innerhalb jedes Stundenschritts sein eigenes Tempo geht.</summary>
    public void Advance(DateTime jetzt)
    {
        if (jetzt <= ZuletztAktualisiert) return;

        // Deckel einmalig gegen das tatsächliche Ziel prüfen, nicht gegen jeden Stundenschritt -
        // die äußere Schleife läuft ohnehin stundenweise durch, ein Deckel je Teilschritt würde
        // nie greifen, weil der Rückstand zu jedem Teilschritt-Zeitpunkt nie sieben Tage beträgt.
        BegrenzeMaterialNachholzeit(jetzt);

        var schrittGroesse = TimeSpan.FromHours(1);
        var zeitpunkt = ZuletztAktualisiert;
        while (zeitpunkt < jetzt)
        {
            var naechsterZeitpunkt = zeitpunkt + schrittGroesse;
            if (naechsterZeitpunkt > jetzt) naechsterZeitpunkt = jetzt;

            // Hofstufe wirkt wie eine bessere Unterbringung auf alle Pferde gleich; dazu kommt die
            // Decken-Ausrüstung des einzelnen Pferdes (Erholung und Stimmung zusammengefasst).
            float hofBonus = (Hofstufe - 1) * 15f;

            // Neugeborene und ausgewertete Wettkämpfe erst nach der Schleife anhängen bzw.
            // verarbeiten - während der Iteration die Pferdeliste selbst zu verändern würde eine
            // Exception auslösen.
            var neugeborene = new List<Pferd>();
            foreach (var pferd in Pferde)
            {
                float ausruestungsBonus = AusruestungsHelfer.ModifikatorSumme(pferd, Ausruestungen, Merkmalsattribut.Erholung)
                                         + AusruestungsHelfer.ModifikatorSumme(pferd, Ausruestungen, Merkmalsattribut.Stimmung);
                pferd.Advance(zeitpunkt, naechsterZeitpunkt, hofBonus + ausruestungsBonus);

                if (pferd.Traechtigkeit != null && pferd.Traechtigkeit.Geburtstermin <= naechsterZeitpunkt)
                {
                    neugeborene.Add(pferd.Traechtigkeit.Fohlen);
                    pferd.Traechtigkeit = null;
                }

                foreach (var anmeldung in pferd.Anmeldungen.Where(a => a.Zeitpunkt <= naechsterZeitpunkt).ToList())
                {
                    LoeseWettkampfAus(pferd, anmeldung, anmeldung.Zeitpunkt);
                    pferd.Anmeldungen.Remove(anmeldung);
                }
            }
            Pferde.AddRange(neugeborene);

            if (HofAusbauFertig != null && HofAusbauFertig <= naechsterZeitpunkt)
            {
                Hofstufe += 1;
                HofAusbauFertig = null;
            }

            BucheUnterhaltskosten(zeitpunkt, naechsterZeitpunkt);
            ProduziereFaelligesMaterial(naechsterZeitpunkt);

            zeitpunkt = naechsterZeitpunkt;
        }
        ZuletztAktualisiert = jetzt;
    }

    private void BucheUnterhaltskosten(DateTime von, DateTime bis)
    {
        const float basiskostenProTag = 5f;
        float stunden = (float)(bis - von).TotalHours;

        float summe = 0f;
        foreach (var pferd in Pferde)
            summe += basiskostenProTag / 24f * stunden * (1f + pferd.ModifikatorSumme(Merkmalsattribut.Futterbedarf) / 100f);

        // Guthaben darf ins Minus laufen - das Spiel bestraft Abwesenheit nicht, kein Pferd
        // verhungert oder wird zwangsverkauft (Designgrundsatz 1).
        Guthaben -= (int)MathF.Round(summe);
    }
}
