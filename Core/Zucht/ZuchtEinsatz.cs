namespace Core;

/// <summary>Die Materialien, die für einen einzelnen Zuchtversuch eingelegt werden. Jedes Material
/// wird höchstens einmal pro Versuch genutzt - "Kraftfutter hebt die Blutlinienstufe leicht an"
/// klingt nicht danach, dass ein zweites Kraftfutter das verdoppeln sollte.</summary>
public class ZuchtEinsatz
{
    public bool Kraftfutter { get; set; }
    public bool Ahnentafel { get; set; }
    public bool SelteneLinie { get; set; }
    public bool Wagnis { get; set; }

    // Diese beiden brauchen eine Zielangabe vom Spieler und sind deshalb keine reinen Häkchen.
    public string? FremdblutFamilie { get; set; }
    public string? SpezialistenMerkmalId { get; set; }

    public bool FremdblutGenutzt => !string.IsNullOrEmpty(FremdblutFamilie);
    public bool SpezialistenbetreuungGenutzt => !string.IsNullOrEmpty(SpezialistenMerkmalId);

    /// <summary>Wagnis (hohe Streuung) widerspricht den Garantie-Materialien - beides zusammen
    /// ergäbe keinen Sinn und ist laut Projektanweisung ausdrücklich ausgeschlossen.</summary>
    public bool IstGueltig()
    {
        bool garantieGenutzt = Ahnentafel || FremdblutGenutzt || SpezialistenbetreuungGenutzt;
        return !(Wagnis && garantieGenutzt);
    }

    /// <summary>Alle Materialtypen, die dieser Einsatz tatsächlich verbraucht - für Bestandsprüfung
    /// und Abbuchung.</summary>
    public IEnumerable<MaterialTyp> GenutzteMaterialien()
    {
        if (Kraftfutter) yield return MaterialTyp.Kraftfutter;
        if (Ahnentafel) yield return MaterialTyp.Ahnentafel;
        if (SelteneLinie) yield return MaterialTyp.SelteneLinie;
        if (Wagnis) yield return MaterialTyp.Wagnis;
        if (FremdblutGenutzt) yield return MaterialTyp.Fremdblut;
        if (SpezialistenbetreuungGenutzt) yield return MaterialTyp.Spezialistenbetreuung;
    }
}
