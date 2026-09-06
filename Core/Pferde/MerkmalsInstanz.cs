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

    // Bei Fohlen aus der Zucht ist ein Merkmal anfangs verborgen und deckt sich erst mit der Zeit
    // auf (siehe "Unbekannte Merkmale" in der Projektanweisung). Bei allen anderen Pferden
    // (Marktbestand, Startpferde) ist per Vorgabe alles von Anfang an bekannt.
    public bool Bekannt { get; set; } = true;
    public DateTime? Aufdeckzeitpunkt { get; set; }

    /// <summary>Prüft, ob die Aufdeckung fällig ist, und schaltet das Merkmal ggf. frei. Wird von
    /// Pferd.Advance für jedes Merkmal aufgerufen.</summary>
    public void PruefeAufdeckung(DateTime bis)
    {
        if (!Bekannt && Aufdeckzeitpunkt != null && Aufdeckzeitpunkt <= bis)
            Bekannt = true;
    }

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
