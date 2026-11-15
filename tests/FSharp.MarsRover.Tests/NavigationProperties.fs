/// Generated worlds are small (at most 12x12) so wrapping and obstacle hits happen often.
module FSharp.MarsRover.Tests.NavigationProperties

open System
open FsCheck
open FsCheck.Xunit
open FSharp.MarsRover.Core

let private maxSize = 12

let private size (PositiveInt n) = 1 + (n - 1) % maxSize

let private modulo size value = ((value % size) + size) % size

let private world w h x y facing (obstacles: (int * int) array) =
    let width, height = size w, size h
    let start = { Position = { X = x |> modulo width; Y = y |> modulo height }; Facing = facing }

    let grid =
        obstacles
        |> Array.map (fun (ox, oy) -> { X = ox |> modulo width; Y = oy |> modulo height })
        |> Array.filter ((<>) start.Position)
        |> gridOf width height

    grid, start

let private asText (commands: Command list) =
    commands
    |> List.map (function
        | Forward -> "f"
        | Backward -> "b"
        | Left -> "l"
        | Right -> "r")
    |> String.concat ""

let private undo = function
    | Forward -> Backward
    | Backward -> Forward
    | Left -> Right
    | Right -> Left

let private emptyLike grid = gridOf (Grid.width grid) (Grid.height grid) []

// Pattern 2, there and back again: undoing every command in reverse order brings the rover home.
[<Property>]
let ``Undoing a sequence returns to the start`` w h x y facing (commands: Command list) =
    let grid, start = world w h x y facing [||]
    let roundTrip = commands @ (commands |> List.rev |> List.map undo)

    Navigation.execute grid start (asText roundTrip) = Completed start

// Pattern 7, test oracle: n forward moves equal plain modular arithmetic on the facing axis.
[<Property>]
let ``Moving forward n times is modular arithmetic`` w h x y facing (NonNegativeInt n) =
    let grid, start = world w h x y facing [||]
    let steps = n % 50
    let dx, dy =
        match facing with
        | N -> 0, 1
        | S -> 0, -1
        | E -> 1, 0
        | W -> -1, 0
    let expected =
        { X = start.Position.X + dx * steps |> modulo (Grid.width grid)
          Y = start.Position.Y + dy * steps |> modulo (Grid.height grid) }

    Navigation.execute grid start (String.replicate steps "f") = Completed { start with Position = expected }

// Pattern 3, invariant: the rover never leaves the grid and never stands on an obstacle.
[<Property>]
let ``Rover stays on the grid and off obstacles`` w h x y facing obstacles commands =
    let grid, start = world w h x y facing obstacles
    let final = (Navigation.execute grid start (asText commands) |> Report.rover).Position

    Grid.contains grid final && not (Grid.isBlocked grid final)

// Pattern 6, easy to verify: a reported obstacle is real and sits exactly one step ahead of or behind the rover.
[<Property>]
let ``Reported obstacle is adjacent along the heading`` w h x y facing obstacles commands =
    let grid, start = world w h x y facing obstacles

    match Navigation.execute grid start (asText commands) with
    | Completed _ -> true
    | Blocked (rover, obstacle) ->
        let after command = (Navigation.execute (emptyLike grid) rover command |> Report.rover).Position
        Grid.isBlocked grid obstacle && (obstacle = after "f" || obstacle = after "b")

// Pattern 5, structural induction: running a+b equals running a, then b from where a left off,
// unless a hit an obstacle, in which case b is never executed.
[<Property>]
let ``Sequences compose`` w h x y facing obstacles a b =
    let grid, start = world w h x y facing obstacles
    let first = Navigation.execute grid start (asText a)
    let whole = Navigation.execute grid start (asText a + asText b)

    whole =
        match first with
        | Completed rover -> Navigation.execute grid rover (asText b)
        | blocked -> blocked

// Pattern 1, different paths same destination: sprinkling non-command characters changes nothing.
[<Property>]
let ``Non command characters are ignored`` w h x y facing obstacles commands (noise: char array) =
    let grid, start = world w h x y facing obstacles
    let junk = noise |> Array.filter (fun c -> not ("fblrFBLR".Contains c)) |> String
    let clean = asText commands
    let noisy = (clean |> Seq.map (fun c -> junk + string c) |> String.concat "") + junk

    Navigation.execute grid start noisy = Navigation.execute grid start clean

// Pattern 8, throw everything at it: any text is a legal command sequence and never throws.
[<Property(MaxTest = 1000)>]
let ``Any text is executed without throwing`` w h x y facing obstacles (input: char array) =
    let grid, start = world w h x y facing obstacles

    Grid.contains grid (Navigation.execute grid start (String input) |> Report.rover).Position
