using Core;

namespace Sim;

/// <summary>Belegt das "Fertig, wenn" aus der Projektanweisung für Phase 3: die Balance soll über
/// mehrere Simulationsjahre stabil sein. Lässt eine einfache Strategie laufen (trainieren, bei
/// Gelegenheit antreten, bei Überschuss den Hof ausbauen, gelegentlich zukaufen) und druckt pro
/// Jahr Kontostand, Ansehen, Bestandsgröße, Materialvorräte und das beste Pferd.</summary>
public static class Wirtschaftsbericht
{
    private const int Jahre = 5;
    private const int MaxBestand = 15;

    public static void Drucken(Inhaltsdatenbank inhalte)
    {
        var stand = SpielstandFabrik.NeuesSpiel(inhalte, new DateTime(2026, 1, 1), seed: 42);
        var klassen = inhalte.AlleWettkampfklassen().ToList();

        Console.WriteLine("Wirtschaftssimulation über mehrere Jahre (Strategie: trainieren, antreten, bei Überschuss ausbauen):");
        Console.WriteLine($"{"Jahr",-5}{"Guthaben",10}{"Ansehen",9}{"Pferde",8}{"Material",10}  Bestes Pferd");

        var start = stand.ZuletztAktualisiert;
        for (int jahr = 1; jahr <= Jahre; jahr++)
        {
            var jahresende = start.AddYears(jahr);
            var tag = stand.ZuletztAktualisiert;
            while (tag < jahresende)
            {
                tag = tag.AddDays(1);
                stand.Advance(tag);
                stand.AktualisiereMarktFallsFaellig(inhalte, tag);
                SimuliereEinenTag(stand, klassen, tag);
            }

            DruckeJahresbericht(jahr, stand);
        }

        Console.WriteLine();
        Console.WriteLine(stand.Guthaben > -1000 && stand.Pferde.Count > 0
            ? "-> Die Wirtschaft bleibt über die simulierten Jahre stabil."
            : "-> Die Wirtschaft kippt (Guthaben tief im Minus oder Bestand ausgestorben) - Formeln prüfen.");
    }

    private static void SimuliereEinenTag(Spielstand stand, List<Wettkampfklasse> klassen, DateTime tag)
    {
        if (stand.Guthaben > stand.HofAusbauKosten() * 3)
            stand.HofAusbauen();

        if (stand.Pferde.Count < MaxBestand && stand.MarktPferde.Count > 0 && stand.Zufall.NaechsterBool(0.1))
        {
            var kandidat = stand.Zufall.Waehle(stand.MarktPferde);
            if (Preisrechner.Kaufpreis(kandidat) < stand.Guthaben / 2)
                stand.KaufeMarktpferd(kandidat);
        }

        foreach (var pferd in stand.Pferde.ToList())
        {
            if (pferd.IstBeschaeftigt || !pferd.AlleMerkmaleBekannt) continue;

            var moeglicheKlassen = klassen.Where(k => WettkampfRechner.DarfTeilnehmen(k, pferd, stand.Ansehen, tag)).ToList();
            if (moeglicheKlassen.Count > 0 && stand.Zufall.NaechsterBool(0.5))
            {
                stand.MeldeAn(stand.Zufall.Waehle(moeglicheKlassen), pferd, tag);
            }
            else
            {
                var ziel = pferd.Werte.Alle().OrderByDescending(w => w.Wert.Potenzial - w.Wert.Aktuell).First().Typ;
                pferd.StarteTraining(ziel, tag, stand.Zufall);
            }
        }
    }

    private static void DruckeJahresbericht(int jahr, Spielstand stand)
    {
        var bestesPferd = stand.Pferde.OrderByDescending(p => p.Werte.Alle().Sum(w => w.Wert.Potenzial)).First();
        int materialSumme = stand.Materialbestand.Values.Sum();

        Console.WriteLine($"{jahr,-5}{stand.Guthaben,10:N0}{stand.Ansehen,9:N0}{stand.Pferde.Count,8}{materialSumme,10}  " +
                           $"{bestesPferd.Name} ({bestesPferd.Seltenheit}, Blutlinie {bestesPferd.Blutlinienstufe})");
    }
}
