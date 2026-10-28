using Ambio.Web.Components.Shared;

namespace Ambio.Web.Tests.Components.Shared;

public class DisplaySizesTests
{
    [Theory]
    [MemberData(nameof(Sizes))]
    public void ToDisplaySize_WritesKilobytesThenMegabytes(long bytes, string expected)
    {
        Assert.Equal(expected, bytes.ToDisplaySize());
    }

    public static TheoryData<long, string> Sizes => new()
    {
        { 1, "1 KB" },
        { 188_416, "184 KB" },
        { 188_417, "185 KB" },
        { 1_258_291, "1.2 MB" },
        { 5 * 1024 * 1024, "5 MB" },
    };
}
