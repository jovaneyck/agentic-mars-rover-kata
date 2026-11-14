# Components: Mars Rover (hexagonal)

Each process is a hexagon. Adapters at the edges do the I/O, and each process's composition root (`Program.cs`) wires them in. Ports and application services come from `MarsRover.Core`, together with the pure domain (`Navigation`, `Grid`), `Protocol` and `Startup`. Dependencies always point inwards, from adapters to core.

## Rover

```mermaid
C4Component
  title Component diagram for the Rover process

  Container_Ext(commandCenter, "Command Center", ".NET console app", "Writes one command sequence per line")
  Person(operator, "Operator", "Reads rover replies")

  Container_Boundary(rover, "Rover (MarsRover.RoverApp)") {
    Component(stdin, "StdinCommandSource", "Driving adapter: ICommandSource", "Reads stdin lazily, one sequence per line")
    Component(startup, "Startup", "Core, pure", "Parses x y D ox,oy... into a Grid and a Rover, or an error")
    Component(controller, "RoverController", "Core application service", "Feeds sequences to Navigation and keeps rover state")
    Component(navigation, "Navigation + Grid", "Core domain, pure", "f/b/l/r, wrapping, obstacle detection")
    Component(protocol, "Protocol", "Core, pure", "Formats Report as OK / OBSTACLE line")
    Component(stdout, "StdoutStatusReporter", "Driven adapter: IStatusReporter", "Writes formatted report to stdout")
  }

  Rel(commandCenter, stdin, "Pipes sequences into", "stdin")
  Rel(startup, controller, "Provides start Grid and Rover to")
  Rel(stdin, controller, "Supplies sequences to", "ICommandSource")
  Rel(controller, navigation, "Executes sequences with", "ExecuteAll")
  Rel(controller, stdout, "Publishes Report through", "IStatusReporter")
  Rel(stdout, protocol, "Formats with")
  Rel(stdout, operator, "Writes replies to", "stdout")

  UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

## Command Center

```mermaid
C4Component
  title Component diagram for the Command Center process

  Person(operator, "Operator", "Passes command sequences")
  Container_Ext(rover, "Rover", ".NET console app", "Reads sequences from stdin")

  Container_Boundary(commandCenter, "Command Center (MarsRover.CommandCenterApp)") {
    Component(args, "ArgsCommandSource", "Driving adapter: ICommandSource", "Exposes CLI args as sequences")
    Component(service, "CommandCenter", "Core application service", "Sends each sequence as one line")
    Component(protocol, "Protocol", "Core, pure", "Strips line breaks: one sequence = one line")
    Component(transmitter, "StdoutCommandTransmitter", "Driven adapter: ICommandTransmitter", "Writes the line to stdout")
  }

  Rel(operator, args, "Passes sequences to", "CLI args")
  Rel(args, service, "Supplies sequences to", "ICommandSource")
  Rel(service, protocol, "Sanitizes with", "AsSingleLine")
  Rel(service, transmitter, "Transmits through", "ICommandTransmitter")
  Rel(transmitter, rover, "Pipes sequences into", "stdout | stdin")

  UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
```

In tests, the adapters are replaced by in-memory fakes (`ApplicationTests`). Only `EndToEndTests` uses the real console adapters and processes.
