namespace Core;

/// <summary>Das gewürfelte Ergebnis eines einzelnen Effekts auf einem konkreten Pferd.</summary>
public class GewuerfelterEffekt
{
    public Merkmalsattribut Attribut { get; set; }
    public float Wert { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }

    /// <summary>Position des Wurfs im Bereich: 0 = schlechtestmöglich, 1 = bestmöglich.
    /// Für die Anzeige, damit man sieht, wie gut der Wurf war.</summary>
    public float Position => Max > Min ? (Wert - Min) / (Max - Min) : 1f;
}

/// <summary>Ein Merkmal, wie es tatsächlich auf einem Pferd sitzt: welche Definition, welche Stufe,
/// welche Werte innerhalb der Stufen-Bandbreite gewürfelt wurden.</summary>
public class MerkmalsInstanz
{
    public string DefinitionId { get; set; } = "";
    public int Stufe { get; set; }
    public List<GewuerfelterEffekt> Effekte { get; set; } = new();

    /// <summary>Römische Ziffer für die Anzeige, z.B. "III".</summary>
    public string StufeRoemisch() => Stufe switch
    {
        1 => "I",
        2 => "II",
        3 => "III",
        4 => "IV",
        5 => "V",
        _ => Stufe.ToString()
    };
}
