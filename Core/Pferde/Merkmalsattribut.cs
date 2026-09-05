namespace Core;

/// <summary>Alles, worauf ein Merkmalseffekt wirken kann. Die ersten vier sind Kernwerte mit
/// Aktuell/Potenzial (siehe StatBlock) und werden trainiert. Die übrigen wirken nur als
/// Prozent-Modifikator (siehe Pferd.ModifikatorSumme) - für sie gibt es kein Training.</summary>
public enum Merkmalsattribut
{
    Tempo,
    Ausdauer,
    Sprungkraft,
    Rittigkeit,

    // Körperliche Anlage ohne eigenen Trainingswert.
    Wachstumsgeschwindigkeit,

    // Wesen und Gesundheit - Domäne der Suffixe.
    Trainingsgeschwindigkeit,
    Verletzungsanfaelligkeit,
    Erholung,
    Stimmung,
    Wettkampfverhalten,
    Futterbedarf,
    Vererbungsstaerke
}

/// <summary>Die vier Kernwerte, die trainiert werden können und in die Wettkampfformel eingehen.</summary>
public enum StatTyp
{
    Tempo,
    Ausdauer,
    Sprungkraft,
    Rittigkeit
}

public static class MerkmalsattributErweiterungen
{
    /// <summary>Liefert den passenden Kernwert, falls das Attribut einer ist - sonst null.</summary>
    public static StatTyp? AlsStatTyp(this Merkmalsattribut attribut) => attribut switch
    {
        Merkmalsattribut.Tempo => StatTyp.Tempo,
        Merkmalsattribut.Ausdauer => StatTyp.Ausdauer,
        Merkmalsattribut.Sprungkraft => StatTyp.Sprungkraft,
        Merkmalsattribut.Rittigkeit => StatTyp.Rittigkeit,
        _ => null
    };

    public static Merkmalsattribut AlsMerkmalsattribut(this StatTyp typ) => typ switch
    {
        StatTyp.Tempo => Merkmalsattribut.Tempo,
        StatTyp.Ausdauer => Merkmalsattribut.Ausdauer,
        StatTyp.Sprungkraft => Merkmalsattribut.Sprungkraft,
        StatTyp.Rittigkeit => Merkmalsattribut.Rittigkeit,
        _ => throw new ArgumentOutOfRangeException(nameof(typ))
    };
}
