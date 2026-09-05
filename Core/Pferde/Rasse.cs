namespace Core;

public class StatBereich
{
    public float Min { get; set; }
    public float Max { get; set; }
}

/// <summary>Die Basis eines Pferdes. Legt die Grundwerte fest und bestimmt, welche Präfixe und
/// Suffixe überhaupt rollen können.</summary>
public class Rasse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Beschreibung { get; set; } = "";

    public StatBereich TempoPotenzial { get; set; } = new();
    public StatBereich AusdauerPotenzial { get; set; } = new();
    public StatBereich SprungkraftPotenzial { get; set; } = new();
    public StatBereich RittigkeitPotenzial { get; set; } = new();

    public string ImplizitesMerkmal { get; set; } = "";
    public List<string> MoeglichePraefixe { get; set; } = new();
    public List<string> MoeglicheSuffixe { get; set; } = new();
    public List<string> Farben { get; set; } = new();

    public StatBereich Potenzial(StatTyp typ) => typ switch
    {
        StatTyp.Tempo => TempoPotenzial,
        StatTyp.Ausdauer => AusdauerPotenzial,
        StatTyp.Sprungkraft => SprungkraftPotenzial,
        StatTyp.Rittigkeit => RittigkeitPotenzial,
        _ => throw new ArgumentOutOfRangeException(nameof(typ))
    };
}
