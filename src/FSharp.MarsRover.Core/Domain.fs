namespace FSharp.MarsRover.Core

type Direction = N | E | S | W

type Command = Forward | Backward | Left | Right

type Position = { X: int; Y: int }

type Rover = { Position: Position; Facing: Direction }

type Report =
    | Completed of Rover
    | Blocked of Rover * obstacle: Position

module Report =
    let rover = function
        | Completed rover
        | Blocked (rover, _) -> rover

type Grid = private { Width: int; Height: int; Obstacles: Set<Position> }

module Grid =
    let defaultSize = 100

    let width grid = grid.Width
    let height grid = grid.Height
    let obstacles grid = grid.Obstacles

    let contains grid { X = x; Y = y } =
        0 <= x && x < grid.Width && 0 <= y && y < grid.Height

    let isBlocked grid position = grid.Obstacles |> Set.contains position

    let wrap grid { X = x; Y = y } =
        let modulo size value = ((value % size) + size) % size
        { X = x |> modulo grid.Width; Y = y |> modulo grid.Height }

    let outsideError grid { X = x; Y = y } =
        $"position {x},{y} is outside the {grid.Width}x{grid.Height} grid"

    let create width height obstacles =
        if width <= 0 || height <= 0 then
            Error $"grid size must be positive, got {width}x{height}"
        else
            let obstacles = List.ofSeq obstacles
            let grid = { Width = width; Height = height; Obstacles = Set.ofList obstacles }

            match obstacles |> List.tryFind (contains grid >> not) with
            | Some outside -> Error (outsideError grid outside)
            | None -> Ok grid
