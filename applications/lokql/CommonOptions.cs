using CommandLine;

public class CommonOptions
{
    [Option(HelpText = "Default folder to load/save data/results to")]
    public string Data { get; set; } = ".";

    [Option("args", HelpText = "Passes arguments to the script as arg0, arg1 etc")]
    public IEnumerable<string> Args { get; set; } = [];
    [Option('f', HelpText = "Runs a script or sequence of scripts")]
    public IEnumerable<string> Scripts { get; set; } = [];

    [Option('c', HelpText = "Runs a command or sequence of commands")]
    public IEnumerable<string> Commands { get; set; } = [];

    [Option('l', HelpText = "Loads data files")]
    public IEnumerable<string> Load { get; set; } = [];

}
