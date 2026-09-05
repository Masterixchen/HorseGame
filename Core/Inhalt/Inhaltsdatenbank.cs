using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core;

/// <summary>Lädt die thematischen Inhalte (Rassen, Merkmale, Namen) aus den JSON-Dateien unter
/// Core/Data. Die Dateien sind als eingebettete Ressourcen ins Assembly kompiliert statt als lose
/// Datei auf der Platte zu liegen - so läuft dieselbe Core-Bibliothek unverändert in der Konsole,
/// in Tests und im Browser (Blazor WebAssembly hat kein Dateisystem).</summary>
public class Inhaltsdatenbank
{
    private readonly Dictionary<string, Rasse> _rassen;
    private readonly Dictionary<string, MerkmalsDefinition> _merkmale;
    private readonly List<string> _namenStuten;
    private readonly List<string> _namenHengste;

    public Inhaltsdatenbank()
    {
        var optionen = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        _rassen = LadeListe<Rasse>("rassen.json", optionen).ToDictionary(r => r.Id);
        _merkmale = LadeListe<MerkmalsDefinition>("merkmale.json", optionen).ToDictionary(m => m.Id);

        var namen = LadeObjekt<NamensListen>("namen.json", optionen);
        _namenStuten = namen.Stuten;
        _namenHengste = namen.Hengste;
    }

    public IReadOnlyCollection<Rasse> AlleRassen() => _rassen.Values;
    public Rasse HoleRasse(string id) => _rassen[id];
    public MerkmalsDefinition HoleMerkmal(string id) => _merkmale[id];
    public IReadOnlyList<string> NamenFuer(Geschlecht geschlecht) => geschlecht == Geschlecht.Stute ? _namenStuten : _namenHengste;

    private static List<T> LadeListe<T>(string dateiname, JsonSerializerOptions optionen) => LadeObjekt<List<T>>(dateiname, optionen);

    private static T LadeObjekt<T>(string dateiname, JsonSerializerOptions optionen)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var ressourcenname = $"Core.Data.{dateiname}";
        using var stream = assembly.GetManifestResourceStream(ressourcenname)
            ?? throw new InvalidOperationException($"Eingebettete Datei nicht gefunden: {ressourcenname}");
        return JsonSerializer.Deserialize<T>(stream, optionen)
            ?? throw new InvalidOperationException($"Konnte {dateiname} nicht laden.");
    }

    private class NamensListen
    {
        public List<string> Stuten { get; set; } = new();
        public List<string> Hengste { get; set; } = new();
    }
}
