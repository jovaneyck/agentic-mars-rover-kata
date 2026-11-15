# Components: Mars Rover, F# implementation (hexagonal)

The F# version has the same hexagons, ports and wire protocol as the [C# version](c4-components.md), so the two can be swapped at the [container](c4-containers.md) level. The main difference is that there are **no classes or interfaces**:

- **Ports are function types.** A driving port is a `seq<string>` of command sequences, read lazily. A driven port is a function such as `Report -> unit` or `string -> unit`.
- **Application services are curried functions.** Adapters come first, so each composition root (`Program.fs`) wires in its I/O by partial application. Tests pass `ResizeArray.Add` instead of the stdout adapters.
- **Validation returns `Result`.** `Grid.create` and `Startup.parse` return `Result<_, string>`, and the composition root matches on `Ok`/`Error`. Nothing throws.
- **`Report` is a discriminated union.** `Completed of Rover | Blocked of Rover * obstacle: Position` replaces the record with a nullable obstacle.

Dependencies always point inwards, from adapters to `FSharp.MarsRover.Core`. The diagrams use the same layered flowchart style and colours as the [C# components](c4-components.md).

## Rover

```mermaid
flowchart LR
  cc(["<b>Command Center</b><br/>other process"]):::external
  op(["<b>Operator</b>"]):::person

  subgraph rover["Rover process: FSharp.MarsRover.RoverApp"]
    subgraph driving["Driving adapters"]
      stdin["<b>Adapters.stdinCommands</b><br/><i>TextReader -> string seq</i><br/>reads stdin lazily,<br/>one sequence per line"]:::adapter
    end
    subgraph core["FSharp.MarsRover.Core"]
      startup["<b>Startup.parse</b><br/><i>pure</i><br/>CLI args to Ok (Grid, Rover)<br/>or Error"]:::core
      controller["<b>RoverController.run</b><br/><i>application service</i><br/>report -> grid -> start -> commands<br/>keeps rover state between sequences"]:::core
      navigation["<b>Navigation + Grid</b><br/><i>pure domain</i><br/>f/b/l/r, wrapping, obstacles,<br/>executeAll via Seq.scan"]:::core
      protocol["<b>Protocol.format</b><br/><i>pure</i><br/>Report to<br/>OK / OBSTACLE line"]:::core
    end
    subgraph driven["Driven adapters"]
      stdout["<b>Adapters.stdoutReporter</b><br/><i>TextWriter -> Report -> unit</i><br/>writes one reply<br/>line to stdout"]:::adapter
    end
  end

  cc -- "stdin pipe" --> stdin
  stdin -- "string seq" --> controller
  stdin ~~~ startup
  startup -- "Ok (grid, rover)" --> controller
  controller -- "executeAll" --> navigation
  navigation ~~~ stdout
  controller -- "partially applied<br/>report function" --> stdout
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

  subgraph commandCenter["Command Center process: FSharp.MarsRover.CommandCenterApp"]
    subgraph driving["Driving adapters"]
      args["<b>argv</b><br/><i>string array as string seq</i><br/>CLI args are the sequences,<br/>no wrapper needed"]:::adapter
    end
    subgraph core["FSharp.MarsRover.Core"]
      service["<b>CommandCenter.run</b><br/><i>application service</i><br/>transmit -> commands -> unit"]:::core
      protocol["<b>Protocol.asSingleLine</b><br/><i>pure</i><br/>strips line breaks:<br/>one sequence = one line"]:::core
    end
    subgraph driven["Driven adapters"]
      transmitter["<b>Adapters.stdoutTransmitter</b><br/><i>TextWriter -> string -> unit</i><br/>writes the line<br/>to stdout"]:::adapter
    end
  end

  op -- "CLI args" --> args
  args -- "string seq" --> service
  service -- "Seq.map" --> protocol
  protocol ~~~ transmitter
  service -- "partially applied<br/>transmit function" --> transmitter
  transmitter -- "stdout pipe" --> rover

  classDef person fill:#08427b,stroke:#052e56,color:#fff
  classDef external fill:#999,stroke:#6b6b6b,color:#fff
  classDef adapter fill:#85bbf0,stroke:#5d82a8,color:#000
  classDef core fill:#1168bd,stroke:#0b4884,color:#fff
```

## Core module map

```mermaid
flowchart LR
  subgraph Core["FSharp.MarsRover.Core (compile order, left to right)"]
    Domain["Domain.fs<br/>Direction, Command, Position, Rover,<br/>Report DU, Grid (private) + Grid module"]
    Navigation["Navigation.fs<br/>parse, execute, executeAll"]
    Protocol["Protocol.fs<br/>format, asSingleLine"]
    Startup["Startup.fs<br/>parse : string list -> Result"]
    Application["Application.fs<br/>RoverController.run, CommandCenter.run"]
    Domain --> Navigation --> Protocol --> Startup --> Application
  end
  RoverApp["FSharp.MarsRover.RoverApp<br/>Adapters.fs, Program.fs"] --> Core
  CommandCenterApp["FSharp.MarsRover.CommandCenterApp<br/>Adapters.fs, Program.fs"] --> Core
```

F# compiles files in order, so each file can only use the files before it. The arrows show that ordering, not necessarily a direct call.

In tests, the adapters are replaced by `ResizeArray.Add` and in-memory lists (`ApplicationTests.fs`). Only `EndToEndTests.fs` uses the real console adapters and processes.
