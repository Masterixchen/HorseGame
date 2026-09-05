using Core;

namespace Tests;

public class GameRandomTests
{
    [Fact]
    public void GleicherStartwert_ErzeugtGleicheFolge()
    {
        var a = new GameRandom(1234);
        var b = new GameRandom(1234);

        for (int i = 0; i < 50; i++)
            Assert.Equal(a.NaechsteZahl(), b.NaechsteZahl());
    }

    [Fact]
    public void UnterschiedlicherStartwert_ErzeugtUnterschiedlicheFolge()
    {
        var a = new GameRandom(1);
        var b = new GameRandom(2);

        bool mindestensEinUnterschied = false;
        for (int i = 0; i < 20; i++)
            if (a.NaechsteZahl() != b.NaechsteZahl()) mindestensEinUnterschied = true;

        Assert.True(mindestensEinUnterschied);
    }

    [Fact]
    public void NaechsteGanzzahl_BleibtImBereich()
    {
        var zufall = new GameRandom(42);
        for (int i = 0; i < 1000; i++)
        {
            int wert = zufall.NaechsteGanzzahl(5, 10);
            Assert.InRange(wert, 5, 9);
        }
    }

    [Fact]
    public void ZustandLaesstSichSpeichernUndFortsetzen()
    {
        var original = new GameRandom(777);
        _ = original.NaechsteZahl();
        _ = original.NaechsteZahl();

        // Zustand "speichern" (wie beim Speichern des Spielstands) und in einer neuen Instanz fortsetzen.
        var fortgesetzt = new GameRandom(original.Seed) { Zustand = original.Zustand };

        Assert.Equal(original.NaechsteZahl(), fortgesetzt.NaechsteZahl());
    }
}
