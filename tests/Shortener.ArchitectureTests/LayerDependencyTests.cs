using System.Reflection;

namespace Shortener.ArchitectureTests;

public sealed class LayerDependencyTests
{
    // Anchor types used to resolve assemblies under test
    private static readonly Assembly DomainAssembly = typeof(Domain.AssemblyMarker).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.AssemblyMarker).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.AssemblyMarker).Assembly;
    private static readonly Assembly ApiAssembly = typeof(Shortener.Api.ApiMarker).Assembly;
    private static readonly Assembly RedirectAssembly = typeof(Shortener.Redirect.RedirectMarker).Assembly;

    [Fact]
    public void Domain_must_not_reference_Application()
        => AssertNoReference(DomainAssembly, ApplicationAssembly);

    [Fact]
    public void Domain_must_not_reference_Infrastructure()
        => AssertNoReference(DomainAssembly, InfrastructureAssembly);

    [Fact]
    public void Domain_must_not_reference_Api()
        => AssertNoReference(DomainAssembly, ApiAssembly);

    [Fact]
    public void Application_must_not_reference_Infrastructure()
        => AssertNoReference(ApplicationAssembly, InfrastructureAssembly);

    [Fact]
    public void Application_must_not_reference_Api()
        => AssertNoReference(ApplicationAssembly, ApiAssembly);

    [Fact]
    public void Redirect_must_not_reference_Api()
        => AssertNoReference(RedirectAssembly, ApiAssembly);

    private static void AssertNoReference(Assembly source, Assembly forbidden)
    {
        var referencedNames = source.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.False(
            referencedNames.Contains(forbidden.GetName().Name),
            $"{source.GetName().Name} must NOT reference {forbidden.GetName().Name}.");
    }
}
