namespace Core;

/// <summary>Berechnet Wettkampfwertungen nach der Formel aus der Projektanweisung und würfelt
/// Platzierung und Preise aus. Braucht bewusst keine Inhaltsdatenbank - die Wettkampfklasse steckt
/// bereits fertig in der Anmeldung (siehe WettkampfAnmeldung), Ausrüstungseffekte tragen die
/// Ausruestung-Instanzen selbst. So kann Spielstand.Advance(DateTime) das hier ohne Content-Zugriff
/// auswerten. Guthaben, Ansehen und Materialbestand bucht Spielstand - diese Klasse liefert nur
/// das Ergebnis.</summary>
public static class WettkampfRechner
{
    private const int AnzahlGegner = 6;

    /// <summary>Der nächste Termin dieser Klasse nach dem gegebenen Zeitpunkt - jeden Tag zur
    /// gleichen Uhrzeit.</summary>
    public static DateTime NaechsterZeitpunkt(Wettkampfklasse klasse, DateTime nach)
    {
        var kandidat = nach.Date.AddHours(klasse.Stundenzeitpunkt);
        if (kandidat <= nach) kandidat = kandidat.AddDays(1);
        return kandidat;
    }

    public static bool DarfTeilnehmen(Wettkampfklasse klasse, Pferd pferd, int ansehen, DateTime zeitpunkt)
    {
        if (ansehen < klasse.BenoetigtesAnsehen) return false;
        int alter = pferd.AlterInJahren(zeitpunkt);
        if (klasse.MinAlter is int minAlter && alter < minAlter) return false;
        if (klasse.MaxAlter is int maxAlter && alter > maxAlter) return false;
        return true;
    }

    public static float BerechneWertung(GameRandom zufall, Pferd pferd, Wettkampfklasse klasse, IEnumerable<Ausruestung> ausruestungen, DateTime zeitpunkt)
    {
        float summe = 0f;
        foreach (var (typ, stat) in pferd.Werte.Alle())
        {
            float ausruestungsBonus = AusruestungsHelfer.ModifikatorSumme(pferd, ausruestungen, typ.AlsMerkmalsattribut());
            summe += stat.Aktuell * (1f + ausruestungsBonus / 100f) * klasse.Gewicht(typ);
        }

        float altersfaktor = Altersfaktor(pferd.AlterInJahren(zeitpunkt));
        float konditionsfaktor = 0.62f + 0.38f * (pferd.Kondition / 100f);
        float stimmungsfaktor = 0.82f + 0.30f * (pferd.Stimmung / 100f);
        float merkmalsmodifikator = 1f + pferd.ModifikatorSumme(Merkmalsattribut.Wettkampfverhalten) / 100f;
        float ausruestungsmodifikator = 1f + AusruestungsHelfer.ModifikatorSumme(pferd, ausruestungen, Merkmalsattribut.Wettkampfverhalten) / 100f;
        float zufallsfaktor = zufall.NaechsterBereich(0.85f, 1.15f);

        return summe * altersfaktor * konditionsfaktor * stimmungsfaktor * merkmalsmodifikator * ausruestungsmodifikator * zufallsfaktor;
    }

    // Erster Entwurf, zum Feintunen mit dem Sim-Werkzeug gedacht: junge und sehr alte Pferde
    // laufen unter ihren Werten, die beste Zeit liegt zwischen fünf und zwölf Jahren.
    private static float Altersfaktor(int alter) => alter switch
    {
        < 3 => 0.5f,
        < 5 => 0.85f,
        <= 12 => 1.0f,
        <= 18 => 0.85f,
        _ => 0.65f
    };

    /// <summary>Lässt das Pferd gegen gewürfelte Gegner antreten, die um den stufenabhängigen
    /// Basiswert der Klasse gestreut sind, und würfelt Platzierung, Preisgeld und Materialbonus aus.</summary>
    public static WettkampfErgebnis Werte(GameRandom zufall, Pferd pferd, Wettkampfklasse klasse, IEnumerable<Ausruestung> ausruestungen, DateTime zeitpunkt)
    {
        float eigeneWertung = BerechneWertung(zufall, pferd, klasse, ausruestungen, zeitpunkt);

        int platzierung = 1;
        for (int i = 0; i < AnzahlGegner; i++)
        {
            float gegnerWertung = klasse.GegnerBasiswert * zufall.NaechsterBereich(0.8f, 1.2f);
            if (gegnerWertung > eigeneWertung) platzierung++;
        }

        var ergebnis = new WettkampfErgebnis
        {
            PferdId = pferd.Id,
            PferdName = pferd.Name,
            WettkampfklasseId = klasse.Id,
            Zeitpunkt = zeitpunkt,
            Platzierung = platzierung,
            AnsehenGewonnen = klasse.AnsehenTeilnahme
        };

        if (platzierung == 1) { ergebnis.Preisgeld = klasse.Preisgeld1; ergebnis.AnsehenGewonnen += klasse.AnsehenErster; }
        else if (platzierung == 2) ergebnis.Preisgeld = klasse.Preisgeld2;
        else if (platzierung == 3) ergebnis.Preisgeld = klasse.Preisgeld3;

        if (platzierung <= 6) ergebnis.Preisgeld += klasse.Antrittsgeld;

        double materialChance = platzierung switch { 1 => 0.30, 2 => 0.20, 3 => 0.10, _ => 0.0 };
        if (zufall.NaechsterBool(materialChance))
            ergebnis.GewonnenesMaterial = zufall.Waehle(Enum.GetValues<MaterialTyp>());

        return ergebnis;
    }
}
