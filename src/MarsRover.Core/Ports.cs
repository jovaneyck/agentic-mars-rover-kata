namespace MarsRover.Core;

/// <summary>Driving port: a stream of raw command sequences, e.g. "ffrl".</summary>
public interface ICommandSource
{
    IEnumerable<string> Commands();
}

/// <summary>Driven port of the command center: sends a command sequence to the rover.</summary>
public interface ICommandTransmitter
{
    void Transmit(string commands);
}

/// <summary>Driven port of the rover: publishes the outcome of a command sequence.</summary>
public interface IStatusReporter
{
    void Report(Report report);
}

public sealed class RoverController(Grid grid, Rover start, ICommandSource commands, IStatusReporter reporter)
{
    public void Run()
    {
        foreach (var report in Navigation.ExecuteAll(grid, start, commands.Commands()))
            reporter.Report(report);
    }
}

public sealed class CommandCenter(ICommandSource operatorInput, ICommandTransmitter transmitter)
{
    public void Run()
    {
        foreach (var commands in operatorInput.Commands())
            transmitter.Transmit(Protocol.AsSingleLine(commands));
    }
}
