namespace Core;

/// <summary>Der komplette Zucht-Algorithmus: aus zwei Elterntieren und den eingelegten Materialien
/// wird ein Fohlen gewürfelt. Diese Klasse ist absichtlich die EINZIGE Stelle, die das tut - sowohl
/// die tatsächliche Zucht als auch die Wahrscheinlichkeitsvorschau (ZuchtVorschau) rufen genau
/// diesen Code auf. Nur so kann die Vorschau ehrlich sein, statt eine zweite, separat gepflegte
/// Formel zu zeigen, die von der echten Regel abweichen könnte.
///
/// gebaeudestufen kommt von außen (Spielstand kennt seine Gebäude, ZuchtRechner bleibt eine reine
/// Funktion) und bestimmt über GebaeudeRegeln, was Zuchtstall, Fohlenaufzucht und Genetiklabor
/// gerade zulassen (siehe Phase-4-Auftrag, Block 2).</summary>
public static class ZuchtRechner
{
    // Erste Entwürfe, zum Feintunen mit dem Sim-Werkzeug gedacht (siehe Projektanweisung).
    private const double BasisErbchance = 0.35;
    private const double TieraufstiegChance = 0.20;
    private const int MaxMerkmaleProAchse = 3;

    /// <summary>Würfelt das Fohlen sofort und verpackt es in eine Trächtigkeit, die erst nach der
    /// Tragezeit auf die Stallliste kommt. Gewürfelt wird bei der Zucht, nicht erst bei der Geburt.</summary>
    public static Traechtigkeit StarteZucht(GameRandom zufall, Inhaltsdatenbank inhalte, Pferd mutter, Pferd vater,
        ZuchtEinsatz einsatz, IReadOnlyDictionary<Gebaeude, int> gebaeudestufen, DateTime jetzt)
    {
        var fohlen = ErzeugeFohlen(zufall, inhalte, mutter, vater, einsatz, gebaeudestufen, jetzt);
        var dauer = GebaeudeRegeln.Traechtigkeitsdauer(gebaeudestufen.GetValueOrDefault(Gebaeude.Zuchtstall, 1));
        return new Traechtigkeit { Beginn = jetzt, Geburtstermin = jetzt + dauer, Fohlen = fohlen };
    }

    public static Pferd ErzeugeFohlen(GameRandom zufall, Inhaltsdatenbank inhalte, Pferd mutter, Pferd vater,
        ZuchtEinsatz einsatz, IReadOnlyDictionary<Gebaeude, int> gebaeudestufen, DateTime geburtsdatum)
    {
        var rasse = inhalte.HoleRasse(zufall.NaechsterBool() ? mutter.RasseId : vater.RasseId);
        int zuchtstallStufe = gebaeudestufen.GetValueOrDefault(Gebaeude.Zuchtstall, 1);

        var fohlen = new Pferd
        {
            RasseId = rasse.Id,
            Geschlecht = zufall.NaechsterBool() ? Geschlecht.Stute : Geschlecht.Hengst,
            Farbe = zufall.NaechsterBool() ? mutter.Farbe : vater.Farbe,
            Geburtsdatum = geburtsdatum,
            Blutlinienstufe = WuerfleBlutlinienstufe(zufall, mutter, vater, einsatz, zuchtstallStufe),
            MutterId = mutter.Id,
            VaterId = vater.Id
        };
        if (!rasse.Farben.Contains(fohlen.Farbe)) fohlen.Farbe = zufall.Waehle(rasse.Farben);
        fohlen.Name = zufall.Waehle(inhalte.NamenFuer(fohlen.Geschlecht));

        fohlen.Praefixe = SammleMerkmale(zufall, inhalte, mutter, vater, einsatz, MerkmalsArt.Praefix, rasse.MoeglichePraefixe, fohlen.Blutlinienstufe);
        fohlen.Suffixe = SammleMerkmale(zufall, inhalte, mutter, vater, einsatz, MerkmalsArt.Suffix, rasse.MoeglicheSuffixe, fohlen.Blutlinienstufe);
        fohlen.Seltenheit = SeltenheitRegeln.AusMerkmalsanzahl(fohlen.Praefixe.Count, fohlen.Suffixe.Count);

        var implizit = inhalte.HoleMerkmal(rasse.ImplizitesMerkmal);
        fohlen.ImplizitesMerkmal = MerkmalsWuerfler.Wuerfle(zufall, implizit, BlutlinienRegeln.MaxMerkmalsstufe(fohlen.Blutlinienstufe, implizit.Stufen.Count));

        int genetiklaborStufe = gebaeudestufen.GetValueOrDefault(Gebaeude.Genetiklabor, 1);
        VerbirgMerkmale(fohlen, zufall, geburtsdatum, genetiklaborStufe);

        int fohlenaufzuchtStufe = gebaeudestufen.GetValueOrDefault(Gebaeude.Fohlenaufzucht, 1);
        fohlen.Werte = WuerfleWerte(zufall, rasse, mutter, vater, fohlen, fohlenaufzuchtStufe);
        fohlen.Kondition = 100f;
        fohlen.Stimmung = zufall.NaechsterBereich(70f, 100f);

        return fohlen;
    }

