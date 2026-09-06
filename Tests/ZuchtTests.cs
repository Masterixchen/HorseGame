using Core;

namespace Tests;

public class ZuchtTests
{
    private readonly Inhaltsdatenbank _inhalte = new();
    private readonly Dictionary<Gebaeude, int> _gebaeudestufen = Enum.GetValues<Gebaeude>().ToDictionary(g => g, _ => 1);

    private (Pferd mutter, Pferd vater) ErzeugeEltern(GameRandom zufall)
    {
        var rasse = _inhalte.HoleRasse("warmblut");
        var mutter = PferdeGenerator.Erzeuge(zufall, _inhalte, rasse, DateTime.UtcNow, Geschlecht.Stute);
        var vater = PferdeGenerator.Erzeuge(zufall, _inhalte, rasse, DateTime.UtcNow, Geschlecht.Hengst);
        return (mutter, vater);
    }

    [Fact]
    public void ErzeugeFohlen_HaeltSeltenheitsGrenzenEin()
    {
        var zufall = new GameRandom(100);
        var (mutter, vater) = ErzeugeEltern(zufall);
        var einsatz = new ZuchtEinsatz();

        for (int i = 0; i < 500; i++)
        {
            var fohlen = ZuchtRechner.ErzeugeFohlen(zufall, _inhalte, mutter, vater, einsatz, _gebaeudestufen, DateTime.UtcNow);
            Assert.True(fohlen.Praefixe.Count <= 3);
            Assert.True(fohlen.Suffixe.Count <= 3);
            Assert.Equal(SeltenheitRegeln.AusMerkmalsanzahl(fohlen.Praefixe.Count, fohlen.Suffixe.Count), fohlen.Seltenheit);
        }
    }

    [Fact]
    public void ErzeugtesFohlen_HatAlleMerkmaleVerborgen()
    {
        var zufall = new GameRandom(101);
        var (mutter, vater) = ErzeugeEltern(zufall);
        var fohlen = ZuchtRechner.ErzeugeFohlen(zufall, _inhalte, mutter, vater, new ZuchtEinsatz(), _gebaeudestufen, DateTime.UtcNow);

        Assert.All(fohlen.AlleMerkmale(), m => Assert.False(m.Bekannt));
        Assert.All(fohlen.AlleMerkmale(), m => Assert.NotNull(m.Aufdeckzeitpunkt));
        Assert.False(fohlen.AlleMerkmaleBekannt);
    }

    [Fact]
    public void Ahnentafel_GarantiertMindestensEinPraefixUndSuffix()
    {
        // Blutlinienstufe 1 ohne Vererbung (frisch erzeugte Eltern haben oft 0 Merkmale) -
        // hier soll Ahnentafel trotzdem je Achse mindestens ein Merkmal erzwingen.
        var zufall = new GameRandom(202);
        var rasse = _inhalte.HoleRasse("warmblut");
        var mutter = new Pferd { RasseId = rasse.Id, Geschlecht = Geschlecht.Stute, Blutlinienstufe = 1 };
        var vater = new Pferd { RasseId = rasse.Id, Geschlecht = Geschlecht.Hengst, Blutlinienstufe = 1 };
        mutter.Werte = vater.Werte = new StatBlock();
        var einsatz = new ZuchtEinsatz { Ahnentafel = true };

        var fohlen = ZuchtRechner.ErzeugeFohlen(zufall, _inhalte, mutter, vater, einsatz, _gebaeudestufen, DateTime.UtcNow);

        Assert.True(fohlen.Praefixe.Count >= 1);
        Assert.True(fohlen.Suffixe.Count >= 1);
    }

    [Fact]
    public void Spezialistenbetreuung_FuegtGewaehltesMerkmalAufStufeEinsHinzu()
    {
        var zufall = new GameRandom(303);
        var (mutter, vater) = ErzeugeEltern(zufall);
        var einsatz = new ZuchtEinsatz { SpezialistenMerkmalId = "windlaeufer" };

        var fohlen = ZuchtRechner.ErzeugeFohlen(zufall, _inhalte, mutter, vater, einsatz, _gebaeudestufen, DateTime.UtcNow);

        var windlaeufer = fohlen.Praefixe.FirstOrDefault(p => p.DefinitionId == "windlaeufer");
        Assert.NotNull(windlaeufer);
        Assert.Equal(1, windlaeufer!.Stufe);
    }

