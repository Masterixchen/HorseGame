namespace Core;

/// <summary>Die Blutlinienstufe ist das Gegenstück zum Gegenstandslevel: sie begrenzt, welche
/// Merkmalsstufe bei einem Wurf überhaupt möglich ist. Ein Spitzenmerkmal aus einer schwachen
/// Linie darf es laut Projektanweisung nicht geben.</summary>
public static class BlutlinienRegeln
{
    public static int MaxMerkmalsstufe(int blutlinienstufe, int anzahlDefinierterStufen) =>
        Math.Clamp((blutlinienstufe + 1) / 2, 1, anzahlDefinierterStufen);
}
