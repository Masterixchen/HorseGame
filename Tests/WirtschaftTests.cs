using Core;

namespace Tests;

public class WirtschaftTests
{
    private readonly Inhaltsdatenbank _inhalte = new();

    private Spielstand NeuesSpiel(DateTime jetzt, ulong seed = 1) => SpielstandFabrik.NeuesSpiel(_inhalte, jetzt, seed);

    [Fact]
    public void NeuesSpiel_HatStartguthabenPferdeUndMarkt()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));

        Assert.True(stand.Guthaben > 0);
        Assert.NotEmpty(stand.Pferde);
        Assert.NotEmpty(stand.MarktPferde);
        Assert.All(Enum.GetValues<Gebaeude>(), g => Assert.Equal(1, stand.GebaeudeStufe(g)));
    }

    [Fact]
    public void Advance_BuchtUnterhaltskostenAb()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        int guthabenVorher = stand.Guthaben;

        stand.Advance(stand.ZuletztAktualisiert.AddDays(10));

        Assert.True(stand.Guthaben < guthabenVorher);
    }

    [Fact]
    public void Advance_ProduziertMaterialImDreissigMinutenTakt()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        int materialVorher = stand.Materialbestand.Values.Sum();

        stand.Advance(stand.ZuletztAktualisiert.AddHours(2));

        int materialNachher = stand.Materialbestand.Values.Sum();
        Assert.True(materialNachher > materialVorher);
    }

    [Fact]
    public void Advance_HoltBeiLangerAbwesenheitHoechstensSiebenTageMaterialNach()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        stand.Advance(stand.ZuletztAktualisiert.AddDays(7));
        int materialNachSiebenTagen = stand.Materialbestand.Values.Sum();

        var zweitesSpiel = NeuesSpiel(new DateTime(2026, 1, 1));
        zweitesSpiel.Advance(zweitesSpiel.ZuletztAktualisiert.AddDays(60));
        int materialNachZweiMonaten = zweitesSpiel.Materialbestand.Values.Sum();

        // Sechzig Tage Abwesenheit dürfen nicht mehr Material bringen als sieben Tage Nachholzeit
        // erlauben würden (Designgrundsatz 2: Obergrenze von sieben Tagen für Offline-Ressourcen).
        // Bei identischem Startzustand und Zufall sollte exakt dieselbe Menge nachgeholt werden.
        Assert.Equal(materialNachSiebenTagen, materialNachZweiMonaten);
    }

    [Fact]
    public void HofAusbauen_LaeuftEineZeitLangUndErhoehtDannDieStufeDesGewaehltenGebaeudes()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        stand.Guthaben = 100_000;
        int kostenVorher = stand.HofAusbauKosten(Gebaeude.Zuchtstall);

        stand.HofAusbauen(Gebaeude.Zuchtstall, stand.ZuletztAktualisiert);
        Assert.NotNull(stand.LaufenderAusbau);
        Assert.Equal(1, stand.GebaeudeStufe(Gebaeude.Zuchtstall)); // noch nicht fertig
        Assert.Equal(1, stand.GebaeudeStufe(Gebaeude.Weide)); // andere Gebäude unberührt

        stand.Advance(stand.LaufenderAusbau!.Fertig.AddMinutes(1));

        Assert.Equal(2, stand.GebaeudeStufe(Gebaeude.Zuchtstall));
        Assert.Null(stand.LaufenderAusbau);
        Assert.True(stand.HofAusbauKosten(Gebaeude.Zuchtstall) > kostenVorher);
    }

    [Fact]
    public void HofAusbauen_OhneGenugGeld_WirftFehler()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        stand.Guthaben = 0;

        Assert.Throws<InvalidOperationException>(() => stand.HofAusbauen(Gebaeude.Zuchtstall, stand.ZuletztAktualisiert));
        Assert.Equal(1, stand.GebaeudeStufe(Gebaeude.Zuchtstall));
    }

    [Fact]
    public void HofAusbauen_WaehrendLaufenderAusbauLaeuft_WirftFehlerAuchFuerAnderesGebaeude()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        stand.Guthaben = 100_000;
        stand.HofAusbauen(Gebaeude.Zuchtstall, stand.ZuletztAktualisiert);

        Assert.Throws<InvalidOperationException>(() => stand.HofAusbauen(Gebaeude.Weide, stand.ZuletztAktualisiert));
    }

    [Fact]
    public void KaufeUndVerkaufePferd_BewegenGuthabenUndBestand()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        stand.Guthaben = 100_000;
        var pferd = stand.MarktPferde[0];
        int anzahlVorher = stand.Pferde.Count;

        stand.KaufeMarktpferd(pferd);
        Assert.Equal(anzahlVorher + 1, stand.Pferde.Count);
        Assert.DoesNotContain(pferd, stand.MarktPferde);

        int guthabenVorVerkauf = stand.Guthaben;
        stand.VerkaufePferd(pferd);
        Assert.True(stand.Guthaben > guthabenVorVerkauf);
        Assert.DoesNotContain(pferd, stand.Pferde);
    }

    [Fact]
    public void KaufeMarktpferd_OhneGenugGeld_WirftFehlerUndLaesstPferdImMarkt()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        stand.Guthaben = 0;
        var pferd = stand.MarktPferde[0];

        Assert.Throws<InvalidOperationException>(() => stand.KaufeMarktpferd(pferd));
        Assert.Contains(pferd, stand.MarktPferde);
    }

    [Fact]
    public void AktualisiereMarktFallsFaellig_TauschtPferdeErstNachTermin()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        var ersterMarkt = stand.MarktPferde.Select(p => p.Id).ToList();

        stand.AktualisiereMarktFallsFaellig(_inhalte, stand.ZuletztAktualisiert.AddDays(1));
        Assert.Equal(ersterMarkt, stand.MarktPferde.Select(p => p.Id).ToList());

        stand.AktualisiereMarktFallsFaellig(_inhalte, stand.ZuletztAktualisiert.AddDays(8));
        Assert.NotEqual(ersterMarkt, stand.MarktPferde.Select(p => p.Id).ToList());
    }

    [Fact]
    public void KaufeAusruestungUndRuesteAn_WirktAufModifikatorSumme()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        stand.Guthaben = 100_000;
        var pferd = stand.Pferde[0];
        var definition = _inhalte.AlleAusruestung().First(a => a.Slot == AusruestungsSlot.Sattel);

        stand.KaufeAusruestung(definition);
        var ausruestung = stand.Ausruestungen.Single();
        stand.RuesteAus(pferd, ausruestung, AusruestungsSlot.Sattel);

        float bonus = AusruestungsHelfer.ModifikatorSumme(pferd, stand.Ausruestungen, Merkmalsattribut.Rittigkeit);
        Assert.True(bonus > 0);

        stand.RuesteAb(pferd, AusruestungsSlot.Sattel);
        Assert.Empty(pferd.Ausgeruestet);
    }

    [Fact]
    public void VerkaufPferd_MitAusgeruesteterAusruestung_GibtSieWiederFrei()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        stand.Guthaben = 100_000;
        var pferd = stand.Pferde[0];
        var definition = _inhalte.AlleAusruestung().First(a => a.Slot == AusruestungsSlot.Decke);
        stand.KaufeAusruestung(definition);
        var ausruestung = stand.Ausruestungen.Single();
        stand.RuesteAus(pferd, ausruestung, AusruestungsSlot.Decke);

        stand.VerkaufePferd(pferd);

        Assert.Null(ausruestung.AngelegtBeiPferdId);
    }

    [Fact]
    public void WettkampfRechner_NaechsterZeitpunkt_LiegtAufFestemRasterInDerZukunft()
    {
        var klasse = _inhalte.HoleWettkampfklasse("kreis_dressur");
        var jetzt = new DateTime(2026, 3, 10, 12, 7, 0);

        var naechster = WettkampfRechner.NaechsterZeitpunkt(klasse, jetzt);

        Assert.True(naechster > jetzt);
        Assert.True((naechster - jetzt).TotalMinutes <= klasse.IntervallMinuten);
        Assert.Equal(0, (int)(naechster - naechster.Date).TotalMinutes % klasse.IntervallMinuten);
    }

    [Fact]
    public void WettkampfRechner_NaechsterZeitpunkt_IstDeterministischFuerDenselbenTermin()
    {
        var klasse = _inhalte.HoleWettkampfklasse("kreis_springen");
        var jetzt = new DateTime(2026, 3, 10, 12, 7, 0);

        var ersterAufruf = WettkampfRechner.NaechsterZeitpunkt(klasse, jetzt);
        var zweiterAufruf = WettkampfRechner.NaechsterZeitpunkt(klasse, jetzt);

        Assert.Equal(ersterAufruf, zweiterAufruf);
    }

    [Fact]
    public void WettkampfRechner_DarfTeilnehmen_PrueftAnsehenUndAlter()
    {
        var klasse = _inhalte.HoleWettkampfklasse("meister_vielseitigkeit");
        var pferd = new Pferd { Geburtsdatum = new DateTime(2020, 1, 1) };
        var jetzt = new DateTime(2026, 1, 1);

        Assert.False(WettkampfRechner.DarfTeilnehmen(klasse, pferd, ansehen: 0, jetzt));
        Assert.True(WettkampfRechner.DarfTeilnehmen(klasse, pferd, ansehen: 500, jetzt));

        var fohlen = new Pferd { Geburtsdatum = jetzt };
        Assert.False(WettkampfRechner.DarfTeilnehmen(klasse, fohlen, ansehen: 500, jetzt));
    }

    [Fact]
    public void MeldeAn_UndAdvance_LoestWettkampfAusUndLeertAnmeldungen()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1, 6, 0, 0));
        var pferd = stand.Pferde.First(p => p.AlleMerkmaleBekannt);
        var klasse = _inhalte.HoleWettkampfklasse("kreis_dressur");

        stand.MeldeAn(klasse, pferd, stand.ZuletztAktualisiert);
        Assert.Single(pferd.Anmeldungen);

        stand.Advance(stand.ZuletztAktualisiert.AddHours(1));

        Assert.Empty(pferd.Anmeldungen);
        Assert.Single(stand.WettkampfErgebnisse);
        Assert.InRange(stand.WettkampfErgebnisse[0].Platzierung, 1, 7);
    }

    [Fact]
    public void MeldeAn_MehrereTermine_LegtMehrereAnmeldungenAn()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1, 6, 0, 0));
        var pferd = stand.Pferde.First(p => p.AlleMerkmaleBekannt);
        var klasse = _inhalte.HoleWettkampfklasse("kreis_dressur");

        stand.MeldeAn(klasse, pferd, stand.ZuletztAktualisiert, anzahlTermine: 3);

        Assert.Equal(3, pferd.Anmeldungen.Count);
        Assert.Equal(pferd.Anmeldungen.Select(a => a.Zeitpunkt).Distinct().Count(), pferd.Anmeldungen.Count);
    }

    [Fact]
    public void MeldeAn_TraechtigesPferd_WirftFehler()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        var mutter = stand.Pferde.First(p => p.Geschlecht == Geschlecht.Stute && p.AlleMerkmaleBekannt);
        var vater = stand.Pferde.First(p => p.Geschlecht == Geschlecht.Hengst && p.AlleMerkmaleBekannt);
        stand.StarteZucht(_inhalte, mutter, vater, new ZuchtEinsatz(), stand.ZuletztAktualisiert);
        var klasse = _inhalte.HoleWettkampfklasse("kreis_dressur");

        Assert.Throws<InvalidOperationException>(() => stand.MeldeAn(klasse, mutter, stand.ZuletztAktualisiert));
    }

    [Fact]
    public void Training_BlockiertWettkampfAnmeldungNichtMehr()
    {
        // Kernänderung aus dem Phase-4-Auftrag: Training läuft nebenbei mit, es ist keine
        // exklusive Beschäftigung mehr wie in den ersten Phasen.
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        var pferd = stand.Pferde.First(p => p.AlleMerkmaleBekannt);
        pferd.TrainingEinreihen(StatTyp.Tempo, Intensitaet.Intensiv);
        var klasse = _inhalte.HoleWettkampfklasse("kreis_dressur");

        stand.MeldeAn(klasse, pferd, stand.ZuletztAktualisiert);

        Assert.Single(pferd.Anmeldungen);
    }

    [Fact]
    public void Preisrechner_VerkaufIstGuenstigerAlsKauf()
    {
        var pferd = PferdeGenerator.Erzeuge(new GameRandom(5), _inhalte, _inhalte.HoleRasse("warmblut"), DateTime.UtcNow);
        Assert.True(Preisrechner.Verkaufspreis(pferd) < Preisrechner.Kaufpreis(pferd));
    }
}
