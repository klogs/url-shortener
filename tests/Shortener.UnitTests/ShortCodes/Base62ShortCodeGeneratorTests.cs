using Microsoft.Extensions.Options;
using Shortener.Application.Options;
using Shortener.Infrastructure.ShortCodes;

namespace Shortener.UnitTests.ShortCodes;

public sealed class Base62ShortCodeGeneratorTests
{
    private static Base62ShortCodeGenerator CreateGenerator(int length = 7)
        => new(Options.Create(new ShortenerOptions { ShortCodeLength = length }));

    [Fact]
    public void Generate_ReturnsCorrectLength()
    {
        var generator = CreateGenerator(7);
        var code = generator.Generate();
        Assert.Equal(7, code.Length);
    }

    [Fact]
    public void Generate_ContainsOnlyBase62Characters()
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var generator = CreateGenerator(7);

        for (var i = 0; i < 100; i++)
        {
            var code = generator.Generate();
            Assert.All(code, c => Assert.Contains(c, alphabet));
        }
    }

    [Fact]
    public void Generate_ProducesUniqueCodes()
    {
        var generator = CreateGenerator(7);
        var codes = Enumerable.Range(0, 1000).Select(_ => generator.Generate()).ToHashSet();
        // With 62^7 ≈ 3.5 trillion combinations, collisions in 1000 draws are astronomically unlikely
        Assert.Equal(1000, codes.Count);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(12)]
    public void Generate_RespectsConfiguredLength(int length)
    {
        var generator = CreateGenerator(length);
        Assert.Equal(length, generator.Generate().Length);
    }
}
