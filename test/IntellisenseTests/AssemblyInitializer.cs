using System.Runtime.CompilerServices;

namespace IntellisenseTests;

public static class AssemblyInitializer
{
    [ModuleInitializer]
    public static void Initializer() => TestHelper.TestDirectory.CreateOrClean();
}
