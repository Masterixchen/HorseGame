namespace Core;

/// <summary>Läuft nach einer bestätigten Zucht auf der Mutterstute. Das Fohlen steht zu diesem
/// Zeitpunkt bereits fest gewürfelt fest - "geboren" wird nur der Zeitpunkt, ab dem es im Stall
/// auftaucht (siehe ZuchtRechner: gewürfelt wird bei der Zucht, nie später).</summary>
public class Traechtigkeit
{
    public DateTime Beginn { get; set; }
    public DateTime Geburtstermin { get; set; }
    public Pferd Fohlen { get; set; } = null!;
}
