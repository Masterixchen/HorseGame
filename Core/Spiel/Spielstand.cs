namespace Core;

/// <summary>Der gesamte Spielstand: Zeit, Zufallsquelle und alle Pferde. Wird als Ganzes
/// gespeichert und geladen (siehe Speicher/SpielstandService).</summary>
public class Spielstand
{
    public DateTime ZuletztAktualisiert { get; set; }
    public GameRandom Zufall { get; set; } = new();
    public List<Pferd> Pferde { get; set; } = new();

    /// <summary>Rechnet die Zeit seit dem letzten Besuch in Schritten von höchstens einer Stunde
    /// nach, damit sich fällige Ereignisse nicht überholen (siehe Zeitmodell in der
    /// Projektanweisung). Wird beim Laden und vor jeder Spieleraktion aufgerufen - es gibt keine
    /// tickende Schleife im Hintergrund.</summary>
    public void Advance(DateTime jetzt)
    {
        if (jetzt <= ZuletztAktualisiert) return;

        var schrittGroesse = TimeSpan.FromHours(1);
        var zeitpunkt = ZuletztAktualisiert;
        while (zeitpunkt < jetzt)
        {
            var naechsterZeitpunkt = zeitpunkt + schrittGroesse;
            if (naechsterZeitpunkt > jetzt) naechsterZeitpunkt = jetzt;

            foreach (var pferd in Pferde)
                pferd.Advance(zeitpunkt, naechsterZeitpunkt);

            zeitpunkt = naechsterZeitpunkt;
        }
        ZuletztAktualisiert = jetzt;
    }
}
