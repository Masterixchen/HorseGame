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
        Assert.Equal(1, stand.Hofstufe);
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
    public void Advance_ProduziertWoechentlichMaterial()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        int materialVorher = stand.Materialbestand.Values.Sum();

        stand.Advance(stand.ZuletztAktualisiert.AddDays(8));

        int materialNachher = stand.Materialbestand.Values.Sum();
        Assert.True(materialNachher > materialVorher);
    }

    [Fact]
    public void HofAusbauen_ErhoehtStufeUndKostenSteigenDanach()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        stand.Guthaben = 100_000;
        int kostenVorher = stand.HofAusbauKosten();

        stand.HofAusbauen();

        Assert.Equal(2, stand.Hofstufe);
        Assert.True(stand.HofAusbauKosten() > kostenVorher);
    }

    [Fact]
    public void HofAusbauen_OhneGenugGeld_WirftFehler()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        stand.Guthaben = 0;

        Assert.Throws<InvalidOperationException>(stand.HofAusbauen);
        Assert.Equal(1, stand.Hofstufe);
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
    public void WettkampfRechner_NaechsterZeitpunkt_LiegtInDerZukunft()
    {
        var klasse = _inhalte.HoleWettkampfklasse("kreis_dressur");
        var jetzt = new DateTime(2026, 3, 10, 12, 0, 0);

        var naechster = WettkampfRechner.NaechsterZeitpunkt(klasse, jetzt);

        Assert.True(naechster > jetzt);
        Assert.Equal(klasse.Stundenzeitpunkt, naechster.Hour);
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
    public void MeldeAn_UndAdvance_LoestWettkampfAusUndSetztAnmeldungZurueck()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1, 6, 0, 0));
        var pferd = stand.Pferde.First(p => p.AlleMerkmaleBekannt);
        var klasse = _inhalte.HoleWettkampfklasse("kreis_dressur");

        stand.MeldeAn(klasse, pferd, stand.ZuletztAktualisiert);
        Assert.NotNull(pferd.Anmeldung);

        stand.Advance(stand.ZuletztAktualisiert.AddDays(2));

        Assert.Null(pferd.Anmeldung);
        Assert.Single(stand.WettkampfErgebnisse);
        Assert.InRange(stand.WettkampfErgebnisse[0].Platzierung, 1, 7);
    }

    [Fact]
    public void MeldeAn_BeschaeftigtesPferd_WirftFehler()
    {
        var stand = NeuesSpiel(new DateTime(2026, 1, 1));
        var pferd = stand.Pferde.First(p => p.AlleMerkmaleBekannt);
        pferd.StarteTraining(StatTyp.Tempo, stand.ZuletztAktualisiert, stand.Zufall);
        var klasse = _inhalte.HoleWettkampfklasse("kreis_dressur");

        Assert.Throws<InvalidOperationException>(() => stand.MeldeAn(klasse, pferd, stand.ZuletztAktualisiert));
    }

    [Fact]
    public void Preisrechner_VerkaufIstGuenstigerAlsKauf()
    {
        var pferd = PferdeGenerator.Erzeuge(new GameRandom(5), _inhalte, _inhalte.HoleRasse("warmblut"), DateTime.UtcNow);
        Assert.True(Preisrechner.Verkaufspreis(pferd) < Preisrechner.Kaufpreis(pferd));
    }
}
