using System.Runtime.CompilerServices;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

public static class ExtensionsForBogus
{
    public static Faker<T> WithRecord<T>(this Faker<T> faker) where T : class
    {
        faker.CustomInstantiator(_ => RuntimeHelpers.GetUninitializedObject(typeof(T)) as T ?? throw new InvalidOperationException());
        return faker;
    }
}
