# Mars Rover

The [Mars Rover kata](https://kata-log.rocks/mars-rover-kata) in functional, hexagonal C#. A **command center** and a **rover** run as two independent processes that talk to each other over a stdin/stdout pipe.

```
operator ──args──▶ CommandCenterApp ──stdout │ stdin──▶ RoverApp ──stdout──▶ operator
```

## Quick start

From the repository root, in PowerShell:

```powershell
dotnet build MarsRover.slnx

$cc    = ".\src\MarsRover.CommandCenterApp\bin\Debug\net10.0\MarsRover.CommandCenterApp.exe"
$rover = ".\src\MarsRover.RoverApp\bin\Debug\net10.0\MarsRover.RoverApp.exe"

& $cc ffrff bLff "x?!" fff | & $rover 0 0 N 1,3
```

Output:

```
OK 2 2 E
OBSTACLE 1 2 N 1 3
OK 1 2 N
OBSTACLE 1 2 N 1 3
```

> Don't put `dotnet run` on the left side of the pipe. Any build output it prints would go down the pipe and the rover would read it as commands. Run the built executables instead, or use `dotnet <path>.dll`.

## The apps

### Command center: `MarsRover.CommandCenterApp`

```
MarsRover.CommandCenterApp <sequence> [<sequence> ...]
```

- Each argument is one **command sequence**. It is written to stdout as exactly one line.
- Line breaks inside an argument are removed, so one argument can never turn into two sequences.
- The command center does not validate commands. The rover ignores anything it doesn't understand.
- It exits with code 0 once every sequence has been sent.

### Rover: `MarsRover.RoverApp`

```
MarsRover.RoverApp [x y N|E|S|W [ox,oy ...]]
```

| Arguments | Meaning |
|-----------|---------|
| *(none)* | Start at `0 0 N` with no obstacles. |
| `x y D` | Start at position `(x, y)`, facing `D`. `D` is case-insensitive. |
| `ox,oy ...` | Obstacles. These can only be given after an explicit start position. |

- The grid is **100×100**, with coordinates from `0` to `99`.
- The grid **wraps** at every edge, like a torus: moving north from `y = 99` lands on `y = 0`.
- The rover reads command sequences from stdin, one per line, until end-of-file, and answers each sequence with **one reply line** on stdout.
- The rover keeps its state between sequences. Each sequence continues from where the previous one ended.
- **Startup errors** (exit code `1`, message on stderr; the rover never starts):
  - non-numeric coordinates or an unknown heading;
  - a start position or obstacle outside the grid;
  - starting on an obstacle;
  - the wrong number of arguments.

You can also run the rover by itself and type sequences by hand. End input with `Ctrl+Z` then `Enter` on Windows, or `Ctrl+D` on Linux and macOS.

```powershell
& $rover 5 5 E
```

## Protocol

The protocol is plain text and line-based. Lines end with the platform's newline.

### Command center → rover (rover stdin)

Each line is one command sequence. Each character is one command:

| Command | Effect |
|---------|--------|
| `f` | Move one square forward in the direction the rover is facing. |
| `b` | Move one square backward. The heading does not change. |
| `l` | Turn 90° left. |
| `r` | Turn 90° right. |

- Commands are **case-insensitive**.
- Any other character is **ignored**: spaces, punctuation, unicode, and so on.
- An empty line is a valid sequence that does nothing.
- Directions: `N` is `y + 1`, `E` is `x + 1`, `S` is `y − 1`, `W` is `x − 1`.

### Rover → operator (rover stdout)

The rover writes exactly one reply for each received sequence.

| Reply | Meaning |
|-------|---------|
| `OK <x> <y> <D>` | The whole sequence ran. The reply gives the final position and heading. |
| `OBSTACLE <x> <y> <D> <ox> <oy>` | The next move would have hit the obstacle at `(ox, oy)`. The rover stayed on the last free square `(x, y)` with heading `D`, and the rest of the sequence was dropped. |

**Obstacle detection** happens before each move, including moves that wrap across an edge. Turning is never blocked.

### Example session

The rover starts with `0 0 N 1,3`, so there is an obstacle at `(1, 3)`:

| Sent | Reply | Why |
|------|-------|-----|
| `ffrff` | `OK 2 2 E` | Moves north twice, turns east, moves twice. |
| `bLff` | `OBSTACLE 1 2 N 1 3` | Moves back to `(1,2)`, turns north. The first `f` would hit `(1,3)`. |
| `x?!` | `OK 1 2 N` | Not commands, so nothing happens. |
| `fff` | `OBSTACLE 1 2 N 1 3` | Still blocked. |

## Architecture

C4 diagrams: [context](docs/architecture/c4-context.md), [containers](docs/architecture/c4-containers.md), [components (hexagonal)](docs/architecture/c4-components.md).

The code uses hexagonal architecture (ports and adapters). All I/O stays at the edges.

| Project | Contents |
|---------|----------|
| `src/MarsRover.Core` | Pure domain (`Navigation`, `Grid`), `Protocol`, `Startup` argument parsing, ports (`ICommandSource`, `ICommandTransmitter`, `IStatusReporter`) and the application services `RoverController` and `CommandCenter`. |
| `src/MarsRover.RoverApp` | stdin and stdout adapters, plus the composition root. |
| `src/MarsRover.CommandCenterApp` | Command-line argument and stdout adapters, plus the composition root. |
| `tests/MarsRover.Tests` | Example tests, FsCheck property tests, in-memory adapter tests, and one end-to-end test. |

## Tests

```powershell
dotnet test --solution MarsRover.slnx
```

- Most tests run in memory against the core and fake adapters.
- `EndToEndTests` starts both executables as real processes and pipes the command center's stdout into the rover's stdin.

The design decisions taken during implementation are listed in [decisions.md](decisions.md).
