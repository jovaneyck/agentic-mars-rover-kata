using MarsRover.CommandCenterApp.Adapters;
using MarsRover.Core;

new CommandCenter(new ArgsCommandSource(args), new StdoutCommandTransmitter(Console.Out)).Run();
