using MarsRover.Core;
using static MarsRover.Core.Direction;

namespace MarsRover.Tests;

public class NavigationTests
{
    private static readonly Grid Empty = Grid.WithDefaultSize([]);

    private static Rover At(int x, int y, Direction facing) => new(new Position(x, y), facing);

    private static Report Ok(int x, int y, Direction facing) => new(At(x, y, facing), null);

    [Theory]
    [InlineData(N, 5, 6)]
    [InlineData(E, 6, 5)]
    [InlineData(S, 5, 4)]
    [InlineData(W, 4, 5)]
    public void Forward_moves_one_square_in_the_facing_direction(Direction facing, int x, int y) =>
        Assert.Equal(Ok(x, y, facing), Navigation.Execute(Empty, At(5, 5, facing), "f"));

    [Theory]
    [InlineData(N, 5, 4)]
    [InlineData(E, 4, 5)]
    [InlineData(S, 5, 6)]
    [InlineData(W, 6, 5)]
    public void Backward_moves_one_square_away_from_the_facing_direction_without_turning(Direction facing, int x, int y) =>
        Assert.Equal(Ok(x, y, facing), Navigation.Execute(Empty, At(5, 5, facing), "b"));

    [Theory]
    [InlineData(N, "l", W)]
    [InlineData(W, "l", S)]
    [InlineData(S, "l", E)]
    [InlineData(E, "l", N)]
    [InlineData(N, "r", E)]
    [InlineData(E, "r", S)]
    [InlineData(S, "r", W)]
    [InlineData(W, "r", N)]
    public void Turning_changes_heading_only(Direction facing, string turn, Direction expected) =>
        Assert.Equal(Ok(5, 5, expected), Navigation.Execute(Empty, At(5, 5, facing), turn));

    [Theory]
    [InlineData(0, 99, N, "f", 0, 0)]
    [InlineData(0, 0, S, "f", 0, 99)]
    [InlineData(99, 0, E, "f", 0, 0)]
    [InlineData(0, 0, W, "f", 99, 0)]
    [InlineData(0, 0, N, "b", 0, 99)]
    [InlineData(99, 0, W, "b", 0, 0)]
    public void Wraps_around_the_edges_of_the_100x100_grid(int x, int y, Direction facing, string commands, int ex, int ey) =>
        Assert.Equal(Ok(ex, ey, facing), Navigation.Execute(Empty, At(x, y, facing), commands));

    [Fact]
    public void Executes_a_sequence_of_commands() =>
        Assert.Equal(Ok(2, 2, E), Navigation.Execute(Empty, At(0, 0, N), "ffrff"));

    [Fact]
    public void Commands_are_case_insensitive() =>
        Assert.Equal(Ok(2, 2, E), Navigation.Execute(Empty, At(0, 0, N), "FFRff"));

    [Theory]
    [InlineData("fxf rzff")]
    [InlineData("?f!f-r ff\t")]
    [InlineData("ffrff🚀")]
    public void Invalid_commands_are_ignored(string commands) =>
        Assert.Equal(Ok(2, 2, E), Navigation.Execute(Empty, At(0, 0, N), commands));

    [Fact]
    public void Empty_sequence_leaves_the_rover_in_place() =>
        Assert.Equal(Ok(3, 4, S), Navigation.Execute(Empty, At(3, 4, S), ""));

    [Fact]
    public void Stops_at_the_last_free_square_and_reports_the_obstacle_aborting_the_rest()
    {
        var grid = Grid.WithDefaultSize([new Position(0, 3)]);

        var report = Navigation.Execute(grid, At(0, 0, N), "fffffrf");

        Assert.Equal(new Report(At(0, 2, N), new Position(0, 3)), report);
    }

    [Fact]
    public void Detects_obstacles_across_the_wrapping_edge()
    {
        var grid = Grid.WithDefaultSize([new Position(0, 99)]);

        Assert.Equal(new Report(At(0, 0, N), new Position(0, 99)), Navigation.Execute(grid, At(0, 0, N), "b"));
    }

    [Fact]
    public void Turning_is_never_blocked_by_an_adjacent_obstacle()
    {
        var grid = Grid.WithDefaultSize([new Position(0, 1)]);

        Assert.Equal(Ok(0, 0, E), Navigation.Execute(grid, At(0, 0, N), "r"));
    }

    [Fact]
    public void Each_sequence_continues_from_where_the_previous_one_ended_even_after_an_obstacle()
    {
        var grid = Grid.WithDefaultSize([new Position(0, 2)]);

        var reports = Navigation.ExecuteAll(grid, At(0, 0, N), ["fff", "rf", "lff"]);

        Assert.Equal(
            [
                new Report(At(0, 1, N), new Position(0, 2)),
                Ok(1, 1, E),
                Ok(1, 3, N),
            ],
            reports);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 5)]
    public void A_grid_must_have_a_positive_size(int width, int height) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Grid(width, height, []));

    [Fact]
    public void Obstacles_must_lie_inside_the_grid() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Grid(10, 10, [new Position(10, 0)]));
}
