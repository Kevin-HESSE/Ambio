using Ambio.Web.Components.Account.Shared;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.Extensions.DependencyInjection;

namespace Ambio.Web.Tests.Components.Account.Shared;

public class RedirectToLoginTests : BunitContext
{
    private const string ProtectedUri = "http://localhost/applications?status=Applied";
    private const string LoginPath = "Account/Login";

    [Fact]
    public void Render_NavigatesToLoginWithReturnUrl()
    {
        var navigationManager = Services.GetRequiredService<BunitNavigationManager>();
        navigationManager.NavigateTo(ProtectedUri);

        Render<RedirectToLogin>();

        var navigation = navigationManager.History.First();
        Assert.Equal($"{LoginPath}?returnUrl={Uri.EscapeDataString(ProtectedUri)}", navigation.Uri);
        Assert.True(navigation.Options.ForceLoad);
    }
}
