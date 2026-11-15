/// Wire format of the rover's replies, one line per received command sequence.
module FSharp.MarsRover.Core.Protocol

let private formatRover { Position = { X = x; Y = y }; Facing = facing } =
    sprintf "%d %d %A" x y facing

let format = function
    | Completed rover -> sprintf "OK %s" (formatRover rover)
    | Blocked (rover, { X = x; Y = y }) -> sprintf "OBSTACLE %s %d %d" (formatRover rover) x y

/// A command sequence travels as exactly one line, so embedded line breaks are dropped.
let asSingleLine (commands: string) =
    commands |> String.filter (fun c -> c <> '\r' && c <> '\n')
