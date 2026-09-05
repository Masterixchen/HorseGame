using Core;

namespace Tests;

public class SeltenheitTests
{
    [Theory]
    [InlineData(Seltenheit.Gewoehnlich, 0)]
    [InlineData(Seltenheit.Solide, 1)]
    [InlineData(Seltenheit.Selten, 2)]
    [InlineData(Seltenheit.Elite, 3)]
    public void MaxPraefixeUndSuffixe_EntsprichtDerTabelle(Seltenheit stufe, int erwartet)
    {
        Assert.Equal(erwartet, SeltenheitRegeln.MaxPraefixe(stufe));
        Assert.Equal(erwartet, SeltenheitRegeln.MaxSuffixe(stufe));
    }

    [Fact]
    public void Wuerfle_LiefertNieEinzigartig()
    {
        var zufall = new GameRandom(9001);
        for (int i = 0; i < 10_000; i++)
            Assert.NotEqual(Seltenheit.Einzigartig, SeltenheitRegeln.Wuerfle(zufall));
    }
}
