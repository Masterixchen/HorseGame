using Core;

namespace Sim;

/// <summary>Belegt das "Fertig, wenn" aus der Projektanweisung für Phase 2: eine über mehrere
/// Generationen gezielt gezüchtete Linie soll nachweisbar besser sein als frisch zugekaufte
/// Marktpferde. Zieht pro Generation mehrere Fohlen, behält die beiden besten (nach
/// Tempo-Potenzial) als nächstes Elternpaar und vergleicht am Ende mit dem Marktdurchschnitt.</summary>
public static class Zuchtbericht
{
    public static void Drucken(GameRandom zufall, Inhaltsdatenbank inhalte)
    {
        var rasse = inhalte.HoleRasse("vollblut");
        var einsatz = new ZuchtEinsatz();
        var gebaeudestufen = Enum.GetValues<Gebaeude>().ToDictionary(g => g, _ => 1);

        var mutter = PferdeGenerator.Erzeuge(zufall, inhalte, rasse, DateTime.UtcNow, Geschlecht.Stute);
        var vater = PferdeGenerator.Erzeuge(zufall, inhalte, rasse, DateTime.UtcNow, Geschlecht.Hengst);

        Console.WriteLine("Zuchtlinie über Generationen (Rasse: Vollblut, Zuchtziel: Tempo):");
        DruckeGeneration(0, mutter, vater);

        const int generationen = 6;
        for (int generation = 1; generation <= generationen; generation++)
        {
            var kandidaten = Enumerable.Range(0, 10)
                .Select(_ => ZuchtRechner.ErzeugeFohlen(zufall, inhalte, mutter, vater, einsatz, gebaeudestufen, DateTime.UtcNow))
                .OrderByDescending(f => f.Werte.Tempo.Potenzial)
                .ToList();

            mutter = kandidaten.First(f => f.Geschlecht == Geschlecht.Stute);
            vater = kandidaten.First(f => f.Geschlecht == Geschlecht.Hengst);
            DruckeGeneration(generation, mutter, vater);
        }

        float zuchtDurchschnitt = (mutter.Werte.Tempo.Potenzial + vater.Werte.Tempo.Potenzial) / 2f;

        const int marktAnzahl = 500;
        float marktSumme = 0f;
        for (int i = 0; i < marktAnzahl; i++)
            marktSumme += PferdeGenerator.Erzeuge(zufall, inhalte, rasse, DateTime.UtcNow).Werte.Tempo.Potenzial;
        float marktDurchschnitt = marktSumme / marktAnzahl;

        Console.WriteLine();
        Console.WriteLine($"Marktdurchschnitt Tempo-Potenzial (über {marktAnzahl:N0} frische Vollblüter): {marktDurchschnitt:F1}");
        Console.WriteLine($"Zuchtlinie nach {generationen} Generationen:                        {zuchtDurchschnitt:F1}");
        Console.WriteLine(zuchtDurchschnitt > marktDurchschnitt
            ? "-> Die gezüchtete Linie liegt über dem Markt."
            : "-> Die gezüchtete Linie liegt NICHT über dem Markt - Formeln prüfen.");
    }

    private static void DruckeGeneration(int generation, Pferd mutter, Pferd vater)
    {
        float durchschnitt = (mutter.Werte.Tempo.Potenzial + vater.Werte.Tempo.Potenzial) / 2f;
        int merkmale = mutter.Praefixe.Count + mutter.Suffixe.Count + vater.Praefixe.Count + vater.Suffixe.Count;
        Console.WriteLine($"  Generation {generation,2}: Tempo-Potenzial Ø {durchschnitt,5:F1}  " +
                           $"(Blutlinie {mutter.Blutlinienstufe}/{vater.Blutlinienstufe}, {merkmale} Merkmale bei den Eltern)");
    }
}
