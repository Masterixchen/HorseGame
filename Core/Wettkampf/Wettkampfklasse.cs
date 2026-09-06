namespace Core;

/// <summary>Eine Wettkampfklasse bündelt Disziplin (nur zur Anzeige - die Gewichte hier sind das,
/// was tatsächlich zählt), Zulassung über Ansehen und die Preise. Findet jeden Tag zur gleichen
/// Uhrzeit statt (siehe WettkampfRechner.NaechsterZeitpunkt).</summary>
public class Wettkampfklasse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Disziplin { get; set; } = "";
    public int Stundenzeitpunkt { get; set; }
    public int BenoetigtesAnsehen { get; set; }

    public float GewichtTempo { get; set; }
    public float GewichtAusdauer { get; set; }
    public float GewichtSprungkraft { get; set; }
    public float GewichtRittigkeit { get; set; }

    public float GegnerBasiswert { get; set; }
    public int Preisgeld1 { get; set; }
    public int Preisgeld2 { get; set; }
    public int Preisgeld3 { get; set; }
    public int Antrittsgeld { get; set; }
    public int AnsehenErster { get; set; }
    public int AnsehenTeilnahme { get; set; }

    // Einschränkungen, damit ausgereizte Pferde nicht überall automatisch gewinnen (siehe
    // "Damit die Sammlung nicht veraltet" in der Projektanweisung). Bislang nur Altersgrenzen.
    public int? MinAlter { get; set; }
    public int? MaxAlter { get; set; }

    public float Gewicht(StatTyp typ) => typ switch
    {
        StatTyp.Tempo => GewichtTempo,
        StatTyp.Ausdauer => GewichtAusdauer,
        StatTyp.Sprungkraft => GewichtSprungkraft,
        StatTyp.Rittigkeit => GewichtRittigkeit,
        _ => 0f
    };
}
