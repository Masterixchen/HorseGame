namespace Core;

/// <summary>Ein wartender Eintrag in der Trainings-Warteschlange eines Pferdes - noch ohne
/// Zeitstempel, die bekommt er erst, wenn die Einheit tatsächlich beginnt (siehe Training).</summary>
public class Trainingsauftrag
{
    public StatTyp Ziel { get; set; }
    public Intensitaet Intensitaet { get; set; }
}
