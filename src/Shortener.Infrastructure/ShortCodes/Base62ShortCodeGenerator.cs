using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;

namespace Shortener.Infrastructure.ShortCodes;

internal sealed class Base62ShortCodeGenerator(IOptions<ShortenerOptions> options) : IShortCodeGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private readonly int _length = options.Value.ShortCodeLength;

    public string Generate()
    {
        var chars = new char[_length];
        // Use rejection sampling to avoid modulo bias
        var alphabetLength = (byte)Alphabet.Length;
        var limit = (byte)(256 - 256 % alphabetLength);

        var i = 0;
        while (i < _length)
        {
            var b = RandomNumberGenerator.GetBytes(1)[0];
            if (b < limit)
            {
                chars[i++] = Alphabet[b % alphabetLength];
            }
        }

        return new string(chars);
    }
}
