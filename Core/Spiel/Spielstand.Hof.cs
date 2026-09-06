namespace Core;

public partial class Spielstand
{
    private static readonly TimeSpan MaterialMaxNachholzeit = TimeSpan.FromDays(7);
    private const int MaterialTaktMinuten = 30;

    /// <summary>Deckelt den Rückstand auf höchstens sieben Tage, bevor die stundenweise Schleife in
    /// Advance überhaupt beginnt (Designgrundsatz 2: zeitgebundene Ressourcen sammeln sich offline
    /// nur bis zu einer Obergrenze von sieben Tagen an).</summary>
    private void BegrenzeMaterialNachholzeit(DateTime jetzt)
    {
        var fruehesterZaehlbarerZeitpunkt = jetzt - MaterialMaxNachholzeit;
        if (NaechsteMaterialproduktion < fruehesterZaehlbarerZeitpunkt)
            NaechsteMaterialproduktion = fruehesterZaehlbarerZeitpunkt;
    }

    /// <summary>Alle 30 Minuten ein Materialschub - die Menge spiegelt den gesamten Ausbaustand des
    /// Hofes wider (siehe GebaeudeRegeln.MaterialProZeittakt), keinem einzelnen Gebäude zugeordnet.
    /// Braucht keine Inhaltsdatenbank, läuft deshalb direkt in Advance(DateTime).</summary>
    private void ProduziereFaelligesMaterial(DateTime bis)
    {
        var typen = Enum.GetValues<MaterialTyp>();
        int anzahlProTakt = GebaeudeRegeln.MaterialProZeittakt(Gebaeudestufen);

        while (NaechsteMaterialproduktion <= bis)
        {
            for (int i = 0; i < anzahlProTakt; i++)
            {
                var typ = Zufall.Waehle(typen);
                Materialbestand[typ] = Materialbestand.GetValueOrDefault(typ) + 1;
            }
            NaechsteMaterialproduktion = NaechsteMaterialproduktion.AddMinutes(MaterialTaktMinuten);
        }
    }

    public int HofAusbauKosten(Gebaeude gebaeude) => GebaeudeRegeln.AusbauKosten(GebaeudeStufe(gebaeude));

    public void HofAusbauen(Gebaeude gebaeude, DateTime jetzt)
    {
        if (LaufenderAusbau != null)
            throw new InvalidOperationException("Es läuft bereits ein Ausbau.");

        int kosten = HofAusbauKosten(gebaeude);
        if (Guthaben < kosten)
            throw new InvalidOperationException("Nicht genug Guthaben für den Ausbau.");

        Guthaben -= kosten;
        LaufenderAusbau = new AusbauAuftrag { Gebaeude = gebaeude, Fertig = jetzt + GebaeudeRegeln.AusbauDauer(GebaeudeStufe(gebaeude) + 1) };
    }
}
