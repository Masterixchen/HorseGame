namespace Core;

public partial class Spielstand
{
    private const int HofAusbauBasiskosten = 400;

    /// <summary>Wöchentliche, gedeckelte Materialerzeugung des Hofes - braucht keine
    /// Inhaltsdatenbank (Materialtypen sind ein reines Aufzählungs-Enum), läuft deshalb direkt in
    /// Advance(DateTime). Jede Hofstufe bringt zwei zufällige Materialien pro Woche.</summary>
    private void ProduziereWochenmaterial()
    {
        var typen = Enum.GetValues<MaterialTyp>();
        int anzahl = Hofstufe * 2;
        for (int i = 0; i < anzahl; i++)
        {
            var typ = Zufall.Waehle(typen);
            Materialbestand[typ] = Materialbestand.GetValueOrDefault(typ) + 1;
        }
    }

    /// <summary>Kosten steigen mit jeder Stufe - ein größerer Hof bringt mehr Materialien und lässt
    /// (siehe Pferd.Advance) alle Pferde etwas schneller erholen.</summary>
    public int HofAusbauKosten() => HofAusbauBasiskosten * Hofstufe;

    public void HofAusbauen()
    {
        int kosten = HofAusbauKosten();
        if (Guthaben < kosten)
            throw new InvalidOperationException("Nicht genug Guthaben für den Ausbau.");

        Guthaben -= kosten;
        Hofstufe += 1;
    }
}
