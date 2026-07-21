using ArchUnitNET.Domain;
using ArchUnitNET.xUnit;
using Shouldly;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Zajednica.Architecture.Tests;

public class ModulesTests : BaseArchitecturalTests
{
    [Theory]
    [MemberData(nameof(GetModules))]
    public void Api_projects_should_only_reference_themselves_and_core_building_blocks(string moduleName)
    {
        var examinedTypes = GetExaminedTypes($"Zajednica.{moduleName}.Api");
        var forbiddenTypes = GetForbiddenTypes("Zajednica.BuildingBlocks.Core", $"Zajednica.{moduleName}.Api");

        var rule = Types().That().Are(examinedTypes).Should().NotDependOnAny(forbiddenTypes).WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Theory]
    [MemberData(nameof(GetModules))]
    public void Core_projects_should_only_reference_themselves_Api_projects_and_core_building_blocks(string moduleName)
    {
        var examinedTypes = GetExaminedTypes($"Zajednica.{moduleName}.Core");
        var forbiddenTypes = GetForbiddenTypes("Zajednica.BuildingBlocks.Core", "Zajednica\\..+\\.Api", $"Zajednica.{moduleName}.Core");

        // WithoutRequiringPositiveResults: module Core assemblies may still be empty scaffolds.
        var rule = Types().That().Are(examinedTypes).Should().NotDependOnAny(forbiddenTypes).WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Theory]
    [MemberData(nameof(GetModules))]
    public void Infra_projects_should_only_reference_themselves_their_Api_and_core_projects_and_building_blocks(string moduleName)
    {
        var examinedTypes = GetExaminedTypes($"Zajednica.{moduleName}.Infrastructure");
        var forbiddenTypes = GetForbiddenTypes("Zajednica.BuildingBlocks.", $"Zajednica.{moduleName}.");

        var rule = Types().That().Are(examinedTypes).Should().NotDependOnAny(forbiddenTypes).WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Theory]
    [MemberData(nameof(GetModules))]
    public void Domain_namespaces_should_only_reference_themselves_and_core_building_blocks(string moduleName)
    {
        var allTypesFromCoreAssembly = GetExaminedTypes($"Zajednica.{moduleName}.Core").ToList();
        var domainTypes = allTypesFromCoreAssembly.Where(x => x.FullName.Contains(".Domain.")).ToList();
        var nonDomainTypes = allTypesFromCoreAssembly.Where(x => !x.FullName.Contains(".Domain."));
        var typesFromOtherAssemblies = GetForbiddenTypes("Zajednica.BuildingBlocks.Core", $"Zajednica.{moduleName}.Core");

        var otherAssemblyRule = Types().That().Are(domainTypes).Should().NotDependOnAny(typesFromOtherAssemblies).WithoutRequiringPositiveResults();
        var sameAssemblyRule = Types().That().Are(domainTypes).Should().NotDependOnAny(nonDomainTypes).WithoutRequiringPositiveResults();

        otherAssemblyRule.Check(Architecture);
        sameAssemblyRule.Check(Architecture);
    }

    [Theory]
    [MemberData(nameof(GetModules))]
    public void Services_should_not_reference_public_Apis_of_other_modules(string moduleName)
    {
        var allTypesFromCoreAssembly = GetExaminedTypes($"Zajednica.{moduleName}.Core").ToList();
        var useCaseTypes = allTypesFromCoreAssembly.Where(x => x.FullName.Contains(".UseCases.")).ToList();
        var typesFromOtherAssemblies = GetForbiddenTypes("Zajednica.Api", $"Zajednica.{moduleName}.Api");
        var publicApiTypesFromOtherAssemblies = typesFromOtherAssemblies.Where(x => x.FullName.Contains("Api.Public"));

        var rule = Types().That().Are(useCaseTypes).Should().NotDependOnAny(publicApiTypesFromOtherAssemblies).WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Fact]
    public void Web_Api_should_not_reference_internal_Apis_of_modules()
    {
        var apiTypes = GetExaminedTypes("Zajednica.Api").ToList();
        var typesFromOtherAssemblies = GetForbiddenTypes("Zajednica.Api");
        var internalApiTypes = typesFromOtherAssemblies.Where(x => x.FullName.Contains("Api.Internal"));

        var rule = Types().That().Are(apiTypes).Should().NotDependOnAny(internalApiTypes).WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    // The cross-module seam is meant to be enumerable: every type another module may touch is an
    // IInternal*Service interface, so a caller's constructor shows at a glance what crosses a boundary.
    [Theory]
    [MemberData(nameof(GetModules))]
    public void Internal_Api_services_should_be_interfaces_named_IInternalXService(string moduleName)
    {
        var internalApiTypes = GetExaminedTypes($"Zajednica.{moduleName}.Api")
            .Where(x => x.FullName.Contains(".Api.Internal.") && !x.FullName.Contains(".Api.Internal.Dto."))
            .ToList();

        var offenders = internalApiTypes
            .Where(x => x is not Interface || !x.Name.StartsWith("IInternal") || !x.Name.EndsWith("Service"))
            .Select(x => x.FullName)
            .ToList();

        offenders.ShouldBeEmpty($"{moduleName}: Api/Internal must expose only IInternal*Service interfaces.");
    }

    public static IEnumerable<object[]> GetModules() => new List<object[]>
    {
        new object[] { "Identity" },
        new object[] { "Community" },
        new object[] { "Feed" },
        new object[] { "Chat" }
    };
}
