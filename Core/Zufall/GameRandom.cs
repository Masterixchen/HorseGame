namespace Core;

/// <summary>
/// Die einzige Zufallsquelle im Spiel. Nutzt einen eigenen, einfachen Algorithmus (xorshift64*)
/// statt System.Random, weil sich dessen interner Zustand nicht speichern und wiederherstellen
/// lässt. Mit gespeichertem Zustand kann ein Spielverlauf exakt nachgestellt werden.
/// </summary>
public class GameRandom
{
    public ulong Seed { get; set; }
    public ulong Zustand { get; set; }

    public GameRandom() : this((ulong)DateTime.UtcNow.Ticks)
    {
    }

    public GameRandom(ulong seed)
    {
        Seed = seed;
        Zustand = seed == 0 ? 0x9E3779B97F4A7C15 : seed;
    }

    private ulong NaechsterRohwert()
    {
        // xorshift64* - schnell und für Spielzwecke gut genug, kein Kryptoanspruch.
        Zustand ^= Zustand >> 12;
        Zustand ^= Zustand << 25;
        Zustand ^= Zustand >> 27;
        return Zustand * 0x2545F4914F6CDD1DUL;
    }

    /// <summary>Gleichverteilte Kommazahl in [0, 1).</summary>
    public double NaechsteZahl() => (NaechsterRohwert() >> 11) * (1.0 / (1UL << 53));

    /// <summary>Ganzzahl in [minInklusiv, maxExklusiv).</summary>
    public int NaechsteGanzzahl(int minInklusiv, int maxExklusiv)
    {
        if (maxExklusiv <= minInklusiv) return minInklusiv;
        return minInklusiv + (int)(NaechsteZahl() * (maxExklusiv - minInklusiv));
    }

    /// <summary>Kommazahl im Bereich [min, max].</summary>
    public float NaechsterBereich(float min, float max) => min + (float)(NaechsteZahl() * (max - min));

    public bool NaechsterBool(double wahrscheinlichkeit = 0.5) => NaechsteZahl() < wahrscheinlichkeit;

    /// <summary>Wählt ein zufälliges Element aus einer Liste.</summary>
    public T Waehle<T>(IReadOnlyList<T> liste) => liste[NaechsteGanzzahl(0, liste.Count)];
}
