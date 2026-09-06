namespace Core;

/// <summary>Ein besessenes Exemplar einer AusruestungsDefinition. Getrennt von der Definition, weil
/// der Spieler mehrere gleiche Stücke besitzen und jedes einzeln einem Pferd umhängen können soll.
/// Die Effekte werden beim Kauf aus der Definition kopiert (sie sind fest, nicht gewürfelt), damit
/// Spielstand.Advance sie ohne Zugriff auf die Inhaltsdatenbank auswerten kann.</summary>
public class Ausruestung
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DefinitionId { get; set; } = "";
    public Guid? AngelegtBeiPferdId { get; set; }
    public List<AusruestungsEffekt> Effekte { get; set; } = new();
}
