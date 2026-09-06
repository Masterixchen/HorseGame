namespace Core;

/// <summary>Der gesamte Spielstand: Zeit, Zufallsquelle und alle Pferde. Wird als Ganzes
/// gespeichert und geladen (siehe Speicher/SpielstandService).</summary>
public class Spielstand
{
    public DateTime ZuletztAktualisiert { get; set; }
    public GameRandom Zufall { get; set; } = new();
    public List<Pferd> Pferde { get; set; } = new();

    // Kommen ab Phase 3 aus Wettkämpfen, Hof-Erzeugung und Verkäufen. Bis die Wirtschaft steht,
    // startet jedes Spiel mit einer kleinen Grundausstattung, damit sich die Zucht schon jetzt
    // ausprobieren lässt.
    public Dictionary<MaterialTyp, int> Materialbestand { get; set; } = new();

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

            // Neugeborene erst nach der Schleife anhängen - während der Iteration die Liste selbst
            // zu verändern würde eine Exception auslösen.
            var neugeborene = new List<Pferd>();
            foreach (var pferd in Pferde)
            {
                pferd.Advance(zeitpunkt, naechsterZeitpunkt);
                if (pferd.Traechtigkeit != null && pferd.Traechtigkeit.Geburtstermin <= naechsterZeitpunkt)
                {
                    neugeborene.Add(pferd.Traechtigkeit.Fohlen);
                    pferd.Traechtigkeit = null;
                }
            }
            Pferde.AddRange(neugeborene);

            zeitpunkt = naechsterZeitpunkt;
        }
        ZuletztAktualisiert = jetzt;
    }

    /// <summary>Prüft alle Voraussetzungen, bucht die Materialien ab und setzt die Stute trächtig.
    /// Das Fohlen selbst wird sofort von ZuchtRechner gewürfelt - diese Methode kümmert sich nur um
    /// den Rahmen (Validierung, Materialbestand, Zuweisung).</summary>
    public void StarteZucht(Inhaltsdatenbank inhalte, Pferd mutter, Pferd vater, ZuchtEinsatz einsatz, DateTime jetzt)
    {
        if (mutter.Geschlecht != Geschlecht.Stute || vater.Geschlecht != Geschlecht.Hengst)
            throw new InvalidOperationException("Es braucht eine Stute und einen Hengst.");
        if (mutter.Id == vater.Id)
            throw new InvalidOperationException("Ein Pferd kann nicht mit sich selbst gezüchtet werden.");
        if (mutter.Traechtigkeit != null)
            throw new InvalidOperationException("Diese Stute ist bereits trächtig.");
        if (!mutter.AlleMerkmaleBekannt || !vater.AlleMerkmaleBekannt)
            throw new InvalidOperationException("Beide Elterntiere müssen vollständig aufgedeckt sein.");
        if (!einsatz.IstGueltig())
            throw new InvalidOperationException("Wagnis lässt sich nicht mit Garantie-Materialien kombinieren.");

        foreach (var material in einsatz.GenutzteMaterialien())
            if (Materialbestand.GetValueOrDefault(material) < 1)
                throw new InvalidOperationException($"Nicht genug {material} vorhanden.");

        foreach (var material in einsatz.GenutzteMaterialien())
            Materialbestand[material] -= 1;

        mutter.Traechtigkeit = ZuchtRechner.StarteZucht(Zufall, inhalte, mutter, vater, einsatz, jetzt);
    }
}
