namespace Core;

/// <summary>Alle Formeln, die aus einer Gebäudestufe eine konkrete Zahl machen. Erste Entwürfe,
/// zum Feintunen mit dem Sim-Werkzeug gedacht. Absichtlich rein funktional (keine
/// Spielstand-Abhängigkeit), damit Core/Zucht/ZuchtRechner sie ohne Umweg nutzen kann und die
/// Oberfläche exakt dieselbe Formel für "was würde ein Ausbau bringen" anzeigen kann.</summary>
public static class GebaeudeRegeln
{
    private const int AusbauBasiskosten = 400;

    public static int AusbauKosten(int aktuelleStufe) => AusbauBasiskosten * aktuelleStufe;

    /// <summary>15 Minuten bis 4 Stunden, je nach Zielstufe.</summary>
    public static TimeSpan AusbauDauer(int zielstufe) =>
        TimeSpan.FromMinutes(Math.Min(240, 15 + Math.Max(0, zielstufe - 2) * 25));

    // --- Zuchtstall: hebt die maximal erreichbare Blutlinienstufe an und verkürzt die Trächtigkeit.

    public static int MaxBlutlinienstufe(int stufe) => 20 + stufe * 7;

    public static TimeSpan Traechtigkeitsdauer(int stufe) =>
        TimeSpan.FromHours(Math.Max(2.5, 4.0 - (stufe - 1) * 0.25));

    // --- Fohlenaufzucht: hebt die Untergrenze des Wurfs an - ein gut aufgezogenes Fohlen verliert
    // nichts von der Anlage seiner Eltern, ein schlecht aufgezogenes (niedrige Stufe) schon.

    public static float MinWerteStreuung(int stufe) => Math.Min(1f, 0.9f + (stufe - 1) * 0.02f);

    // --- Genetiklabor: enthüllt Merkmale früher und zeigt mehr Details in der Vorschau (siehe
    // ZuchtVorschauErgebnis.MerkmalsChancen, immer berechnet - die Oberfläche entscheidet anhand
    // dieser Stufe, ob sie es anzeigt).

    public static (float MinStunden, float MaxStunden) AufdeckZeitraum(int stufe) => (
        Math.Max(0.25f, 1f - (stufe - 1) * 0.1f),
        Math.Max(1f, 8f - (stufe - 1) * 1f)
    );

    public const int GenetiklaborDetailstufe = 2;

    // --- Weide: schnellere Erholung für alle Pferde (Kondition und Stimmung).

    public static float ErholungsBonusProzent(int stufe) => (stufe - 1) * 15f;

    // --- Deckstation: mehrere Trächtigkeiten gleichzeitig, güngstigere Materialien.

    public static int MaxGleichzeitigeTraechtigkeiten(int stufe) => stufe;

    public static float MaterialRueckerstattungsChance(int stufe) => Math.Min(0.5f, (stufe - 1) * 0.1f);

    // --- Futterlager und Verwaltung: laufende Kosten und Marktangebot.

    public static float UnterhaltsMultiplikator(int stufe) => Math.Max(0.5f, 1f - (stufe - 1) * 0.05f);

    public static int AnzahlMarktpferde(int stufe) => 5 + (stufe - 1);

    // --- Materialerzeugung ist keinem einzelnen Gebäude zugeordnet, sondern spiegelt den
    // gesamten Ausbaustand des Hofes wider - alle Gebäude starten bei Stufe 1 (Summe 7), das
    // entspricht der bisherigen Grundproduktion von 1 Material je Takt.
    public static int MaterialProZeittakt(IReadOnlyDictionary<Gebaeude, int> gebaeudestufen) =>
        Math.Max(0, gebaeudestufen.Values.Sum() - (Enum.GetValues<Gebaeude>().Length - 1));
}
