using System.Globalization;

namespace MarsRover.Core;

/// <summary>Wire format of the rover's replies, one line per received command sequence.</summary>
public static class Protocol
{
    public static string Format(Report report) => report.Obstacle is { } o
        ? $"OBSTACLE {Format(report.Rover)} {o.X} {o.Y}"
        : $"OK {Format(report.Rover)}";

    /// <summary>A command sequence travels as exactly one line, so embedded line breaks are dropped.</summary>
    public static string AsSingleLine(string commands) =>
        string.Concat(commands.Where(c => c is not ('\r' or '\n')));

    private static string Format(Rover rover) =>
        string.Create(CultureInfo.InvariantCulture, $"{rover.Position.X} {rover.Position.Y} {rover.Facing}");
}
