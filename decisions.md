# Decisions to proofread

Decisions taken during implementation that were not explicitly settled in the grilling session.
Settled by you: wrapping at edges (torus), default 100x100, ignore invalid commands, independent processes,
one rover, command-center input via args, plain-text protocol, in-memory hex tests + one end-to-end test.

## Domain

1. **Coordinates are 0-based**: `x, y ∈ [0, 100)`. `N` = `y+1`, `E` = `x+1`, `S` = `y-1`, `W` = `x-1`.
2. **`b` moves backwards without turning** (heading unchanged).
3. **Commands are case-insensitive** (`F` == `f`). Every other character, including whitespace and unicode, is a silent no-op.
4. **Obstacle handling**: the rover stays on the last free square, the remaining commands of *that* sequence are dropped,
   and the report includes both the rover's final position/heading and the obstacle's coordinates.
   The next sequence continues from where the rover stopped (state is kept for the lifetime of the rover process).
5. **Turning is never blocked**; only moves check for obstacles. Obstacles across a wrapped edge are detected.
6. The core `Grid` supports any positive size; the rover process always uses 100x100 (no CLI flag for size — YAGNI).

## Processes & protocol

7. **Topology**: `CommandCenterApp` and `RoverApp` are fully independent executables connected by a pipe:
   `MarsRover.CommandCenterApp.exe ffrff bLff | MarsRover.RoverApp.exe 0 0 N 1,3`.
   This is one-directional (CC → rover); the rover's replies go to its own stdout (the operator/test reads them).
   A two-way channel would need the CC to spawn the rover or named pipes; neither was requested.
8. **Command center**: each argument is one command sequence and becomes exactly one line on stdout.
   Embedded line breaks are stripped so one argument can never turn into two sequences. The CC does no validation (the rover ignores junk).
9. **Rover replies**: one line per received sequence:
   - `OK <x> <y> <dir>`
   - `OBSTACLE <x> <y> <dir> <obstacleX> <obstacleY>`
10. **Rover startup args**: `rover [x y N|E|S|W [ox,oy ...]]`. No args → `0 0 N`, no obstacles.
    Invalid args (bad numbers/heading, outside the grid, starting on an obstacle) → message on stderr, exit code 1, rover never starts.
    Obstacles can only be given together with an explicit start position.

## Code structure

11. **Hello example removed** and replaced by `MarsRover.slnx`:
    - `src/MarsRover.Core` — domain (`Navigation`, `Grid`), `Protocol`, `Startup` parsing, ports (`ICommandSource`,
      `ICommandTransmitter`, `IStatusReporter`) and the two application services (`RoverController`, `CommandCenter`). No I/O.
    - `src/MarsRover.RoverApp` — stdin/stdout adapters + composition root.
    - `src/MarsRover.CommandCenterApp` — args/stdout adapters + composition root.
    - `tests/MarsRover.Tests` — examples, FsCheck properties, in-memory adapter tests, one end-to-end test.
12. `Protocol.Format` lives in core (shared wire language); the stdout adapter calls it.
13. Functional style: immutable records/structs, pure `Navigation.Execute` (a fold over commands), side effects only in adapters.

## Properties (FsCheck, generated grids ≤ 12x12 so wrapping/obstacles are hit often)

| # | Pattern | Property | EDFH cheat it blocks |
|---|---------|----------|----------------------|
| 1 | Inverse | Running a sequence then its reversed undo (`f↔b`, `l↔r`) returns to the start | wrong wrapping/backward direction (identity still passes → see 2) |
| 2 | Oracle | `n` × `f` equals modular arithmetic on the facing axis | identity / no-op rover |
| 3 | Invariant | Rover always ends inside the grid and never on an obstacle | ignoring obstacles, broken modulo |
| 4 | Easy to verify | A reported obstacle is a real obstacle exactly one step ahead/behind | inventing obstacles |
| 5 | Induction | `exec(a+b) == exec(b, from exec(a))`, or `exec(a)` if `a` hit an obstacle | not aborting after an obstacle |
| 6 | Same destination | Sprinkling non-command characters changes nothing | treating junk as commands |
| 7 | Robustness | Any unicode text executes without throwing | crashes on odd input |
| 8 | Robustness | Any startup args yield `Valid`/`Invalid`, never an exception | crashes on odd CLI input |

Each property was checked against deliberate mutations (broken modulo, no abort on obstacle, `b` moving forward); all were caught.
