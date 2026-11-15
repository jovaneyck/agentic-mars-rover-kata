module FSharp.MarsRover.Tests.NavigationTests

open Xunit
open Swensen.Unquote
open FSharp.MarsRover.Core

let private run grid rover commands = Navigation.execute grid rover commands

[<Fact>]
let ``Forward moves one square in the facing direction`` () =
    test <@ [ N; E; S; W ] |> List.map (fun facing -> run empty (at 5 5 facing) "f")
              = [ ok 5 6 N; ok 6 5 E; ok 5 4 S; ok 4 5 W ] @>

[<Fact>]
let ``Backward moves one square away from the facing direction without turning`` () =
    test <@ [ N; E; S; W ] |> List.map (fun facing -> run empty (at 5 5 facing) "b")
              = [ ok 5 4 N; ok 4 5 E; ok 5 6 S; ok 6 5 W ] @>

[<Fact>]
let ``Turning changes heading only`` () =
    let turn facing command = run empty (at 5 5 facing) command

    test <@ [ N; W; S; E ] |> List.map (fun facing -> turn facing "l") = [ ok 5 5 W; ok 5 5 S; ok 5 5 E; ok 5 5 N ] @>
    test <@ [ N; E; S; W ] |> List.map (fun facing -> turn facing "r") = [ ok 5 5 E; ok 5 5 S; ok 5 5 W; ok 5 5 N ] @>

[<Fact>]
let ``Wraps around the edges of the 100x100 grid`` () =
    let cases =
        [ at 0 99 N, "f", ok 0 0 N
          at 0 0 S, "f", ok 0 99 S
          at 99 0 E, "f", ok 0 0 E
          at 0 0 W, "f", ok 99 0 W
          at 0 0 N, "b", ok 0 99 N
          at 99 0 W, "b", ok 0 0 W ]

    test <@ cases |> List.filter (fun (rover, commands, expected) -> run empty rover commands <> expected) = [] @>

[<Fact>]
let ``Executes a sequence of commands`` () =
    test <@ run empty (at 0 0 N) "ffrff" = ok 2 2 E @>

[<Fact>]
let ``Commands are case insensitive`` () =
    test <@ run empty (at 0 0 N) "FFRff" = ok 2 2 E @>

[<Fact>]
let ``Invalid commands are ignored`` () =
    test <@ [ "fxf rzff"; "?f!f-r ff\t"; "ffrff🚀" ] |> List.map (run empty (at 0 0 N)) |> List.forall ((=) (ok 2 2 E)) @>

[<Fact>]
let ``Empty sequence leaves the rover in place`` () =
    test <@ run empty (at 3 4 S) "" = ok 3 4 S @>

[<Fact>]
let ``Stops at the last free square and reports the obstacle aborting the rest`` () =
    let grid = gridWith [ { X = 0; Y = 3 } ]

    test <@ run grid (at 0 0 N) "fffffrf" = Blocked (at 0 2 N, { X = 0; Y = 3 }) @>

[<Fact>]
let ``Detects obstacles across the wrapping edge`` () =
    let grid = gridWith [ { X = 0; Y = 99 } ]

    test <@ run grid (at 0 0 N) "b" = Blocked (at 0 0 N, { X = 0; Y = 99 }) @>

[<Fact>]
let ``Turning is never blocked by an adjacent obstacle`` () =
    let grid = gridWith [ { X = 0; Y = 1 } ]

    test <@ run grid (at 0 0 N) "r" = ok 0 0 E @>

[<Fact>]
let ``Each sequence continues from where the previous one ended even after an obstacle`` () =
    let grid = gridWith [ { X = 0; Y = 2 } ]

    test <@ Navigation.executeAll grid (at 0 0 N) [ "fff"; "rf"; "lff" ] |> List.ofSeq
              = [ Blocked (at 0 1 N, { X = 0; Y = 2 }); ok 1 1 E; ok 1 3 N ] @>

[<Fact>]
let ``A grid must have a positive size`` () =
    test <@ [ 0, 1; 1, 0; -1, 5 ] |> List.forall (fun (w, h) -> Grid.create w h [] |> Result.isError) @>

[<Fact>]
let ``Obstacles must lie inside the grid`` () =
    test <@ Grid.create 10 10 [ { X = 10; Y = 0 } ] |> Result.isError @>
