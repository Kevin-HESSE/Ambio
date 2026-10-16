using Microsoft.Extensions.Options;

using NanoidDotNet;

namespace Ambio.Infrastructure.Common;

/// <summary>Draws the random short ids of the URLs (ADR 0014) with NanoId, from the <see cref="ShortIdOptions"/>.</summary>
internal sealed class ShortIdGenerator
{
    // A free id is found on the first draw until the alphabet is nearly used up: this bound only stops a full one.
    private const int MaxAttempts = 100;

    private readonly ShortIdOptions _options;

    public ShortIdGenerator(IOptions<ShortIdOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>A random id that <paramref name="isTakenAsync"/> doesn't report as taken, drawn again while it is.
    /// The unique index of the table is the final guard.</summary>
    /// <exception cref="InvalidOperationException">Every draw was taken: the alphabet or the size must grow.</exception>
    public async Task<string> FreeAsync(Func<string, Task<bool>> isTakenAsync)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var candidate = Nanoid.Generate(_options.Alphabet, _options.Size);

            if (!await isTakenAsync(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            $"No free short id after {MaxAttempts} draws: widen the alphabet or the size of {ShortIdOptions.Section}.");
    }
}
