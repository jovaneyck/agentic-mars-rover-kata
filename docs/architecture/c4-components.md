# Components: Mars Rover, C# implementation (hexagonal)

The F# implementation is described in [c4-components-fsharp.md](c4-components-fsharp.md).

Each process is a hexagon. Adapters at the edges do the I/O, and each process's composition root (`Program.cs`) wires them in. Ports and application services come from `MarsRover.Core`, together with the pure domain (`Navigation`, `Grid`), `Protocol` and `Startup`. Dependencies always point inwards, from adapters to core.

The diagrams are Mermaid flowcharts in C4 colours rather than `C4Component` diagrams. Mermaid's C4 renderer has no automatic layout, whereas flowcharts lay out each hexagon left to right: driving adapters, core, driven adapters. Light blue boxes are adapters, dark blue boxes are core, and grey boxes are the other process. Dotted arrows point to the adapter that uses a pure helper.

## Rover

```mermaid
flowchart LR
  cc(["<b>Command Center</b><br/>other process"]):::external
  op(["<b>Operator</b>"]):::person

  subgraph rover["Rover process: MarsRover.RoverApp"]
    subgraph driving["Driving adapters"]
      stdin["<b>StdinCommandSource</b><br/><i>ICommandSource</i><br/>reads stdin lazily,<br/>one sequence per line"]:::adapter
    end
    subgraph core["MarsRover.Core"]
      startup["<b>Startup</b><br/><i>pure</i><br/>CLI args to Grid + Rover,<br/>or an error"]:::core
      controller["<b>RoverController</b><br/><i>application service</i><br/>keeps rover state<br/>between sequences"]:::core
      navigation["<b>Navigation + Grid</b><br/><i>pure domain</i><br/>f/b/l/r, wrapping,<br/>obstacle detection"]:::core
      protocol["<b>Protocol</b><br/><i>pure</i><br/>Report to<br/>OK / OBSTACLE line"]:::core
    end
    subgraph driven["Driven adapters"]
      stdout["<b>StdoutStatusReporter</b><br/><i>IStatusReporter</i><br/>writes one reply<br/>line to stdout"]:::adapter
    end
  end

  cc -- "stdin pipe" --> stdin
  stdin -- "ICommandSource" --> controller
  stdin ~~~ startup
  startup -- "start Grid + Rover" --> controller
  controller -- "ExecuteAll" --> navigation
  navigation ~~~ stdout
  controller -- "IStatusReporter" --> stdout
  protocol -. "formats replies for" .-> stdout
  stdout -- "stdout" --> op

  classDef person fill:#08427b,stroke:#052e56,color:#fff
  classDef external fill:#999,stroke:#6b6b6b,color:#fff
  classDef adapter fill:#85bbf0,stroke:#5d82a8,color:#000
  classDef core fill:#1168bd,stroke:#0b4884,color:#fff
```

## Command Center

```mermaid
flowchart LR
  op(["<b>Operator</b>"]):::person
  rover(["<b>Rover</b><br/>other process"]):::external

  subgraph commandCenter["Command Center process: MarsRover.CommandCenterApp"]
    subgraph driving["Driving adapters"]
      args["<b>ArgsCommandSource</b><br/><i>ICommandSource</i><br/>exposes CLI args<br/>as sequences"]:::adapter
    end
    subgraph core["MarsRover.Core"]
      service["<b>CommandCenter</b><br/><i>application service</i><br/>sends each sequence<br/>as one line"]:::core
      protocol["<b>Protocol</b><br/><i>pure</i><br/>strips line breaks:<br/>one sequence = one line"]:::core
    end
    subgraph driven["Driven adapters"]
      transmitter["<b>StdoutCommandTransmitter</b><br/><i>ICommandTransmitter</i><br/>writes the line<br/>to stdout"]:::adapter
    end
  end

  op -- "CLI args" --> args
  args -- "ICommandSource" --> service
  service -- "AsSingleLine" --> protocol
  protocol ~~~ transmitter
  service -- "ICommandTransmitter" --> transmitter
  transmitter -- "stdout pipe" --> rover

  classDef person fill:#08427b,stroke:#052e56,color:#fff
  classDef external fill:#999,stroke:#6b6b6b,color:#fff
  classDef adapter fill:#85bbf0,stroke:#5d82a8,color:#000
  classDef core fill:#1168bd,stroke:#0b4884,color:#fff
```

In tests, the adapters are replaced by in-memory fakes (`ApplicationTests`). Only `EndToEndTests` uses the real console adapters and processes.
