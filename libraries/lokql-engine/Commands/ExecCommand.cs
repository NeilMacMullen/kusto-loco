using System.Diagnostics;
using System.Text;
using CommandLine;
using KustoLoco.PluginSupport;
using NotNullStrings;

namespace Lokql.Engine.Commands;

/// <summary>
///     Execute a command as a separate process and display the output
/// </summary>
public static class ExecCommand
{
    internal static async Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;

        var blocks = context.InputProcessor;
        var execLine = blocks.ConsumeNextBlock().Trim();
        if (execLine.IsBlank())
        {
            console.Warn("No command specified");
            return;
        }

        var exit = o.RunInTerminal
            ? o.UsePowerShell
                ? "-NoExit"
                : "/K"
            : o.UsePowerShell
                ? string.Empty
                : "/C";

        var shell = o.UsePowerShell ? "pwsh.exe" : "cmd.exe";
        // Use Base64 encoding to avoid escaping issues with PowerShell
        var encodedCommand = Convert.ToBase64String(Encoding.Unicode.GetBytes(execLine));

        var arguments = o.UsePowerShell
            ? $" {exit} -EncodedCommand {encodedCommand}"
            : $"{exit} {execLine}";

        try
        {
            if (o.RunInTerminal)
            {
                arguments = $"-w 0 new-tab  {shell} {arguments}";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "wt.exe",
                    Arguments = arguments,
                    UseShellExecute = true
                };

                using var process = Process.Start(startInfo);
            }
            else
            {
                // Capture output mode (existing behavior)
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                var startInfo = new ProcessStartInfo
                {
                    FileName = shell, //fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };

                // Capture output and error streams
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        errorBuilder.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                var exitCode = process.ExitCode;

                // Display output
                if (outputBuilder.Length > 0) console.Write(outputBuilder.ToString());

                // Display errors if any
                if (errorBuilder.Length > 0) console.Error(errorBuilder.ToString());

                if (exitCode != 0)
                    console.Warn($"Process exited with code: {exitCode}");
                else
                    console.Info($"Process completed successfully (exit code: {exitCode})");
            }
        }
        catch (Exception ex)
        {
            console.Error($"Error executing command: {ex.Message}");
        }
    }

    [Verb("exec", aliases: ["execute"],
        HelpText = """
                   Executes the following block of text as a separate process and display the output.
                   Normally the CMD.EXE shell is used but PWSH.EXE can be used using the -p option.
                   The command can be run in a windows terminal tab using the -t option
                   Examples:
                     .exec
                      echo "hello KQL"                              

                     .exec -p -t  # powershell script run in new windows terminal 
                      Get-Process | Where-Object {$_.CPU -gt 10}    
                      
                      
                     .set procname msedge #pass in a parameter
                     .exec -p -t 
                      Get-Process | Where-Object {$_.ProcessName -eq "$procname"}             
                      
                   """)]
    internal class Options
    {
        [Option('t',"wt", Required = false, HelpText = "Launch the command in a new Windows Terminal tab")]
        public bool RunInTerminal { get; set; }

        [Option('p',"pwsh", Required = false, HelpText = "Use PowerShell instead of CMD")]
        public bool UsePowerShell { get; set; }
    }
}
