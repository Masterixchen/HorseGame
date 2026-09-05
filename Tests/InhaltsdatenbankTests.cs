using Core;

namespace Tests;

/// <summary>Prüft nicht nur, dass die JSON-Dateien laden, sondern auch, dass sie in sich
/// konsistent sind - z.B. dass jede Rasse nur auf Merkmal-Ids verweist, die es auch gibt.
/// Das fängt Tippfehler in den Data-Dateien ab, ohne dass man die Oberfläche starten muss.</summary>
public class InhaltsdatenbankTests
{
    private readonly Inhaltsdatenbank _inhalte = new();

    [Fact]
    public void AlleRassen_HabenGueltigesImplizitesMerkmal()
    {
        foreach (var rasse in _inhalte.AlleRassen())
        {
            var merkmal = _inhalte.HoleMerkmal(rasse.ImplizitesMerkmal);
            Assert.Equal(MerkmalsArt.Implizit, merkmal.Art);
        }
    }

    [Fact]
    public void AlleRassen_VerweisenNurAufVorhandenePraefixeUndSuffixe()
    {
        foreach (var rasse in _inhalte.AlleRassen())
        {
            foreach (var id in rasse.MoeglichePraefixe)
                Assert.Equal(MerkmalsArt.Praefix, _inhalte.HoleMerkmal(id).Art);

            foreach (var id in rasse.MoeglicheSuffixe)
                Assert.Equal(MerkmalsArt.Suffix, _inhalte.HoleMerkmal(id).Art);
        }
    }

    [Fact]
    public void AlleRassen_HabenMindestensEineFarbe()
    {
        foreach (var rasse in _inhalte.AlleRassen())
            Assert.NotEmpty(rasse.Farben);
    }

    [Fact]
    public void AlleMerkmalsstufen_HabenGueltigeWertebereiche()
    {
        foreach (var rasse in _inhalte.AlleRassen())
        foreach (var id in rasse.MoeglichePraefixe.Concat(rasse.MoeglicheSuffixe).Append(rasse.ImplizitesMerkmal))
        {
            var definition = _inhalte.HoleMerkmal(id);
            foreach (var stufe in definition.Stufen)
                foreach (var effekt in stufe.Effekte)
                    Assert.True(effekt.Max >= effekt.Min, $"{definition.Id} Stufe {stufe.Stufe}: Max < Min");
        }
    }

    [Fact]
    public void NamenslistenSindNichtLeer()
    {
        Assert.NotEmpty(_inhalte.NamenFuer(Geschlecht.Stute));
        Assert.NotEmpty(_inhalte.NamenFuer(Geschlecht.Hengst));
    }
}
