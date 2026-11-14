using MarsRover.Core;

namespace MarsRover.CommandCenterApp.Adapters;

public sealed class ArgsCommandSource(string[] args) : ICommandSource
{
    public IEnumerable<string> Commands() => args;
}

public sealed class StdoutCommandTransmitter(TextWriter output) : ICommandTransmitter
{
    public void Transmit(string commands)
    {
        output.WriteLine(commands);
        output.Flush();
    }
}
