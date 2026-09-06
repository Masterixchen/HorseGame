using Core;

namespace Sim;

/// <summary>Belegt das "Fertig, wenn" aus der Projektanweisung für Phase 3: die Balance soll über
/// mehrere Simulationsmonate stabil sein. Seit dem Phase-4-Umbau (Warteschlangen statt Einzeltermine,
/// Wettkämpfe im Minuten-/Stunden-Takt) reicht ein täglicher Check-in nicht mehr als Zeitraster -
/// die Strategie schaut alle drei simulierten Stunden vorbei, füllt leere Warteschlangen auf und
/// meldet gleich für mehrere kommende Termine an, ähnlich wie ein Spieler, der mehrmals am Tag
/// kurz reinschaut. Druckt pro Monat Kontostand, Ansehen, Bestandsgröße, Materialvorräte und das
/// beste Pferd.</summary>
public static class Wirtschaftsbericht
{
    private const int Monate = 6;
    private const int MaxBestand = 15;
    private static readonly TimeSpan Checkintervall = TimeSpan.FromHours(3);
    private const int AnzahlTermineJeAnmeldung = 3;
    private const int WarteschlangenAuffuellung = 5;

    public static void Drucken(Inhaltsdatenbank inhalte)
    {
        var stand = SpielstandFabrik.NeuesSpiel(inhalte, new DateTime(2026, 1, 1), seed: 42);
        var klassen = inhalte.AlleWettkampfklassen().ToList();

        Console.WriteLine("Wirtschaftssimulation über mehrere Monate (Check-in alle 3 Std.: Warteschlangen füllen, antreten, bei Überschuss ausbauen):");
        Console.WriteLine($"{"Monat",-6}{"Guthaben",10}{"Ansehen",9}{"Pferde",8}{"Material",10}  Bestes Pferd");

        var start = stand.ZuletztAktualisiert;
        for (int monat = 1; monat <= Monate; monat++)
        {
            var monatsende = start.AddMonths(monat);
            var zeitpunkt = stand.ZuletztAktualisiert;
            while (zeitpunkt < monatsende)
            {
                zeitpunkt += Checkintervall;
                if (zeitpunkt > monatsende) zeitpunkt = monatsende;

                stand.Advance(zeitpunkt);
                stand.AktualisiereMarktFallsFaellig(inhalte, zeitpunkt);
                CheckIn(stand, klassen, zeitpunkt);
            }

            DruckeMonatsbericht(monat, stand);
        }

        Console.WriteLine();
        Console.WriteLine(stand.Guthaben > -1000 && stand.Pferde.Count > 0
            ? "-> Die Wirtschaft bleibt über die simulierten Monate stabil."
            : "-> Die Wirtschaft kippt (Guthaben tief im Minus oder Bestand ausgestorben) - Formeln prüfen.");
    }

    private static void CheckIn(Spielstand stand, List<Wettkampfklasse> klassen, DateTime jetzt)
    {
        if (stand.HofAusbauFertig == null && stand.Guthaben > stand.HofAusbauKosten() * 3)
            stand.HofAusbauen(jetzt);

        if (stand.Pferde.Count < MaxBestand && stand.MarktPferde.Count > 0 && stand.Zufall.NaechsterBool(0.1))
        {
            var kandidat = stand.Zufall.Waehle(stand.MarktPferde);
            if (Preisrechner.Kaufpreis(kandidat) < stand.Guthaben / 2)
                stand.KaufeMarktpferd(kandidat);
        }

        foreach (var pferd in stand.Pferde.ToList())
        {
            if (pferd.IstBeschaeftigt || !pferd.AlleMerkmaleBekannt) continue;

            if (pferd.Anmeldungen.Count == 0)
            {
                var moeglicheKlassen = klassen.Where(k => WettkampfRechner.DarfTeilnehmen(k, pferd, stand.Ansehen, jetzt)).ToList();
                if (moeglicheKlassen.Count > 0 && stand.Zufall.NaechsterBool(0.5))
                    stand.MeldeAn(stand.Zufall.Waehle(moeglicheKlassen), pferd, jetzt, AnzahlTermineJeAnmeldung);
            }

            if (pferd.Warteschlange.Count == 0 && pferd.LaufendesTraining == null)
            {
                var ziel = pferd.Werte.Alle().OrderByDescending(w => w.Wert.Potenzial - w.Wert.Aktuell).First().Typ;
                for (int i = 0; i < WarteschlangenAuffuellung; i++)
                    pferd.TrainingEinreihen(ziel, Intensitaet.Mittel);
            }
        }
    }

    private static void DruckeMonatsbericht(int monat, Spielstand stand)
    {
        var bestesPferd = stand.Pferde.OrderByDescending(p => p.Werte.Alle().Sum(w => w.Wert.Potenzial)).First();
        int materialSumme = stand.Materialbestand.Values.Sum();

        Console.WriteLine($"{monat,-6}{stand.Guthaben,10:N0}{stand.Ansehen,9:N0}{stand.Pferde.Count,8}{materialSumme,10}  " +
                           $"{bestesPferd.Name} ({bestesPferd.Seltenheit}, Blutlinie {bestesPferd.Blutlinienstufe})");
    }
}
