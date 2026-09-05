namespace Core;

/// <summary>Ein laufender Trainingsauftrag. Pro Pferd kann immer nur einer aktiv sein.</summary>
public class Training
{
    public StatTyp Ziel { get; set; }
    public DateTime Start { get; set; }
    public DateTime Ende { get; set; }
}
