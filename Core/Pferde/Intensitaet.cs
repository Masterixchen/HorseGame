namespace Core;

/// <summary>Die eigentliche Entscheidung beim Training: schnell und anstrengend oder schonend und
/// langsam. Ersetzt die feste "4 bis 8 Stunden"-Dauer aus den ersten Phasen - die war ein Fehler,
/// weil sie den Spieler nach ein paar Klicks nichts mehr tun ließ (siehe Phase-4-Auftrag).</summary>
public enum Intensitaet
{
    Leicht,
    Mittel,
    Intensiv
}

public static class IntensitaetsRegeln
{
    // Erste Entwürfe, zum Feintunen mit dem Sim-Werkzeug gedacht. Höhere Intensität bringt mehr
    // Fortschritt pro Einheit, kostet aber überproportional mehr Kondition - eine Sitzung mit
    // lauter Intensiv-Einheiten ist schneller vorbei als eine mit Leicht-Einheiten.
    public static TimeSpan Dauer(Intensitaet intensitaet) => intensitaet switch
    {
        Intensitaet.Leicht => TimeSpan.FromMinutes(5),
        Intensitaet.Mittel => TimeSpan.FromMinutes(8),
        Intensitaet.Intensiv => TimeSpan.FromMinutes(12),
        _ => TimeSpan.FromMinutes(8)
    };

    public static float Konditionskosten(Intensitaet intensitaet) => intensitaet switch
    {
        Intensitaet.Leicht => 8f,
        Intensitaet.Mittel => 15f,
        Intensitaet.Intensiv => 24f,
        _ => 15f
    };

    /// <summary>Anteil der Restlücke zum Potenzial, der pro Einheit geschlossen wird.</summary>
    public static float ErtragsAnteil(Intensitaet intensitaet) => intensitaet switch
    {
        Intensitaet.Leicht => 0.05f,
        Intensitaet.Mittel => 0.09f,
        Intensitaet.Intensiv => 0.14f,
        _ => 0.09f
    };
}
