using MarsRover.Core;
using MarsRover.RoverApp.Adapters;

return Startup.Parse(args) switch
{
    Startup.Valid(var grid, var rover) => Run(grid, rover),
    Startup.Invalid(var error) => Fail(error),
    _ => Fail(Startup.Usage),
};

static int Run(Grid grid, Rover rover)
{
    new RoverController(grid, rover, new StdinCommandSource(Console.In), new StdoutStatusReporter(Console.Out)).Run();
    return 0;
}

static int Fail(string error)
{
    Console.Error.WriteLine(error);
    return 1;
}
