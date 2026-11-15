module FSharp.MarsRover.Core.Navigation

open System

let private toCommand c =
    match Char.ToLowerInvariant c with
    | 'f' -> Some Forward
    | 'b' -> Some Backward
    | 'l' -> Some Left
    | 'r' -> Some Right
    | _ -> None

let parse (input: string) = input |> Seq.choose toCommand |> List.ofSeq

let private turnLeft = function
    | N -> W
    | W -> S
    | S -> E
    | E -> N

let private turnRight = function
    | N -> E
    | E -> S
    | S -> W
    | W -> N

let private step distance { X = x; Y = y } = function
    | N -> { X = x; Y = y + distance }
    | S -> { X = x; Y = y - distance }
    | E -> { X = x + distance; Y = y }
    | W -> { X = x - distance; Y = y }

let private move grid distance rover =
    let target = step distance rover.Position rover.Facing |> Grid.wrap grid

    if target |> Grid.isBlocked grid then
        Blocked (rover, target)
    else
        Completed { rover with Position = target }

let private apply grid rover = function
    | Left -> Completed { rover with Facing = turnLeft rover.Facing }
    | Right -> Completed { rover with Facing = turnRight rover.Facing }
    | Forward -> move grid 1 rover
    | Backward -> move grid -1 rover

/// Runs a command sequence, stopping at the first obstacle.
let execute grid rover input =
    let rec run rover = function
        | [] -> Completed rover
        | command :: rest ->
            match apply grid rover command with
            | Completed rover -> run rover rest
            | blocked -> blocked

    input |> parse |> run rover

/// Each sequence continues from where the previous one ended, even after an obstacle.
let executeAll grid start inputs =
    inputs
    |> Seq.scan (fun report input -> execute grid (Report.rover report) input) (Completed start)
    |> Seq.skip 1
