using System.Diagnostics;

namespace MarsRover.Core;

public static class Navigation
{
    public static IEnumerable<Command> Parse(string input) =>
        input.Select(ToCommand).Where(c => c.HasValue).Select(c => c!.Value);

    public static Report Execute(Grid grid, Rover rover, string input) =>
        Parse(input).Aggregate(
            new Report(rover, null),
            (report, command) => report.Obstacle is null ? Apply(grid, report.Rover, command) : report);

    public static IEnumerable<Report> ExecuteAll(Grid grid, Rover start, IEnumerable<string> inputs)
    {
        var rover = start;
        foreach (var input in inputs)
        {
            var report = Execute(grid, rover, input);
            rover = report.Rover;
            yield return report;
        }
    }

    private static Command? ToCommand(char c) => char.ToLowerInvariant(c) switch
    {
        'f' => Command.Forward,
        'b' => Command.Backward,
        'l' => Command.Left,
        'r' => Command.Right,
        _ => null,
    };

    private static Report Apply(Grid grid, Rover rover, Command command) => command switch
    {
        Command.Left => new Report(rover with { Facing = Turn(rover.Facing, -1) }, null),
        Command.Right => new Report(rover with { Facing = Turn(rover.Facing, +1) }, null),
        Command.Forward => Move(grid, rover, +1),
        Command.Backward => Move(grid, rover, -1),
        _ => throw new UnreachableException(),
    };

    private static Direction Turn(Direction facing, int quarterTurns) =>
        (Direction)(((int)facing + quarterTurns + 4) % 4);

    private static Report Move(Grid grid, Rover rover, int distance)
    {
        var target = grid.Wrap(Step(rover.Position, rover.Facing, distance));
        return grid.IsBlocked(target)
            ? new Report(rover, target)
            : new Report(rover with { Position = target }, null);
    }

    private static Position Step(Position p, Direction facing, int distance) => facing switch
    {
        Direction.N => p with { Y = p.Y + distance },
        Direction.S => p with { Y = p.Y - distance },
        Direction.E => p with { X = p.X + distance },
        Direction.W => p with { X = p.X - distance },
        _ => throw new UnreachableException(),
    };
}
