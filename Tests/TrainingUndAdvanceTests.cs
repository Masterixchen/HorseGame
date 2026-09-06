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
    public void TrainingEinreihen_FuegtWarteschlangeHinzu()
    {
        var pferd = ErzeugeTestpferd();
        pferd.TrainingEinreihen(StatTyp.Tempo, Intensitaet.Mittel);

        Assert.Single(pferd.Warteschlange);
        Assert.Null(pferd.LaufendesTraining);
    }

    [Fact]
    public void Advance_ArbeitetWarteschlangeAbUndNaehertSichPotenzialAn()
    {
        var pferd = ErzeugeTestpferd();
        pferd.TrainingEinreihen(StatTyp.Tempo, Intensitaet.Mittel);
        var start = new DateTime(2026, 1, 1, 8, 0, 0);

        // Ein Aufruf über einen langen Zeitraum muss die Einheit vollständig abarbeiten, auch bei
        // langer Abwesenheit - genau wie beim alten Zeitmodell, nur jetzt in Minuten statt Stunden.
        pferd.Advance(start, start.AddDays(1));

        Assert.Empty(pferd.Warteschlange);
        Assert.Null(pferd.LaufendesTraining);
        Assert.True(pferd.Werte.Tempo.Aktuell > 40f);
        Assert.True(pferd.Werte.Tempo.Aktuell <= pferd.Werte.Tempo.Potenzial);
    }

    [Fact]
    public void Advance_MehrereEinheitenLaufenInEinemAufrufNacheinanderAb()
    {
        var pferd = ErzeugeTestpferd();
        pferd.Kondition = 100f;
        for (int i = 0; i < 3; i++) pferd.TrainingEinreihen(StatTyp.Tempo, Intensitaet.Leicht);
        var start = new DateTime(2026, 1, 1, 8, 0, 0);

        // Drei leichte Einheiten (je 5 Min, 8 Kondition) passen locker in eine Stunde.
        pferd.Advance(start, start.AddHours(1));

        Assert.Empty(pferd.Warteschlange);
        Assert.Null(pferd.LaufendesTraining);
    }

    [Fact]
    public void Advance_PausiertWarteschlangeBeiZuWenigKondition()
    {
        var pferd = ErzeugeTestpferd();
        pferd.Kondition = 0f; // reicht für keine einzige Intensität
        pferd.TrainingEinreihen(StatTyp.Tempo, Intensitaet.Leicht); // kostet 8, braucht bei 25/h ca. 19 Minuten
        var start = new DateTime(2026, 1, 1, 8, 0, 0);

        pferd.Advance(start, start.AddMinutes(10));

        // Zu kurzes Fenster, um auf 8 Kondition zu regenerieren - der Auftrag bleibt liegen, es
        // wird nur passiv weiter erholt.
        Assert.Single(pferd.Warteschlange);
        Assert.Null(pferd.LaufendesTraining);
        Assert.True(pferd.Kondition > 0f);
    }

    [Fact]
    public void Advance_StartetEinheitSobaldGenugKonditionRegeneriertIst()
    {
        var pferd = ErzeugeTestpferd();
        pferd.Kondition = 0f;
        pferd.TrainingEinreihen(StatTyp.Tempo, Intensitaet.Leicht); // kostet 8, braucht bei 25/h ca. 19 Minuten
        var start = new DateTime(2026, 1, 1, 8, 0, 0);

        pferd.Advance(start, start.AddHours(1));

        Assert.Empty(pferd.Warteschlange);
        Assert.True(pferd.Werte.Tempo.Aktuell > 40f);
    }

    [Fact]
    public void AbnehmenderErtrag_ZweiterZuwachsIstKleinerAlsErster()
    {
        var pferd = ErzeugeTestpferd();
        pferd.Kondition = 100f;
        var jetzt = new DateTime(2026, 1, 1);

        float vorher1 = pferd.Werte.Tempo.Aktuell;
        pferd.TrainingEinreihen(StatTyp.Tempo, Intensitaet.Mittel);
        pferd.Advance(jetzt, jetzt.AddMinutes(10));
        float zuwachs1 = pferd.Werte.Tempo.Aktuell - vorher1;

        float vorher2 = pferd.Werte.Tempo.Aktuell;
        pferd.TrainingEinreihen(StatTyp.Tempo, Intensitaet.Mittel);
        pferd.Advance(jetzt.AddMinutes(10), jetzt.AddMinutes(20));
        float zuwachs2 = pferd.Werte.Tempo.Aktuell - vorher2;

        Assert.True(zuwachs1 > 0);
        Assert.True(zuwachs2 < zuwachs1);
    }

    [Fact]
    public void Advance_OhneWarteschlange_ErholtKonditionUndStimmung()
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
    public void Advance_KonditionUeberschreitetNieHundert()
    {
        var pferd = ErzeugeTestpferd();
        pferd.Kondition = 95f;
        var jetzt = new DateTime(2026, 1, 1);

        pferd.Advance(jetzt, jetzt.AddDays(1));

        Assert.Equal(100f, pferd.Kondition);
    }

    [Fact]
    public void SpielstandAdvance_InDerVergangenheit_TutNichts()
    {
        var stand = new Spielstand { ZuletztAktualisiert = new DateTime(2026, 1, 10) };
        stand.Advance(new DateTime(2026, 1, 1));

        Assert.Equal(new DateTime(2026, 1, 10), stand.ZuletztAktualisiert);
    }
}
