namespace Ambio.Infrastructure.Common;

internal sealed class SemaphoreContainer
{
    public SemaphoreSlim Semaphore { get; } = new(1, 1);
}
