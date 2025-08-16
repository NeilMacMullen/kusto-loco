using System.ComponentModel.Design.Serialization;
using KustoLoco.Core.Console;
using KustoLoco.PluginSupport;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Lokql.Engine.Commands;

public class PluginHelper
{

    public static CommandProcessor LoadCommands(string path, IKustoConsole console,CommandProcessor processor)
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
    private static List<T> Load<T>(string path,IKustoConsole console)
    where T:ILokqlPlugin
    {
        var currentName = "";
        try
        {
            var loaders = new List<PluginLoader>();
            var pluginFolders = Directory.EnumerateDirectories(path);
            foreach (var pluginFolder in pluginFolders)
            {
                var name = Path.GetFileName(pluginFolder);
                var dllPath = Path.Combine(pluginFolder, $"{name}.dll");
                if (!File.Exists(dllPath)) continue;
                currentName = name;
                var loader = PluginLoader.CreateFromAssemblyFile(
                    assemblyFile: dllPath,
                    sharedTypes: [typeof(T)],
                    isUnloadable: true);
                loaders.Add(loader);

            }

            var instances = new List<T>();
            // Create an instance of plugin types
            foreach (var loader in loaders)
            {
                foreach (var pluginType in loader
                             .LoadDefaultAssembly()
                             .GetTypes()
                             .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    currentName= pluginType.Name;
                    // This assumes the implementation of IPlugin has a parameterless constructor
                    var plugin = (T)Activator.CreateInstance(pluginType)!;
                    console.Info($"Loaded {typeof(T).Name} plugin: {currentName} '{plugin.GetNameAndVersion()}'");
                    instances.Add(plugin);
                }
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
