using Core;

namespace Tests;

public class TrainingUndAdvanceTests
{
    private static Pferd ErzeugeTestpferd()
    {
        var pferd = new Pferd { Geburtsdatum = new DateTime(2020, 1, 1) };
        pferd.Werte.Tempo.Potenzial = 100f;
        pferd.Werte.Tempo.Aktuell = 40f;
        return pferd;
    }

    [Fact]
    public void StarteTraining_SetztEndeZwischenEinerUndAchtStunden()
    {
        var pferd = ErzeugeTestpferd();
        var zufall = new GameRandom(10);
        var start = new DateTime(2026, 1, 1, 8, 0, 0);

        pferd.StarteTraining(StatTyp.Tempo, start, zufall);

        Assert.NotNull(pferd.AktivesTraining);
        var dauer = pferd.AktivesTraining!.Ende - start;
        Assert.InRange(dauer.TotalHours, 1, 8);
    }

    [Fact]
    public void StarteTraining_ZweitesTrainingWirftFehler()
    {
        var pferd = ErzeugeTestpferd();
        var zufall = new GameRandom(11);
        pferd.StarteTraining(StatTyp.Tempo, DateTime.UtcNow, zufall);

        Assert.Throws<InvalidOperationException>(() => pferd.StarteTraining(StatTyp.Tempo, DateTime.UtcNow, zufall));
    }

    [Fact]
    public void Advance_SchliesstFaelligesTrainingAbUndNaehertSichPotenzialAn()
    {
        var pferd = ErzeugeTestpferd();
        var start = new DateTime(2026, 1, 1, 8, 0, 0);
        pferd.AktivesTraining = new Training { Ziel = StatTyp.Tempo, Start = start, Ende = start.AddHours(5) };

        // Spielstand.Advance ruft in 1-Stunden-Schritten auf - auch bei langer Abwesenheit muss
        // das Training genau dann fertig werden, wenn "Ende" erreicht ist.
        var stand = new Spielstand { ZuletztAktualisiert = start, Pferde = { pferd } };
        stand.Advance(start.AddDays(3));

        Assert.Null(pferd.AktivesTraining);
        Assert.True(pferd.Werte.Tempo.Aktuell > 40f);
        Assert.True(pferd.Werte.Tempo.Aktuell <= pferd.Werte.Tempo.Potenzial);
    }

    [Fact]
    public void AbnehmenderErtrag_ZweiterZuwachsIstKleinerAlsErster()
    {
        var pferd = ErzeugeTestpferd();
        var jetzt = new DateTime(2026, 1, 1);

        float vorher1 = pferd.Werte.Tempo.Aktuell;
        pferd.AktivesTraining = new Training { Ziel = StatTyp.Tempo, Start = jetzt, Ende = jetzt.AddHours(4) };
        pferd.Advance(jetzt, jetzt.AddHours(4));
        float zuwachs1 = pferd.Werte.Tempo.Aktuell - vorher1;

        float vorher2 = pferd.Werte.Tempo.Aktuell;
        pferd.AktivesTraining = new Training { Ziel = StatTyp.Tempo, Start = jetzt.AddHours(4), Ende = jetzt.AddHours(8) };
        pferd.Advance(jetzt.AddHours(4), jetzt.AddHours(8));
        float zuwachs2 = pferd.Werte.Tempo.Aktuell - vorher2;

        Assert.True(zuwachs2 < zuwachs1);
    }

    [Fact]
    public void Advance_OhneTraining_ErholtKonditionUndStimmung()
    {
        var pferd = ErzeugeTestpferd();
        pferd.Kondition = 50f;
        pferd.Stimmung = 50f;
        var jetzt = new DateTime(2026, 1, 1);

        pferd.Advance(jetzt, jetzt.AddHours(2));

        Assert.True(pferd.Kondition > 50f);
        Assert.True(pferd.Stimmung > 50f);
    }

    [Fact]
    public void SpielstandAdvance_InDerVergangenheit_TutNichts()
    {
        var stand = new Spielstand { ZuletztAktualisiert = new DateTime(2026, 1, 10) };
        stand.Advance(new DateTime(2026, 1, 1));

        Assert.Equal(new DateTime(2026, 1, 10), stand.ZuletztAktualisiert);
    }
}
