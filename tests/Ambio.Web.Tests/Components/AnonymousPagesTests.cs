using System.Reflection;

using Ambio.Web.Components;
using Ambio.Web.Components.Account.Pages;
using Ambio.Web.Components.Pages;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Ambio.Web.Tests.Components;

/// <summary>Every page requires the logged-in user through the fallback policy, except the ones marked
/// <see cref="AllowAnonymousAttribute"/> (ADR 0028). An attribute put in an <c>_Imports.razor</c> would reach every
/// page below it, the account pages of <c>Manage</c> included.</summary>
public class AnonymousPagesTests
{
    [Fact]
    public void AllowAnonymous_OnRoutablePages_OnlyThePagesShownBeforeLoggingInAndError()
    {
        var pages = typeof(Routes).Assembly.GetTypes()
            .Where(type => type.GetCustomAttributes<RouteAttribute>().Any())
            .ToList();

        var expected = pages
            .Where(type => type.Namespace == typeof(Login).Namespace)
            .Append(typeof(Error))
            .Select(type => type.FullName)
            .Order();
        var anonymous = pages
            .Where(type => type.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
            .Select(type => type.FullName)
            .Order();

        Assert.Equal(expected, anonymous);
    }
}
