using System.Diagnostics;

namespace MarsRover.Tests;

/// <summary>
/// Starts both executables as independent processes and plays the shell pipe between them:
/// command center stdout -> rover stdin. The rover's stdout is what mission control sees.
/// </summary>
public class EndToEndTests
{
    private static Process Start(string assembly, IEnumerable<string> args, bool redirectInput)
    {
        var info = new ProcessStartInfo("dotnet")
        {
            RedirectStandardInput = redirectInput,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        info.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, assembly));
        foreach (var arg in args) info.ArgumentList.Add(arg);
        return Process.Start(info)!;
    }

    [Fact]
    public async Task Command_center_drives_the_rover_over_a_stdin_stdout_pipe()
    {
        var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        using var rover = Start("MarsRover.RoverApp.dll", ["0", "0", "N", "1,3"], redirectInput: true);
        using var commandCenter = Start("MarsRover.CommandCenterApp.dll", ["ffrff", "bLff", "x?!", "fff"], redirectInput: false);

        await commandCenter.StandardOutput.BaseStream.CopyToAsync(rover.StandardInput.BaseStream, cancel);
        rover.StandardInput.Close();
        var replies = await rover.StandardOutput.ReadToEndAsync(cancel);
        await Task.WhenAll(commandCenter.WaitForExitAsync(cancel), rover.WaitForExitAsync(cancel));

        Assert.Equal(0, commandCenter.ExitCode);
        Assert.Equal(0, rover.ExitCode);
        Assert.Equal(
            ["OK 2 2 E", "OBSTACLE 1 2 N 1 3", "OK 1 2 N", "OBSTACLE 1 2 N 1 3"],
            replies.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
    }
}
