using Core;

namespace Sim;

/// <summary>Listet alle Bilddateinamen auf, die das Spiel aktuell sucht - erzeugt aus den
/// Rassendaten statt von Hand gepflegt, damit die Liste in wwwroot/img/README.md nicht veraltet
/// (siehe Phase-4-Auftrag, Block 3). Bei jeder Änderung an Core/Data/rassen.json hier neu
/// ausführen und die Liste in der README ersetzen.</summary>
public static class Bilddateien
{
    public static void Drucken(Inhaltsdatenbank inhalte)
    {
        Console.WriteLine("Erwartete Bilddateien (aus den Rassendaten erzeugt):");
        Console.WriteLine();
        Console.WriteLine("Grundbilder (horses/base/):");
        foreach (var rasse in inhalte.AlleRassen().OrderBy(r => r.Id))
            Console.WriteLine($"  {rasse.Id}.png");

        Console.WriteLine();
        Console.WriteLine("Fellfarben-Varianten, optional (horses/coats/):");
        foreach (var rasse in inhalte.AlleRassen().OrderBy(r => r.Id))
            foreach (var farbe in rasse.Farben)
                Console.WriteLine($"  {rasse.Id}_{farbe.ToLowerInvariant()}.png");
    }
}
