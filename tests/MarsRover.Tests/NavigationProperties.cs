using FsCheck;
using FsCheck.Xunit;
using MarsRover.Core;

namespace MarsRover.Tests;

/// <summary>Generated worlds are small (at most 12x12) so wrapping and obstacle hits happen often.</summary>
public class NavigationProperties
{
    private const int MaxSize = 12;

    private static int Size(PositiveInt n) => 1 + (n.Get - 1) % MaxSize;

    private static int Mod(int value, int size) => ((value % size) + size) % size;

    private static Rover Start(int width, int height, int x, int y, Direction facing) =>
        new(new Position(Mod(x, width), Mod(y, height)), facing);

    private static Grid GridAround(int width, int height, Rover start, (int X, int Y)[] obstacles) =>
        new(width, height, obstacles
            .Select(o => new Position(Mod(o.X, width), Mod(o.Y, height)))
            .Where(p => p != start.Position));

    private static string AsText(IEnumerable<Command> commands) => string.Concat(commands.Select(c => c switch
    {
        Command.Forward => 'f',
        Command.Backward => 'b',
        Command.Left => 'l',
        _ => 'r',
    }));

    private static Command Undo(Command c) => c switch
    {
        Command.Forward => Command.Backward,
        Command.Backward => Command.Forward,
        Command.Left => Command.Right,
        _ => Command.Left,
    };

    // Pattern 2, there and back again: undoing every command in reverse order brings the rover home.
    [Property]
    public bool Undoing_a_sequence_returns_to_the_start(
        PositiveInt w, PositiveInt h, int x, int y, Direction facing, Command[] commands)
    {
        var (width, height) = (Size(w), Size(h));
        var start = Start(width, height, x, y, facing);
        var roundTrip = commands.Concat(commands.Reverse().Select(Undo));

        return Navigation.Execute(new Grid(width, height, []), start, AsText(roundTrip)) == new Report(start, null);
    }

    // Pattern 7, test oracle: n forward moves equal plain modular arithmetic on the facing axis.
    [Property]
    public bool Moving_forward_n_times_is_modular_arithmetic(
        PositiveInt w, PositiveInt h, int x, int y, Direction facing, NonNegativeInt n)
    {
        var (width, height) = (Size(w), Size(h));
        var start = Start(width, height, x, y, facing);
        var steps = n.Get % 50;
        var (dx, dy) = facing switch
        {
            Direction.N => (0, 1),
            Direction.S => (0, -1),
            Direction.E => (1, 0),
            _ => (-1, 0),
        };
        var expected = new Position(
            Mod(start.Position.X + dx * steps, width),
            Mod(start.Position.Y + dy * steps, height));

        var report = Navigation.Execute(new Grid(width, height, []), start, new string('f', steps));

        return report == new Report(start with { Position = expected }, null);
    }

    // Pattern 3, invariant: the rover never leaves the grid and never stands on an obstacle.
    [Property]
    public bool Rover_stays_on_the_grid_and_off_obstacles(
        PositiveInt w, PositiveInt h, int x, int y, Direction facing, (int, int)[] obstacles, Command[] commands)
    {
        var (width, height) = (Size(w), Size(h));
        var start = Start(width, height, x, y, facing);
        var grid = GridAround(width, height, start, obstacles);

        var final = Navigation.Execute(grid, start, AsText(commands)).Rover.Position;

        return grid.Contains(final) && !grid.IsBlocked(final);
    }

    // Pattern 6, easy to verify: a reported obstacle is real and sits exactly one step ahead of or behind the rover.
    [Property]
    public bool Reported_obstacle_is_adjacent_along_the_heading(
        PositiveInt w, PositiveInt h, int x, int y, Direction facing, (int, int)[] obstacles, Command[] commands)
    {
        var (width, height) = (Size(w), Size(h));
        var start = Start(width, height, x, y, facing);
        var grid = GridAround(width, height, start, obstacles);

        var report = Navigation.Execute(grid, start, AsText(commands));
        if (report.Obstacle is not { } obstacle) return true;

        var ahead = Navigation.Execute(new Grid(width, height, []), report.Rover, "f").Rover.Position;
        var behind = Navigation.Execute(new Grid(width, height, []), report.Rover, "b").Rover.Position;
        return grid.IsBlocked(obstacle) && (obstacle == ahead || obstacle == behind);
    }

    // Pattern 5, structural induction: running a+b equals running a, then b from where a left off,
    // unless a hit an obstacle, in which case b is never executed.
    [Property]
    public bool Sequences_compose(
        PositiveInt w, PositiveInt h, int x, int y, Direction facing, (int, int)[] obstacles, Command[] a, Command[] b)
    {
        var (width, height) = (Size(w), Size(h));
        var start = Start(width, height, x, y, facing);
        var grid = GridAround(width, height, start, obstacles);

        var first = Navigation.Execute(grid, start, AsText(a));
        var whole = Navigation.Execute(grid, start, AsText(a) + AsText(b));

        return whole == (first.Obstacle is null ? Navigation.Execute(grid, first.Rover, AsText(b)) : first);
    }

    // Pattern 1, different paths same destination: sprinkling non-command characters changes nothing.
    [Property]
    public bool Non_command_characters_are_ignored(
        PositiveInt w, PositiveInt h, int x, int y, Direction facing, (int, int)[] obstacles, Command[] commands, NonNull<string> noise)
    {
        var (width, height) = (Size(w), Size(h));
        var start = Start(width, height, x, y, facing);
        var grid = GridAround(width, height, start, obstacles);
        var junk = new string(noise.Get.Where(c => "fblrFBLR".IndexOf(c) < 0).ToArray());
        var clean = AsText(commands);
        var noisy = string.Concat(clean.Select(c => junk + c)) + junk;

        return Navigation.Execute(grid, start, noisy) == Navigation.Execute(grid, start, clean);
    }

    // Pattern 8, throw everything at it: any text is a legal command sequence and never throws.
    [Property(MaxTest = 1000)]
    public bool Any_text_is_executed_without_throwing(
        PositiveInt w, PositiveInt h, int x, int y, Direction facing, (int, int)[] obstacles, NonNull<string> input)
    {
        var (width, height) = (Size(w), Size(h));
        var start = Start(width, height, x, y, facing);
        var grid = GridAround(width, height, start, obstacles);

        return grid.Contains(Navigation.Execute(grid, start, input.Get).Rover.Position);
    }
}
