module FSharp.MarsRover.Tests.StartupAndProtocolTests

open Xunit
open FsCheck.Xunit
open Swensen.Unquote
open FSharp.MarsRover.Core

let private summary (grid, rover) =
    rover, Grid.width grid, Grid.height grid, Grid.obstacles grid

[<Fact>]
let ``Without arguments the rover starts at the origin facing north on an empty 100x100 grid`` () =
    test <@ Startup.parse [] |> Result.map summary = Ok (at 0 0 N, 100, 100, Set.empty) @>

[<Fact>]
let ``Parses start position heading and obstacles`` () =
    test <@ Startup.parse [ "3"; "4"; "w"; "0,3"; "99,99" ] |> Result.map summary
              = Ok (at 3 4 W, 100, 100, set [ { X = 0; Y = 3 }; { X = 99; Y = 99 } ]) @>

[<Fact>]
let ``Rejects invalid startup arguments`` () =
    let invalid =
        [ [ "1" ]
          [ "1"; "2" ]
          [ "x"; "2"; "N" ]
          [ "1"; "2"; "Q" ]
          [ "1"; "2"; "N"; "3" ]
          [ "1"; "2"; "N"; "3,a" ]
          [ "1"; "2"; "N"; "3,4,5" ]
          [ "100"; "0"; "N" ]
          [ "0"; "-1"; "N" ]
          [ "0"; "0"; "N"; "0,100" ]
          [ "2"; "2"; "N"; "2,2" ] ]

    test <@ invalid |> List.filter (Startup.parse >> Result.isOk) = [] @>

// Pattern 8: whatever the operator passes on the command line, startup answers Ok or Error and never throws.
[<Property(MaxTest = 1000)>]
let ``Any arguments yield a verdict without throwing`` (args: char array list) =
    args |> List.map System.String |> Startup.parse |> ignore

[<Fact>]
let ``Formats a completed sequence`` () =
    test <@ Protocol.format (ok 2 3 E) = "OK 2 3 E" @>

[<Fact>]
let ``Formats an obstacle report with the rover position followed by the obstacle`` () =
    test <@ Protocol.format (Blocked (at 0 2 N, { X = 0; Y = 3 })) = "OBSTACLE 0 2 N 0 3" @>

[<Fact>]
let ``A command sequence is flattened to a single line`` () =
    test <@ Protocol.asSingleLine "ff\r\nr\nl" = "ffrl" @>