    private static int WuerfleBlutlinienstufe(GameRandom zufall, Pferd mutter, Pferd vater, ZuchtEinsatz einsatz, int zuchtstallStufe)
    {
        int basis = (int)Math.Round((mutter.Blutlinienstufe + vater.Blutlinienstufe) / 2.0, MidpointRounding.AwayFromZero);
        if (einsatz.Kraftfutter) basis += 1;
        int schwankung = zufall.NaechsteGanzzahl(-1, 2); // -1, 0 oder +1 - "ergibt sich", ist kein reiner Wurf

        // Ohne ausgebauten Zuchtstall ist eine Spitzen-Blutlinie schlicht unerreichbar, egal wie
        // gut die Eltern sind (siehe Phase-4-Auftrag, Block 2).
        int deckel = GebaeudeRegeln.MaxBlutlinienstufe(zuchtstallStufe);
        return Math.Clamp(basis + schwankung, 1, deckel);
    }

    private static List<MerkmalsInstanz> SammleMerkmale(GameRandom zufall, Inhaltsdatenbank inhalte, Pferd mutter, Pferd vater,
        ZuchtEinsatz einsatz, MerkmalsArt art, List<string> rassenPool, int blutlinienstufeFohlen)
    {
        var ergebnis = new List<MerkmalsInstanz>();
        var genutzteIds = new HashSet<string>();

        int MaxStufeFuer(int anzahlDefinierterStufen) =>
            einsatz.SelteneLinie ? anzahlDefinierterStufen : BlutlinienRegeln.MaxMerkmalsstufe(blutlinienstufeFohlen, anzahlDefinierterStufen);

        void FuegeHinzuWennPlatz(MerkmalsInstanz instanz)
        {
            if (ergebnis.Count < MaxMerkmaleProAchse && genutzteIds.Add(instanz.DefinitionId))
                ergebnis.Add(instanz);
        }

        // 1) Vererbung: jedes passende Merkmal jedes Elternteils hat eine Chance, weiterzugeben.
        foreach (var elternteil in new[] { mutter, vater })
        {
            var elternMerkmale = art == MerkmalsArt.Praefix ? elternteil.Praefixe : elternteil.Suffixe;
            double erbchance = BasisErbchance + elternteil.ModifikatorSumme(Merkmalsattribut.Vererbungsstaerke) / 100.0;
            if (einsatz.Wagnis) erbchance *= 1.5;
            erbchance = Math.Clamp(erbchance, 0, 0.95);

            foreach (var elternMerkmal in elternMerkmale)
            {
                if (einsatz.Wagnis && zufall.NaechsterBool(0.2)) continue; // Wagnis: Pech trotz Erbanlage
                if (!zufall.NaechsterBool(erbchance)) continue;

                var definition = inhalte.HoleMerkmal(elternMerkmal.DefinitionId);
                int maxStufeFohlen = MaxStufeFuer(definition.Stufen.Count);
                int zielStufe = Math.Min(elternMerkmal.Stufe, maxStufeFohlen);
                if (zielStufe < maxStufeFohlen && zufall.NaechsterBool(TieraufstiegChance)) zielStufe++;

                FuegeHinzuWennPlatz(MerkmalsWuerfler.WuerfleAufStufe(zufall, definition, zielStufe));
            }
        }

        // 2) Auch ohne Vererbung kann ein ganz neues Merkmal entstehen - sonst bliebe eine Linie aus
        // merkmalslosen Eltern für immer merkmalslos. Wagnis erhöht diese Chance deutlich.
        double neuesMerkmalChance = einsatz.Wagnis ? 0.4 : 0.15;
        if (rassenPool.Count > 0 && zufall.NaechsterBool(neuesMerkmalChance))
        {
            var definition = inhalte.HoleMerkmal(zufall.Waehle(rassenPool));
            FuegeHinzuWennPlatz(MerkmalsWuerfler.Wuerfle(zufall, definition, MaxStufeFuer(definition.Stufen.Count)));
        }

        // 3) Garantien aus Materialien - laufen ins Leere, wenn die Vererbung die Achse schon füllt.
        if (einsatz.Ahnentafel && ergebnis.Count == 0 && rassenPool.Count > 0)
        {
            var definition = inhalte.HoleMerkmal(zufall.Waehle(rassenPool));
            FuegeHinzuWennPlatz(MerkmalsWuerfler.Wuerfle(zufall, definition, MaxStufeFuer(definition.Stufen.Count)));
        }

        if (einsatz.FremdblutGenutzt)
        {
            var passende = inhalte.AlleMerkmale().Where(m => m.Art == art && m.Familie == einsatz.FremdblutFamilie).ToList();
            if (passende.Count > 0)
            {
                var definition = zufall.Waehle(passende);
                FuegeHinzuWennPlatz(MerkmalsWuerfler.Wuerfle(zufall, definition, MaxStufeFuer(definition.Stufen.Count)));
            }
        }

        if (einsatz.SpezialistenbetreuungGenutzt)
        {
            var definition = inhalte.HoleMerkmal(einsatz.SpezialistenMerkmalId!);
            if (definition.Art == art)
                FuegeHinzuWennPlatz(MerkmalsWuerfler.WuerfleAufStufe(zufall, definition, 1)); // bewusst niedrige Stufe
        }

        return ergebnis;
    }

