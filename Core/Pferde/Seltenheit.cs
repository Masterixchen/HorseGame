namespace Core;

/// <summary>Die Seltenheitsstufe eines Pferdes. Begrenzt, wie viele Präfixe und Suffixe es
/// tragen kann. Einzigartige Pferde werden nicht gewürfelt, sondern sind fest definiert (Phase 4).</summary>
public enum Seltenheit
{
    Gewoehnlich,
    Solide,
    Selten,
    Elite,
    Einzigartig
}

public static class SeltenheitRegeln
{
    // Tabelle aus der Projektanweisung: maximale Anzahl Präfixe/Suffixe je Seltenheitsstufe.
    public static int MaxPraefixe(Seltenheit stufe) => stufe switch
    {
        Seltenheit.Gewoehnlich => 0,
        Seltenheit.Solide => 1,
        Seltenheit.Selten => 2,
        Seltenheit.Elite => 3,
        Seltenheit.Einzigartig => 3,
        _ => 0
    };

    // Die Tabelle ist für Präfixe und Suffixe identisch.
    public static int MaxSuffixe(Seltenheit stufe) => MaxPraefixe(stufe);

    /// <summary>Würfelt eine Seltenheitsstufe für ein zufällig erzeugtes Pferd. Die Gewichtung ist
    /// ein erster Entwurf und zum Feintunen mit dem Sim-Werkzeug gedacht (siehe Projektanweisung).</summary>
    public static Seltenheit Wuerfle(GameRandom zufall)
    {
        double wert = zufall.NaechsteZahl();
        return wert switch
        {
            < 0.50 => Seltenheit.Gewoehnlich,
            < 0.78 => Seltenheit.Solide,
            < 0.94 => Seltenheit.Selten,
            _ => Seltenheit.Elite
        };
    }
}
