using Core;

namespace Sim;

/// <summary>Zeigt, ob die Zufallserzeugung von Marktpferden plausibel aussieht: Verteilung der
/// Seltenheitsstufen über viele Würfe, plus ein paar Beispielpferde zum Anschauen.</summary>
public static class Verteilungsbericht
{
    public static void Drucken(GameRandom zufall, Inhaltsdatenbank inhalte)
    {
        var rassen = inhalte.AlleRassen().ToList();
        var jetzt = DateTime.UtcNow;

        const int anzahlWuerfe = 20_000;
        var verteilung = new Dictionary<Seltenheit, int>();

        for (int i = 0; i < anzahlWuerfe; i++)
        {
            var rasse = zufall.Waehle(rassen);
            var pferd = PferdeGenerator.Erzeuge(zufall, inhalte, rasse, jetzt);
            verteilung[pferd.Seltenheit] = verteilung.GetValueOrDefault(pferd.Seltenheit) + 1;
        }

        Console.WriteLine($"Seltenheitsverteilung über {anzahlWuerfe:N0} Würfe (Marktpferde):");
        foreach (var stufe in Enum.GetValues<Seltenheit>())
        {
            int anzahl = verteilung.GetValueOrDefault(stufe);
            double anteil = 100.0 * anzahl / anzahlWuerfe;
            Console.WriteLine($"  {stufe,-12} {anzahl,7:N0}  ({anteil,5:F1} %)");
        }

        Console.WriteLine();
        Console.WriteLine("Fünf Beispielpferde:");
        for (int i = 0; i < 5; i++)
        {
            var rasse = zufall.Waehle(rassen);
            var pferd = PferdeGenerator.Erzeuge(zufall, inhalte, rasse, jetzt);
            DruckePferd(pferd, rasse, inhalte);
        }
    }

    private static void DruckePferd(Pferd pferd, Rasse rasse, Inhaltsdatenbank inhalte)
    {
        Console.WriteLine($"- {pferd.Name} ({pferd.Geschlecht}, {rasse.Name}, {pferd.Farbe}), " +
                           $"Seltenheit {pferd.Seltenheit}, Blutlinienstufe {pferd.Blutlinienstufe}");

        foreach (var (typ, stat) in pferd.Werte.Alle())
            Console.WriteLine($"    {typ,-12} {stat.Aktuell,5:F1} / {stat.Potenzial,5:F1}");

        foreach (var merkmal in pferd.AlleMerkmale())
        {
            var definition = inhalte.HoleMerkmal(merkmal.DefinitionId);
            var effekte = string.Join(", ", merkmal.Effekte.Select(e => $"{e.Attribut} {e.Wert:+0.0;-0.0}%"));
            Console.WriteLine($"    [{definition.Art}] {definition.Name} {merkmal.StufeRoemisch()} - {effekte}");
        }
    }
}
