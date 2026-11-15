# Containers: Mars Rover

There are two independent executables, connected by a one-way OS pipe (`CommandCenterApp | RoverApp`). Both use the shared `MarsRover.Core` library. Core is a library, not a deployable unit, so it appears in [c4-components.md](c4-components.md) and not here.

```mermaid
C4Container
  title Container diagram for Mars Rover

  Person(operator, "Operator", "Mission control on Earth")

  System_Boundary(marsRover, "Mars Rover") {
    Container(commandCenter, "Command Center", ".NET 10 console app", "Turns each CLI argument into one command line on stdout")
    Container(rover, "Rover", ".NET 10 console app", "Runs command lines from stdin and replies with one status line each")
  }

  Rel(operator, commandCenter, "Passes command sequences to", "CLI args")
  Rel(operator, rover, "Starts with start position and obstacles", "CLI args")
  Rel(commandCenter, rover, "Sends one sequence per line to", "stdout | stdin pipe")
  Rel(rover, operator, "Replies OK / OBSTACLE to", "stdout")

  UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
```

The wire format is described in the [README](../../README.md#protocol).

## Two interchangeable implementations

Each container exists twice: once in C# (`MarsRover.*`, built on `MarsRover.Core`) and once in F# (`FSharp.MarsRover.*`, built on `FSharp.MarsRover.Core`). Both speak the same protocol, so you can mix them on either side of the pipe, for example a C# command center piped into an F# rover. The components inside each container are documented separately: [C#](c4-components.md) and [F#](c4-components-fsharp.md).