    [Fact]
    public void ZuchtEinsatz_WagnisMitGarantieIstUngueltig()
    {
        var einsatz = new ZuchtEinsatz { Wagnis = true, Ahnentafel = true };
        Assert.False(einsatz.IstGueltig());

        var gueltig = new ZuchtEinsatz { Wagnis = true, Kraftfutter = true };
        Assert.True(gueltig.IstGueltig());
    }

    [Fact]
    public void StarteZucht_ueberSpielstand_BuchtMaterialienAbUndSetztTraechtigkeit()
    {
        var zufall = new GameRandom(404);
        var (mutter, vater) = ErzeugeEltern(zufall);
        var stand = new Spielstand { Zufall = zufall, Pferde = { mutter, vater } };
        stand.Materialbestand[MaterialTyp.Kraftfutter] = 1;

        stand.StarteZucht(_inhalte, mutter, vater, new ZuchtEinsatz { Kraftfutter = true }, DateTime.UtcNow);

        Assert.Equal(0, stand.Materialbestand[MaterialTyp.Kraftfutter]);
        Assert.NotNull(mutter.Traechtigkeit);
    }

    [Fact]
    public void StarteZucht_OhneGenugMaterial_WirftFehlerUndBuchtNichtsAb()
    {
        var zufall = new GameRandom(405);
        var (mutter, vater) = ErzeugeEltern(zufall);
        var stand = new Spielstand { Zufall = zufall, Pferde = { mutter, vater } };

        Assert.Throws<InvalidOperationException>(() =>
            stand.StarteZucht(_inhalte, mutter, vater, new ZuchtEinsatz { Kraftfutter = true }, DateTime.UtcNow));
        Assert.Null(mutter.Traechtigkeit);
    }

    [Fact]
    public void StarteZucht_FalscheGeschlechterkombination_WirftFehler()
    {
        var zufall = new GameRandom(406);
        var (mutter, _) = ErzeugeEltern(zufall);
        var (mutter2, _) = ErzeugeEltern(zufall);
        var stand = new Spielstand { Zufall = zufall, Pferde = { mutter, mutter2 } };

        Assert.Throws<InvalidOperationException>(() =>
            stand.StarteZucht(_inhalte, mutter, mutter2, new ZuchtEinsatz(), DateTime.UtcNow));
    }

    [Fact]
    public void SpielstandAdvance_LaesstFohlenNachTragezeitGeboreWerden()
    {
        var zufall = new GameRandom(500);
        var (mutter, vater) = ErzeugeEltern(zufall);
        var start = new DateTime(2026, 1, 1);
        var stand = new Spielstand { ZuletztAktualisiert = start, Zufall = zufall, Pferde = { mutter, vater } };
        stand.Materialbestand[MaterialTyp.Kraftfutter] = 1;
        stand.StarteZucht(_inhalte, mutter, vater, new ZuchtEinsatz { Kraftfutter = true }, start);

        int anzahlVorher = stand.Pferde.Count;
        stand.Advance(start.AddDays(20));

        Assert.Null(mutter.Traechtigkeit);
        Assert.Equal(anzahlVorher + 1, stand.Pferde.Count);
    }

    [Fact]
    public void Vorschau_LiefertWahrscheinlichkeitenDieSichZuEinsAufsummieren()
    {
        var zufall = new GameRandom(600);
        var (mutter, vater) = ErzeugeEltern(zufall);

        var ergebnis = ZuchtVorschau.Berechne(_inhalte, mutter, vater, new ZuchtEinsatz(), _gebaeudestufen, stichproben: 500);

        double summe = ergebnis.SeltenheitsChancen.Values.Sum();
        Assert.InRange(summe, 0.99, 1.01);
        Assert.True(ergebnis.BlutlinienstufeMin <= ergebnis.BlutlinienstufeMax);
    }

    [Fact]
    public void Vorschau_RuehrtDenGespeichertenSpielzufallNichtAn()
    {
        var zufall = new GameRandom(700);
        var (mutter, vater) = ErzeugeEltern(zufall);
        ulong zustandVorher = zufall.Zustand;

        ZuchtVorschau.Berechne(_inhalte, mutter, vater, new ZuchtEinsatz(), _gebaeudestufen, stichproben: 500);

        Assert.Equal(zustandVorher, zufall.Zustand);
    }

