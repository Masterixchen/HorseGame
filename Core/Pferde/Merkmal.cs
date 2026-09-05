namespace Core;

public enum MerkmalsArt
{
    Praefix,
    Suffix,
    Implizit
}

/// <summary>Eine Wertspanne, aus der bei einer bestimmten Stufe gewürfelt wird.</summary>
public class MerkmalsEffekt
{
    public Merkmalsattribut Attribut { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }
}

/// <summary>Eine Stufe eines Merkmals, z.B. "Windläufer III". Mehrere Effekte gleichzeitig bilden
/// zweischneidige Merkmale ab: ein echter Bonus mit einem echten Preis.</summary>
public class MerkmalsStufe
{
    public int Stufe { get; set; }
    public List<MerkmalsEffekt> Effekte { get; set; } = new();
}

/// <summary>Die unveränderliche Beschreibung eines Merkmals, wie sie aus merkmale.json geladen
/// wird. Was davon auf einem konkreten Pferd gewürfelt wurde, steht in MerkmalsInstanz.</summary>
public class MerkmalsDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public MerkmalsArt Art { get; set; }

    // Merkmale derselben Familie schließen sich beim Zuchtmaterial "Fremdblut" ein (Phase 2).
    public string Familie { get; set; } = "";
    public string Beschreibung { get; set; } = "";
    public List<MerkmalsStufe> Stufen { get; set; } = new();
}
