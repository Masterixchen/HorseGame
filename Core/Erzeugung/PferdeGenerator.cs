namespace Core;

/// <summary>Erzeugt zufällige Pferde nach den Regeln aus der Projektanweisung. Einzigartige Pferde
/// entstehen hier nicht - die sind fest definiert und kommen in Phase 4.</summary>
public static class PferdeGenerator
{
    public static Pferd Erzeuge(GameRandom zufall, Inhaltsdatenbank inhalte, Rasse rasse, DateTime jetzt, Geschlecht? geschlecht = null)
    {
        var pferd = new Pferd
        {
            RasseId = rasse.Id,
            Geschlecht = geschlecht ?? (zufall.NaechsterBool() ? Geschlecht.Stute : Geschlecht.Hengst),
            Farbe = zufall.Waehle(rasse.Farben),
            Geburtsdatum = jetzt,
            Seltenheit = SeltenheitRegeln.Wuerfle(zufall),
            // Blutlinienstufe für zufällig erzeugte Pferde (Marktangebot, Startbestand): niedrig
            // und eng gestreut. Höhere, gezielt gezüchtete Blutlinien sind Sache von Phase 2.
            Blutlinienstufe = zufall.NaechsteGanzzahl(1, 4)
        };

        pferd.Name = zufall.Waehle(inhalte.NamenFuer(pferd.Geschlecht));

        var praefixIds = WaehleMerkmale(zufall, rasse.MoeglichePraefixe, SeltenheitRegeln.MaxPraefixe(pferd.Seltenheit));
        var suffixIds = WaehleMerkmale(zufall, rasse.MoeglicheSuffixe, SeltenheitRegeln.MaxSuffixe(pferd.Seltenheit));

        pferd.ImplizitesMerkmal = WuerfleMerkmal(zufall, inhalte.HoleMerkmal(rasse.ImplizitesMerkmal), pferd.Blutlinienstufe);
        pferd.Praefixe = praefixIds.Select(id => WuerfleMerkmal(zufall, inhalte.HoleMerkmal(id), pferd.Blutlinienstufe)).ToList();
        pferd.Suffixe = suffixIds.Select(id => WuerfleMerkmal(zufall, inhalte.HoleMerkmal(id), pferd.Blutlinienstufe)).ToList();

        pferd.Werte = WuerfleWerte(zufall, rasse, pferd);
        pferd.Kondition = 100f;
        pferd.Stimmung = zufall.NaechsterBereich(70f, 100f);

        return pferd;
    }

    private static List<string> WaehleMerkmale(GameRandom zufall, List<string> pool, int maxAnzahl)
    {
        var ergebnis = new List<string>();
        if (maxAnzahl <= 0 || pool.Count == 0) return ergebnis;

        int anzahl = zufall.NaechsteGanzzahl(0, maxAnzahl + 1);
        var verbleibend = new List<string>(pool);
        for (int i = 0; i < anzahl && verbleibend.Count > 0; i++)
        {
            var gewaehlt = zufall.Waehle(verbleibend);
            ergebnis.Add(gewaehlt);
            verbleibend.Remove(gewaehlt);
        }
        return ergebnis;
    }

    private static MerkmalsInstanz WuerfleMerkmal(GameRandom zufall, MerkmalsDefinition definition, int blutlinienstufe)
    {
        int maxStufe = BlutlinienRegeln.MaxMerkmalsstufe(blutlinienstufe, definition.Stufen.Count);
        return MerkmalsWuerfler.Wuerfle(zufall, definition, maxStufe);
    }

    private static StatBlock WuerfleWerte(GameRandom zufall, Rasse rasse, Pferd pferd)
    {
        var werte = new StatBlock();
        foreach (var (typ, stat) in werte.Alle())
        {
            var bereich = rasse.Potenzial(typ);
            float basis = zufall.NaechsterBereich(bereich.Min, bereich.Max);
            float bonusProzent = pferd.ModifikatorSumme(typ.AlsMerkmalsattribut());

            stat.Potenzial = basis * (1f + bonusProzent / 100f);
            // Ein neues Pferd ist untrainiert: Startwert bei einem Drittel des Potenzials.
            stat.Aktuell = stat.Potenzial * 0.33f;
        }
        return werte;
    }
}
