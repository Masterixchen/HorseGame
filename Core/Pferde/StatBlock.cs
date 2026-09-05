namespace Core;

/// <summary>Ein Kernwert mit aktuellem Stand und trainierbarem Potenzial. Aktuell nähert sich durch
/// Training Potenzial an, überschreitet es aber nie (Designgrundsatz 3: nur Training, kein Zufall
/// am lebenden Pferd).</summary>
public class Stat
{
    public float Aktuell { get; set; }
    public float Potenzial { get; set; }
}

public class StatBlock
{
    public Stat Tempo { get; set; } = new();
    public Stat Ausdauer { get; set; } = new();
    public Stat Sprungkraft { get; set; } = new();
    public Stat Rittigkeit { get; set; } = new();

    public Stat Hole(StatTyp typ) => typ switch
    {
        StatTyp.Tempo => Tempo,
        StatTyp.Ausdauer => Ausdauer,
        StatTyp.Sprungkraft => Sprungkraft,
        StatTyp.Rittigkeit => Rittigkeit,
        _ => throw new ArgumentOutOfRangeException(nameof(typ))
    };

    public IEnumerable<(StatTyp Typ, Stat Wert)> Alle()
    {
        yield return (StatTyp.Tempo, Tempo);
        yield return (StatTyp.Ausdauer, Ausdauer);
        yield return (StatTyp.Sprungkraft, Sprungkraft);
        yield return (StatTyp.Rittigkeit, Rittigkeit);
    }
}
