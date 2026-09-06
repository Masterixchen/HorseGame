namespace Core;

/// <summary>Der eine laufende Bauauftrag - es kann immer nur an einem Gebäude gleichzeitig gebaut
/// werden. Resolves in Spielstand.Advance, sobald Fertig erreicht ist.</summary>
public class AusbauAuftrag
{
    public Gebaeude Gebaeude { get; set; }
    public DateTime Beginn { get; set; }
    public DateTime Fertig { get; set; }
}
