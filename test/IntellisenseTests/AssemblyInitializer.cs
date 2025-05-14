using System.Runtime.CompilerServices;
using Intellisense.FileSystem;

namespace IntellisenseTests;

public static class AssemblyInitializer
{
    [ModuleInitializer]
    public static void Initializer() => TestHelper.TestDirectory.CreateOrClean();
}