    [Fact]
    public void GezuechteteLinieUebertrifftUeberGenerationenDenMarkt()
    {
        // Belegt das "Fertig, wenn" aus der Projektanweisung für Phase 2 im Kleinen: eine über
        // mehrere Generationen gezielt gezüchtete Linie (immer die zwei besten Nachkommen
        // weiterverpaart) soll beim Tempo-Potenzial über dem Marktdurchschnitt liegen.
        var zufall = new GameRandom(800);
        var rasse = _inhalte.HoleRasse("vollblut");

        var mutter = PferdeGenerator.Erzeuge(zufall, _inhalte, rasse, DateTime.UtcNow, Geschlecht.Stute);
        var vater = PferdeGenerator.Erzeuge(zufall, _inhalte, rasse, DateTime.UtcNow, Geschlecht.Hengst);

        for (int generation = 0; generation < 5; generation++)
        {
            var kandidaten = Enumerable.Range(0, 8)
                .Select(_ => ZuchtRechner.ErzeugeFohlen(zufall, _inhalte, mutter, vater, new ZuchtEinsatz(), _gebaeudestufen, DateTime.UtcNow))
                .OrderByDescending(f => f.Werte.Tempo.Potenzial)
                .ToList();

            mutter = kandidaten.First(f => f.Geschlecht == Geschlecht.Stute);
            vater = kandidaten.First(f => f.Geschlecht == Geschlecht.Hengst);
        }

        float zuchtDurchschnitt = (mutter.Werte.Tempo.Potenzial + vater.Werte.Tempo.Potenzial) / 2f;

        float marktSumme = 0f;
        const int marktAnzahl = 200;
        for (int i = 0; i < marktAnzahl; i++)
            marktSumme += PferdeGenerator.Erzeuge(zufall, _inhalte, rasse, DateTime.UtcNow).Werte.Tempo.Potenzial;

        Assert.True(zuchtDurchschnitt > marktSumme / marktAnzahl);
    }

    [Fact]
    public void Zuchtstall_BegrenztDieBlutlinienstufeAufDenGebaeudeDeckel()
    {
        var zufall = new GameRandom(900);
        var rasse = _inhalte.HoleRasse("warmblut");
        // Sehr hohe Eltern-Blutlinie, aber ein Zuchtstall auf Stufe 1 (Deckel laut GebaeudeRegeln: 27).
        var mutter = new Pferd { RasseId = rasse.Id, Geschlecht = Geschlecht.Stute, Blutlinienstufe = 200, Werte = new StatBlock() };
        var vater = new Pferd { RasseId = rasse.Id, Geschlecht = Geschlecht.Hengst, Blutlinienstufe = 200, Werte = new StatBlock() };
        var niedrigerZuchtstall = new Dictionary<Gebaeude, int> { [Gebaeude.Zuchtstall] = 1 };

        for (int i = 0; i < 50; i++)
        {
            var fohlen = ZuchtRechner.ErzeugeFohlen(zufall, _inhalte, mutter, vater, new ZuchtEinsatz(), niedrigerZuchtstall, DateTime.UtcNow);
            Assert.True(fohlen.Blutlinienstufe <= GebaeudeRegeln.MaxBlutlinienstufe(1));
        }
    }

    [Fact]
    public void Zuchtstall_HoehereStufeErlaubtHoehereBlutlinienstufe()
    {
        var zufall = new GameRandom(901);
        var rasse = _inhalte.HoleRasse("warmblut");
        var mutter = new Pferd { RasseId = rasse.Id, Geschlecht = Geschlecht.Stute, Blutlinienstufe = 200, Werte = new StatBlock() };
        var vater = new Pferd { RasseId = rasse.Id, Geschlecht = Geschlecht.Hengst, Blutlinienstufe = 200, Werte = new StatBlock() };
        var hoherZuchtstall = new Dictionary<Gebaeude, int> { [Gebaeude.Zuchtstall] = 10 };

        var fohlen = ZuchtRechner.ErzeugeFohlen(zufall, _inhalte, mutter, vater, new ZuchtEinsatz(), hoherZuchtstall, DateTime.UtcNow);

        Assert.True(fohlen.Blutlinienstufe > GebaeudeRegeln.MaxBlutlinienstufe(1));
    }

    [Fact]
    public void StarteZucht_DeckstationBegrenztGleichzeitigeTraechtigkeiten()
    {
        var zufall = new GameRandom(902);
        var (mutter, vater) = ErzeugeEltern(zufall);
        var (mutter2, vater2) = ErzeugeEltern(zufall);
        var stand = new Spielstand
        {
            Zufall = zufall,
            Pferde = { mutter, vater, mutter2, vater2 },
            Gebaeudestufen = new Dictionary<Gebaeude, int> { [Gebaeude.Deckstation] = 1 }
        };

        stand.StarteZucht(_inhalte, mutter, vater, new ZuchtEinsatz(), DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            stand.StarteZucht(_inhalte, mutter2, vater2, new ZuchtEinsatz(), DateTime.UtcNow));
    }
}
