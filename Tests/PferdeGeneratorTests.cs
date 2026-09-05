using Core;

namespace Tests;

public class PferdeGeneratorTests
{
    private readonly Inhaltsdatenbank _inhalte = new();

    [Fact]
    public void ErzeugtePferde_HaltenDieSeltenheitsGrenzenEin()
    {
        var zufall = new GameRandom(55);
        var jetzt = new DateTime(2026, 1, 1);

        for (int i = 0; i < 2000; i++)
        {
            var rasse = zufall.Waehle(_inhalte.AlleRassen().ToList());
            var pferd = PferdeGenerator.Erzeuge(zufall, _inhalte, rasse, jetzt);

            Assert.True(pferd.Praefixe.Count <= SeltenheitRegeln.MaxPraefixe(pferd.Seltenheit));
            Assert.True(pferd.Suffixe.Count <= SeltenheitRegeln.MaxSuffixe(pferd.Seltenheit));
        }
    }

    [Fact]
    public void ErzeugtesPferd_HatAktuellNieUeberPotenzial()
    {
        var zufall = new GameRandom(56);
        var rasse = _inhalte.HoleRasse("warmblut");
        var pferd = PferdeGenerator.Erzeuge(zufall, _inhalte, rasse, DateTime.UtcNow);

        foreach (var (_, stat) in pferd.Werte.Alle())
            Assert.True(stat.Aktuell <= stat.Potenzial);
    }

    [Fact]
    public void ErzeugtesPferd_HatKeineDoppeltenMerkmale()
    {
        var zufall = new GameRandom(57);
        var rasse = _inhalte.HoleRasse("warmblut");

        for (int i = 0; i < 200; i++)
        {
            var pferd = PferdeGenerator.Erzeuge(zufall, _inhalte, rasse, DateTime.UtcNow);
            var praefixIds = pferd.Praefixe.Select(p => p.DefinitionId).ToList();
            var suffixIds = pferd.Suffixe.Select(s => s.DefinitionId).ToList();

            Assert.Equal(praefixIds.Count, praefixIds.Distinct().Count());
            Assert.Equal(suffixIds.Count, suffixIds.Distinct().Count());
        }
    }
}
