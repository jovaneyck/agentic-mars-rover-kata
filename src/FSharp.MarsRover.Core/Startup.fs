module FSharp.MarsRover.Core.Startup

open System
open System.Globalization

let usage = "usage: rover [x y N|E|S|W [ox,oy ...]]"

let private tryParseInt (s: string) =
    match Int32.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture) with
    | true, n -> Some n
    | _ -> None

let private parsePosition x y =
    match tryParseInt x, tryParseInt y with
    | Some x, Some y -> Some { X = x; Y = y }
    | _ -> None

let private parseObstacle (arg: string) =
    match arg.Split ',' with
    | [| x; y |] -> parsePosition x y
    | _ -> None

let private parseDirection (arg: string) =
    match arg.ToUpperInvariant() with
    | "N" -> Some N
    | "E" -> Some E
    | "S" -> Some S
    | "W" -> Some W
    | _ -> None

let private parseObstacles args =
    let parsed = args |> List.map (fun arg -> arg, parseObstacle arg)

    match parsed |> List.tryFind (snd >> Option.isNone) with
    | Some (bad, _) -> Error $"invalid obstacle '{bad}'. {usage}"
    | None -> parsed |> List.choose snd |> Ok

let private land position facing obstacles =
    Grid.create Grid.defaultSize Grid.defaultSize obstacles
    |> Result.bind (fun grid ->
        if not (Grid.contains grid position) then
            Error (Grid.outsideError grid position)
        elif Grid.isBlocked grid position then
            Error $"rover cannot start on an obstacle at {position.X},{position.Y}"
        else
            Ok (grid, { Position = position; Facing = facing }))

/// No arguments: start at 0 0 N on an empty 100x100 grid.
/// Otherwise: start position and heading, followed by any number of obstacles as "x,y".
let parse args =
    match args with
    | [] -> land { X = 0; Y = 0 } N []
    | x :: y :: facing :: obstacles ->
        match parsePosition x y, parseDirection facing, parseObstacles obstacles with
        | None, _, _ -> Error $"invalid start position '{x} {y}'. {usage}"
        | _, None, _ -> Error $"invalid direction '{facing}'. {usage}"
        | _, _, Error error -> Error error
        | Some position, Some facing, Ok obstacles -> land position facing obstacles
    | _ -> Error usage
