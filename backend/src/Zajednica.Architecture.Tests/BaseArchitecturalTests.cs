using System.Text.RegularExpressions;
using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using Assembly = System.Reflection.Assembly;

namespace Zajednica.Architecture.Tests;

public class BaseArchitecturalTests
{
    protected ArchUnitNET.Domain.Architecture Architecture;

    public BaseArchitecturalTests()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        foreach (var dll in Directory.GetFiles(path, "Zajednica.*.dll"))
            Assembly.LoadFile(dll);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        Architecture = new ArchLoader().LoadAssemblies(assemblies
            .Where(a => a.FullName!.StartsWith("Zajednica"))
            .Select(a => Assembly.Load(a.FullName!))
            .ToArray()
        ).Build();
    }

    protected IEnumerable<IType> GetExaminedTypes(string assemblyName) =>
        Architecture.Assemblies
            .Where(a => Regex.IsMatch(a.FullName, assemblyName))
            .SelectMany(a => Architecture.Types.Where(c => c.Assembly.Equals(a)));

    protected IEnumerable<IType> GetForbiddenTypes(params string[] exemptAssemblyNames) =>
        Architecture.Assemblies
            .Where(a => exemptAssemblyNames.All(n => !Regex.IsMatch(a.FullName, n)))
            .SelectMany(a => Architecture.Types.Where(c => c.Assembly.Equals(a)));
}
