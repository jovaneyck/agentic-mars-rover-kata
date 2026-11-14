using MarsRover.Core;

namespace MarsRover.Tests;

/// <summary>Both application services wired to in-memory adapters, no processes involved.</summary>
public class ApplicationTests
{
    private sealed class InMemoryCommands(params string[] lines) : ICommandSource
    {
        public IEnumerable<string> Commands() => lines;
    }

    private sealed class RecordingTransmitter : ICommandTransmitter
    {
        public List<string> Sent { get; } = [];
        public void Transmit(string commands) => Sent.Add(commands);
    }

    private sealed class RecordingReporter : IStatusReporter
    {
        public List<string> Lines { get; } = [];
        public void Report(Report report) => Lines.Add(Protocol.Format(report));
    }

    [Fact]
    public void Command_center_transmits_each_operator_sequence_as_one_line()
    {
        var transmitter = new RecordingTransmitter();

        new CommandCenter(new InMemoryCommands("ffr", "b\nb"), transmitter).Run();

        Assert.Equal(["ffr", "bb"], transmitter.Sent);
    }

    [Fact]
    public void Rover_reports_once_per_received_sequence_keeping_state_between_them()
    {
        var reporter = new RecordingReporter();
        var grid = Grid.WithDefaultSize([new Position(1, 3)]);

        new RoverController(grid, new Rover(new Position(0, 0), Direction.N), new InMemoryCommands("ff", "rf", "lff", "", "zz"), reporter).Run();

        Assert.Equal(["OK 0 2 N", "OK 1 2 E", "OBSTACLE 1 2 N 1 3", "OK 1 2 N", "OK 1 2 N"], reporter.Lines);
    }

    [Fact]
    public void Command_center_drives_the_rover_through_in_memory_adapters()
    {
        var wire = new RecordingTransmitter();
        var reporter = new RecordingReporter();

        new CommandCenter(new InMemoryCommands("ffrff", "bbbb"), wire).Run();
        new RoverController(Grid.WithDefaultSize([]), new Rover(new Position(0, 0), Direction.N), new InMemoryCommands([.. wire.Sent]), reporter).Run();

        Assert.Equal(["OK 2 2 E", "OK 98 2 E"], reporter.Lines);
    }
}
