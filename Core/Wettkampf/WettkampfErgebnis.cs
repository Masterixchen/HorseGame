namespace Core;

/// <summary>Ein abgeschlossenes Ergebnis, damit der Spieler beim nächsten Reinschauen sieht, was
/// passiert ist. Reine Anzeige-Historie, keine Spiellogik hängt daran.</summary>
public class WettkampfErgebnis
{
    public Guid PferdId { get; set; }
    public string PferdName { get; set; } = "";
    public string WettkampfklasseId { get; set; } = "";
    public DateTime Zeitpunkt { get; set; }
    public int Platzierung { get; set; }
    public int Preisgeld { get; set; }
    public int AnsehenGewonnen { get; set; }
    public MaterialTyp? GewonnenesMaterial { get; set; }
}
