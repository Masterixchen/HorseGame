namespace Core;

public class ZuchtVorschauErgebnis
{
    public Dictionary<Seltenheit, double> SeltenheitsChancen { get; set; } = new();
    public Dictionary<string, double> FamilienChancen { get; set; } = new();
    public int BlutlinienstufeMin { get; set; }
    public int BlutlinienstufeMax { get; set; }
}

/// <summary>Berechnet die ehrliche Wahrscheinlichkeitsvorschau vor einer Zucht. "Ehrlich" heißt
/// hier wörtlich: es wird nicht separat geschätzt, sondern der echte ZuchtRechner tausende Male
/// mit einem Wegwerf-Zufall durchgespielt und ausgezählt. Der gespeicherte Spielzufall bleibt dabei
/// unangetastet - allein das Ansehen einer Vorschau darf den weiteren Spielverlauf nicht verändern.</summary>
public static class ZuchtVorschau
{
    public static ZuchtVorschauErgebnis Berechne(Inhaltsdatenbank inhalte, Pferd mutter, Pferd vater, ZuchtEinsatz einsatz, int stichproben = 4000)
    {
        var zufall = new GameRandom();
        var ergebnis = new ZuchtVorschauErgebnis { BlutlinienstufeMin = int.MaxValue, BlutlinienstufeMax = int.MinValue };
        var seltenheitTreffer = new Dictionary<Seltenheit, int>();
        var familienTreffer = new Dictionary<string, int>();

        for (int i = 0; i < stichproben; i++)
        {
            var fohlen = ZuchtRechner.ErzeugeFohlen(zufall, inhalte, mutter, vater, einsatz, DateTime.UtcNow);

            seltenheitTreffer[fohlen.Seltenheit] = seltenheitTreffer.GetValueOrDefault(fohlen.Seltenheit) + 1;
            ergebnis.BlutlinienstufeMin = Math.Min(ergebnis.BlutlinienstufeMin, fohlen.Blutlinienstufe);
            ergebnis.BlutlinienstufeMax = Math.Max(ergebnis.BlutlinienstufeMax, fohlen.Blutlinienstufe);

            foreach (var merkmal in fohlen.Praefixe.Concat(fohlen.Suffixe))
            {
                var familie = inhalte.HoleMerkmal(merkmal.DefinitionId).Familie;
                familienTreffer[familie] = familienTreffer.GetValueOrDefault(familie) + 1;
            }
        }

        foreach (var stufe in Enum.GetValues<Seltenheit>())
            ergebnis.SeltenheitsChancen[stufe] = seltenheitTreffer.GetValueOrDefault(stufe) / (double)stichproben;

        foreach (var (familie, anzahl) in familienTreffer)
            ergebnis.FamilienChancen[familie] = anzahl / (double)stichproben;

        return ergebnis;
    }
}
