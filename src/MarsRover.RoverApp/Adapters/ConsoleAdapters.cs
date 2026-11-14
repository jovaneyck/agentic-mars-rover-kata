using MarsRover.Core;

namespace MarsRover.RoverApp.Adapters;

public sealed class StdinCommandSource(TextReader input) : ICommandSource
{
    public IEnumerable<string> Commands()
    {
        while (input.ReadLine() is { } line)
            yield return line;
    }
}

public sealed class StdoutStatusReporter(TextWriter output) : IStatusReporter
{
    public void Report(Report report)
    {
        output.WriteLine(Protocol.Format(report));
        output.Flush();
    }
}
