using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core;

/// <summary>Wandelt den Spielstand in JSON um und zurück. Diese Klasse selbst fasst keine Datei
/// oder den Browser-Speicher an - wohin der Text geschrieben wird, entscheidet die Oberfläche
/// (Datei bei Sim, localStorage plus Export/Import bei Web).</summary>
public static class SpielstandService
{
    private static readonly JsonSerializerOptions Optionen = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Speichern(Spielstand spielstand) => JsonSerializer.Serialize(spielstand, Optionen);

    public static Spielstand Laden(string json) =>
        JsonSerializer.Deserialize<Spielstand>(json, Optionen)
        ?? throw new InvalidOperationException("Spielstand konnte nicht gelesen werden.");
}