    private static void VerbirgMerkmale(Pferd fohlen, GameRandom zufall, DateTime geburtsdatum, int genetiklaborStufe)
    {
        // "Nach und nach": jedes Merkmal deckt sich einzeln zu einem eigenen, gestreuten Zeitpunkt
        // auf. Ein ausgebautes Genetiklabor verkürzt diesen Zeitraum (siehe GebaeudeRegeln).
        var (minStunden, maxStunden) = GebaeudeRegeln.AufdeckZeitraum(genetiklaborStufe);
        foreach (var merkmal in fohlen.AlleMerkmale())
        {
            merkmal.Bekannt = false;
            merkmal.Aufdeckzeitpunkt = geburtsdatum.AddHours(zufall.NaechsterBereich(minStunden, maxStunden));
        }
    }

    private static StatBlock WuerfleWerte(GameRandom zufall, Rasse rasse, Pferd mutter, Pferd vater, Pferd fohlen, int fohlenaufzuchtStufe)
    {
        var werte = new StatBlock();
        float minStreuung = GebaeudeRegeln.MinWerteStreuung(fohlenaufzuchtStufe);

        foreach (var (typ, stat) in werte.Alle())
        {
            // Der Kernhebel, der Zucht lohnender macht als Zukauf: die Basis ist der Durchschnitt
            // der Elternpotenziale statt immer wieder der reine Rassen-Basisbereich. Eine gut
            // ausgebaute Fohlenaufzucht hebt die Untergrenze dieser Streuung an - ein gut
            // aufgezogenes Fohlen verliert nichts von der Anlage seiner Eltern.
            float elternDurchschnitt = (mutter.Werte.Hole(typ).Potenzial + vater.Werte.Hole(typ).Potenzial) / 2f;
            float basis = elternDurchschnitt * zufall.NaechsterBereich(minStreuung, 1.1f);
            basis = Math.Max(basis, rasse.Potenzial(typ).Min);

            float bonusProzent = fohlen.ModifikatorSumme(typ.AlsMerkmalsattribut());
            stat.Potenzial = basis * (1f + bonusProzent / 100f);
            stat.Aktuell = stat.Potenzial * 0.2f; // Fohlen starten weiter unten als ein junges Marktpferd
        }
        return werte;
    }
}
