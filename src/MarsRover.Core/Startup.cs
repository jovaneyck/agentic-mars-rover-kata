using System.Globalization;

namespace MarsRover.Core;

public abstract record Startup
{
    public sealed record Valid(Grid Grid, Rover Rover) : Startup;

    public sealed record Invalid(string Error) : Startup;

    public const string Usage = "usage: rover [x y N|E|S|W [ox,oy ...]]";

    /// <summary>
    /// No arguments: start at 0 0 N on an empty 100x100 grid.
    /// Otherwise: start position and heading, followed by any number of obstacles as "x,y".
    /// </summary>
    public static Startup Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
            return new Valid(Grid.WithDefaultSize([]), new Rover(new Position(0, 0), Direction.N));
        if (args.Count < 3)
            return new Invalid(Usage);

        var position = ParsePosition(args[0], args[1]);
        var facing = ParseDirection(args[2]);
        var obstacles = args.Skip(3).Select(ParseObstacle).ToList();

        if (position is null)
            return new Invalid($"invalid start position '{args[0]} {args[1]}'. {Usage}");
        if (facing is null)
            return new Invalid($"invalid direction '{args[2]}'. {Usage}");
        if (obstacles.FindIndex(o => o is null) is var bad and >= 0)
            return new Invalid($"invalid obstacle '{args[3 + bad]}'. {Usage}");

        var grid = new Grid(Grid.DefaultSize, Grid.DefaultSize, []);
        var outside = obstacles.Select(o => o!.Value).Append(position.Value).Where(p => !grid.Contains(p)).ToList();
        if (outside.Count > 0)
            return new Invalid($"position {outside[0].X},{outside[0].Y} is outside the {grid.Width}x{grid.Height} grid");
        if (obstacles.Contains(position))
            return new Invalid($"rover cannot start on an obstacle at {position.Value.X},{position.Value.Y}");

        return new Valid(Grid.WithDefaultSize(obstacles.Select(o => o!.Value)), new Rover(position.Value, facing.Value));
    }

    private static Position? ParsePosition(string x, string y) =>
        TryParseInt(x) is { } px && TryParseInt(y) is { } py ? new Position(px, py) : null;

    private static Position? ParseObstacle(string arg) =>
        arg.Split(',') is [var x, var y] ? ParsePosition(x, y) : null;

    private static Direction? ParseDirection(string arg) => arg.ToUpperInvariant() switch
    {
        "N" => Direction.N,
        "E" => Direction.E,
        "S" => Direction.S,
        "W" => Direction.W,
        _ => null,
    };

    private static int? TryParseInt(string s) =>
        int.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var n) ? n : null;
}
