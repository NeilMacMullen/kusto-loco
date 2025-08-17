using Kusto.Language.Symbols;
using KustoLoco.Core.Console;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.PluginSupport;
using McMaster.NETCore.Plugins;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public class PluginHelper
{
    public static CommandProcessor LoadCommands(string path, IKustoConsole console, CommandProcessor processor)
    {
        var currentName = "";
        try
        {
            var plugins = Load<ILokqlCommand>(path, console);
            foreach (var instance in plugins)
            {
                currentName = instance.GetNameAndVersion();
                processor = (CommandProcessor)instance.Register(processor);
            }
        }
        catch (Exception ex)
        {
            console.Error($"Failed to register plugin '{currentName}'");
            console.Error($"Exception:{ex.GetType().Name} {ex.Message}");
        }

        return processor;
    }

    public static Dictionary<FunctionSymbol, ScalarFunctionInfo> LoadKqlFunctions(string path, IKustoConsole console)
    {
        var funcs = new Dictionary<FunctionSymbol, ScalarFunctionInfo>();
        var currentName = "";
        try
        {
            var plugins = Load<IKqlFunction>(path, console);
            foreach (var instance in plugins)
            {
                currentName = instance.GetNameAndVersion();
                instance.Register(funcs);
            }
        }
        catch (Exception ex)
        {
            console.Error($"Failed to register plugin '{currentName}'");
            console.Error($"Exception:{ex.GetType().Name} {ex.Message}");
        }

        return funcs;
    }

    private static List<T> Load<T>(string path, IKustoConsole console)
        where T : ILokqlPlugin
    {
        var currentName = "";
        try
        {
            var loaders = new List<PluginLoader>();
            var pluginFolders = Directory.EnumerateDirectories(path);
            foreach (var pluginFolder in pluginFolders)
            {
                var name = Path.GetFileName(pluginFolder);
                foreach (var extension in "dll".Tokenize())
                {
                    var assemblyFile = Path.Combine(pluginFolder, $"{name}.{extension}");
                    if (!File.Exists(assemblyFile)) continue;
                    console.Info($"Loading plugins from {assemblyFile}");

                    currentName = name;
                    var loader = PluginLoader.CreateFromAssemblyFile(
                        assemblyFile,
                        sharedTypes: [typeof(T)],
                        isUnloadable: false);
                    loaders.Add(loader);
                }
            }

            var instances = new List<T>();
            // Create an instance of plugin types
            foreach (var loader in loaders)
            foreach (var pluginType in loader
                         .LoadDefaultAssembly()
                         .GetTypes()
                         .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract))
            {
                currentName = pluginType.Name;
                // This assumes the implementation of IPlugin has a parameterless constructor
                var plugin = (T)Activator.CreateInstance(pluginType)!;
                console.Info($"Loaded {typeof(T).Name} plugin: {currentName} '{plugin.GetNameAndVersion()}'");
                instances.Add(plugin);
            }

            return instances;
        }
        catch (Exception ex)
        {
            console.Error($"Failed to load plugin '{currentName}': {ex.Message}");
            console.Error($"Exception:{ex.GetType().Name} {ex.Message}");
        }

        return [];
    }
}
