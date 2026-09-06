using Core;

namespace Web.Bilder;

/// <summary>Liefert für ein Pferd die Bildkandidaten in Reihenfolge, wie sie PferdBild.razor per
/// onerror durchprobiert: einzigartiges Pferd, dann Fellfarbe, dann Rassen-Grundbild, zuletzt eine
/// nach Fellfarbe eingefärbte Silhouette. Der letzte Kandidat ist ein data-URI (kein Dateizugriff,
/// keine Netzwerkanfrage) und kann darum nie fehlschlagen - so bleibt garantiert nie eine Lücke im
/// Layout, egal wie viele echte Grafiken noch fehlen.</summary>
public static class HorseArt
{
    // Dieselbe Silhouette wie in wwwroot/img/horses/_placeholder.svg (Kopf, Hals, Ohr), hier als
    // eine einzelne Pfadangabe mit zwei Teilstücken, damit sie sich mit einer Fellfarbe befüllen lässt.
    private const string SilhouettenPfad =
        "M55,8 C42,9 30,14 20,22 C12,28 8,34 10,38 C12,42 18,44 24,42 C33,46 42,45 47,40 " +
        "C50,50 54,68 56,88 C57,100 60,110 70,116 C82,120 92,116 96,112 C90,100 84,88 82,70 " +
        "C80,50 76,30 66,14 C63,9 59,7 55,8 Z M52,10 L46,0 L60,6 Z";

    // Grobe Zuordnung der in Core/Data/rassen.json verwendeten Fellfarben-Namen zu Tönen. Neue
    // Farben, die hier fehlen, fallen auf einen neutralen Ton zurück statt einen Fehler zu werfen.
    private static readonly Dictionary<string, string> FarbeZuHex = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Fuchs"] = "#b5651d",
        ["Braun"] = "#5c3a21",
        ["Rappe"] = "#262019",
        ["Schimmel"] = "#d8d3c7",
        ["Falbe"] = "#c9a66b",
        ["Schecke"] = "#8a7d6e",
    };

    private const string NeutralerTon = "#a8998a";

    public static IReadOnlyList<string> Kandidaten(Pferd pferd, Rasse rasse)
    {
        var liste = new List<string>();

        if (pferd.Seltenheit == Seltenheit.Einzigartig && !string.IsNullOrEmpty(pferd.EinzigartigerSchluessel))
            liste.Add($"img/horses/uniques/{pferd.EinzigartigerSchluessel}.png");

        liste.Add($"img/horses/coats/{rasse.Id}_{DateiSicher(pferd.Farbe)}.png");
        liste.Add($"img/horses/base/{rasse.Id}.png");
        liste.Add(SilhouetteAlsDatenUrl(pferd.Farbe));

        return liste;
    }

    private static string DateiSicher(string wert) => wert.Trim().ToLowerInvariant().Replace(' ', '_');

    private static string SilhouetteAlsDatenUrl(string farbe)
    {
        string hex = FarbeZuHex.GetValueOrDefault(farbe, NeutralerTon);
        string svg = $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 120'><path fill='{hex}' d='{SilhouettenPfad}'/></svg>";
        return "data:image/svg+xml," + Uri.EscapeDataString(svg);
    }
}
