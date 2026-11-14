namespace MarsRover.Core;

public enum Direction { N, E, S, W }

public enum Command { Forward, Backward, Left, Right }

public readonly record struct Position(int X, int Y);

public readonly record struct Rover(Position Position, Direction Facing);

public sealed record Report(Rover Rover, Position? Obstacle);

public sealed class Grid
{
    public const int DefaultSize = 100;

    public Grid(int width, int height, IEnumerable<Position> obstacles)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        Width = width;
        Height = height;
        Obstacles = obstacles.ToHashSet();
        if (Obstacles.Any(o => !Contains(o)))
            throw new ArgumentOutOfRangeException(nameof(obstacles), "Obstacles must lie inside the grid.");
    }

    public static Grid WithDefaultSize(IEnumerable<Position> obstacles) => new(DefaultSize, DefaultSize, obstacles);

    public int Width { get; }
    public int Height { get; }
    public IReadOnlySet<Position> Obstacles { get; }

    public bool Contains(Position p) => p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;

    public bool IsBlocked(Position p) => Obstacles.Contains(p);

    public Position Wrap(Position p) => new(Modulo(p.X, Width), Modulo(p.Y, Height));

    private static int Modulo(int value, int size) => ((value % size) + size) % size;
}
