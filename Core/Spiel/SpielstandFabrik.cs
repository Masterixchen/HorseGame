namespace Core;

/// <summary>Erzeugt einen frischen Spielstand mit Startpferden, Marktangebot und
/// Grundausstattung. An einer Stelle gebündelt, damit Web (echtes Spiel) und Sim
/// (Mehrjahres-Simulation für die Balance) exakt denselben Ausgangspunkt verwenden.</summary>
public static class SpielstandFabrik
{
    private const int StartGuthaben = 500;
    private const int AnzahlStartpferde = 6;
    private const int AnzahlMarktpferde = 5;

    public static Spielstand NeuesSpiel(Inhaltsdatenbank inhalte, DateTime jetzt, ulong? seed = null)
    {
        var zufall = seed.HasValue ? new GameRandom(seed.Value) : new GameRandom();
        var stand = new Spielstand
        {
            ZuletztAktualisiert = jetzt,
            Spielbeginn = jetzt,
            Zufall = zufall,
            Guthaben = StartGuthaben,
            Hofstufe = 1,
            NaechsteMaterialproduktion = jetzt,
            NaechsteMarktAktualisierung = jetzt.AddDays(7)
        };

        var rassen = inhalte.AlleRassen().ToList();
        for (int i = 0; i < AnzahlStartpferde; i++)
            stand.Pferde.Add(PferdeGenerator.Erzeuge(zufall, inhalte, zufall.Waehle(rassen), jetzt));
        for (int i = 0; i < AnzahlMarktpferde; i++)
            stand.MarktPferde.Add(PferdeGenerator.Erzeuge(zufall, inhalte, zufall.Waehle(rassen), jetzt));

        // Grundausstattung, solange Wettkämpfe und Hof noch keine eigenen Vorräte erzeugt haben.
        stand.Materialbestand[MaterialTyp.Kraftfutter] = 3;
        stand.Materialbestand[MaterialTyp.Ahnentafel] = 2;
        stand.Materialbestand[MaterialTyp.Fremdblut] = 2;
        stand.Materialbestand[MaterialTyp.Spezialistenbetreuung] = 2;
        stand.Materialbestand[MaterialTyp.SelteneLinie] = 1;
        stand.Materialbestand[MaterialTyp.Wagnis] = 2;

        return stand;
    }
}
