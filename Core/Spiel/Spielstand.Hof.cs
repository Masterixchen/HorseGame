namespace Core;

public partial class Spielstand
{
    private const int HofAusbauBasiskosten = 400;
    private const int MaterialTaktMinuten = 30;
    private static readonly TimeSpan MaterialMaxNachholzeit = TimeSpan.FromDays(7);

    /// <summary>Deckelt den Rückstand auf höchstens sieben Tage, bevor die stundenweise Schleife in
    /// Advance überhaupt beginnt (Designgrundsatz 2: zeitgebundene Ressourcen sammeln sich offline
    /// nur bis zu einer Obergrenze von sieben Tagen an).</summary>
    private void BegrenzeMaterialNachholzeit(DateTime jetzt)
    {
        var fruehesterZaehlbarerZeitpunkt = jetzt - MaterialMaxNachholzeit;
        if (NaechsteMaterialproduktion < fruehesterZaehlbarerZeitpunkt)
            NaechsteMaterialproduktion = fruehesterZaehlbarerZeitpunkt;
    }

    /// <summary>Alle 30 Minuten ein Materialschub, Hofstufe Stück pro Schub - braucht keine
    /// Inhaltsdatenbank (Materialtypen sind ein reines Enum), läuft deshalb direkt in
    /// Advance(DateTime).</summary>
    private void ProduziereFaelligesMaterial(DateTime bis)
    {
        var typen = Enum.GetValues<MaterialTyp>();
        while (NaechsteMaterialproduktion <= bis)
        {
            for (int i = 0; i < Hofstufe; i++)
            {
                var typ = Zufall.Waehle(typen);
                Materialbestand[typ] = Materialbestand.GetValueOrDefault(typ) + 1;
            }
            NaechsteMaterialproduktion = NaechsteMaterialproduktion.AddMinutes(MaterialTaktMinuten);
        }
    }

    public int HofAusbauKosten() => HofAusbauBasiskosten * Hofstufe;

    /// <summary>15 Minuten bis 4 Stunden, je nach Zielstufe - erste Schätzung, zum Tunen gedacht.</summary>
    public static TimeSpan HofAusbauDauer(int zielstufe) =>
        TimeSpan.FromMinutes(Math.Min(240, 15 + Math.Max(0, zielstufe - 2) * 25));

    public void HofAusbauen(DateTime jetzt)
    {
        if (HofAusbauFertig != null)
            throw new InvalidOperationException("Es läuft bereits ein Ausbau.");

        int kosten = HofAusbauKosten();
        if (Guthaben < kosten)
            throw new InvalidOperationException("Nicht genug Guthaben für den Ausbau.");

        Guthaben -= kosten;
        HofAusbauFertig = jetzt + HofAusbauDauer(Hofstufe + 1);
    }
}
