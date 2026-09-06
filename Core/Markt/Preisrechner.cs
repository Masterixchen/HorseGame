namespace Core;

/// <summary>Rechnet Kauf- und Verkaufspreise für Pferde aus. Verkauf bringt bewusst weniger als
/// Kauf - sonst ließe sich durch Hin-und-her-Handeln Geld aus dem Nichts erzeugen.</summary>
public static class Preisrechner
{
    public static int Kaufpreis(Pferd pferd)
    {
        int basis = 80 + pferd.Blutlinienstufe * 40;
        int seltenheitsBonus = (int)pferd.Seltenheit * 200;
        int merkmalsBonus = (pferd.Praefixe.Sum(p => p.Stufe) + pferd.Suffixe.Sum(s => s.Stufe)) * 25;
        return basis + seltenheitsBonus + merkmalsBonus;
    }

    public static int Verkaufspreis(Pferd pferd) => (int)(Kaufpreis(pferd) * 0.5);
}
