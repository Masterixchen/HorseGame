namespace Core;

/// <summary>Die gerade laufende Trainingseinheit - aus der Warteschlange entnommen, sobald genug
/// Kondition da war. Pro Pferd kann immer nur eine gleichzeitig laufen.</summary>
public class Training
{
    public StatTyp Ziel { get; set; }
    public Intensitaet Intensitaet { get; set; }
    public DateTime Start { get; set; }
    public DateTime Ende { get; set; }
}
