using FsCheck;
using FsCheck.Xunit;
using MarsRover.Core;

namespace MarsRover.Tests;

public class StartupTests
{
    [Fact]
    public void Without_arguments_the_rover_starts_at_the_origin_facing_north_on_an_empty_100x100_grid()
    {
        var valid = Assert.IsType<Startup.Valid>(Startup.Parse([]));

        Assert.Equal(new Rover(new Position(0, 0), Direction.N), valid.Rover);
        Assert.Equal((100, 100), (valid.Grid.Width, valid.Grid.Height));
        Assert.Empty(valid.Grid.Obstacles);
    }

    [Fact]
    public void Parses_start_position_heading_and_obstacles()
    {
        var valid = Assert.IsType<Startup.Valid>(Startup.Parse(["3", "4", "w", "0,3", "99,99"]));

        Assert.Equal(new Rover(new Position(3, 4), Direction.W), valid.Rover);
        Assert.Equal([new Position(0, 3), new Position(99, 99)], valid.Grid.Obstacles.OrderBy(p => p.X));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1", "2")]
    [InlineData("x", "2", "N")]
    [InlineData("1", "2", "Q")]
    [InlineData("1", "2", "N", "3")]
    [InlineData("1", "2", "N", "3,a")]
    [InlineData("1", "2", "N", "3,4,5")]
    [InlineData("100", "0", "N")]
    [InlineData("0", "-1", "N")]
    [InlineData("0", "0", "N", "0,100")]
    [InlineData("2", "2", "N", "2,2")]
    public void Rejects_invalid_startup_arguments(params string[] args) =>
        Assert.IsType<Startup.Invalid>(Startup.Parse(args));

    // Pattern 8: whatever the operator passes on the command line, startup answers Valid or Invalid and never throws.
    [Property(MaxTest = 1000)]
    public bool Any_arguments_yield_a_verdict_without_throwing(NonNull<string>[] args) =>
        Startup.Parse(args.Select(a => a.Get).ToArray()) is Startup.Valid or Startup.Invalid;
}

public class ProtocolTests
{
    [Fact]
    public void Formats_a_completed_sequence() =>
        Assert.Equal("OK 2 3 E", Protocol.Format(new Report(new Rover(new Position(2, 3), Direction.E), null)));

    [Fact]
    public void Formats_an_obstacle_report_with_the_rover_position_followed_by_the_obstacle() =>
        Assert.Equal("OBSTACLE 0 2 N 0 3",
            Protocol.Format(new Report(new Rover(new Position(0, 2), Direction.N), new Position(0, 3))));

    [Fact]
    public void A_command_sequence_is_flattened_to_a_single_line() =>
        Assert.Equal("ffrl", Protocol.AsSingleLine("ff\r\nr\nl"));
}
