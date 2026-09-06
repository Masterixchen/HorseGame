namespace Core;

/// <summary>Anders als ein Merkmal hat ein Ausrüstungseffekt keinen Wertebereich - Ausrüstung wird
/// gekauft, nicht gewürfelt (Designgrundsatz 3 gilt nur für lebende Pferde, nicht für Gegenstände,
/// aber ein Sattel soll trotzdem verlässlich das tun, was auf dem Preisschild steht).</summary>
public class AusruestungsEffekt
{
    public Merkmalsattribut Attribut { get; set; }
    public float Wert { get; set; }
}

/// <summary>Ein Ausrüstungsstück, wie es im Laden steht. Sattel, Zaumzeug, Beschlag und Decken
/// haben eigene Seltenheitsstufen (fest vorgegeben, nicht gewürfelt) und lassen sich zwischen
/// Pferden umhängen - siehe Ausruestung für das konkrete, besessene Exemplar.</summary>
public class AusruestungsDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public AusruestungsSlot Slot { get; set; }
    public Seltenheit Seltenheit { get; set; }
    public int Preis { get; set; }
    public string Beschreibung { get; set; } = "";
    public List<AusruestungsEffekt> Effekte { get; set; } = new();
}
