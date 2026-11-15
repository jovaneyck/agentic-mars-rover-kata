/// Both application services wired to in-memory adapters, no processes involved.
module FSharp.MarsRover.Tests.ApplicationTests

open Xunit
open Swensen.Unquote
open FSharp.MarsRover.Core

[<Fact>]
let ``Command center transmits each operator sequence as one line`` () =
    let sent = ResizeArray()

    CommandCenter.run sent.Add [ "ffr"; "b\nb" ]

    test <@ List.ofSeq sent = [ "ffr"; "bb" ] @>

[<Fact>]
let ``Rover reports once per received sequence keeping state between them`` () =
    let lines = ResizeArray()

    RoverController.run (Protocol.format >> lines.Add) (gridWith [ { X = 1; Y = 3 } ]) (at 0 0 N) [ "ff"; "rf"; "lff"; ""; "zz" ]

    test <@ List.ofSeq lines = [ "OK 0 2 N"; "OK 1 2 E"; "OBSTACLE 1 2 N 1 3"; "OK 1 2 N"; "OK 1 2 N" ] @>

[<Fact>]
let ``Command center drives the rover through in-memory adapters`` () =
    let wire = ResizeArray()
    let lines = ResizeArray()

    CommandCenter.run wire.Add [ "ffrff"; "bbbb" ]
    RoverController.run (Protocol.format >> lines.Add) empty (at 0 0 N) wire

    test <@ List.ofSeq lines = [ "OK 2 2 E"; "OK 98 2 E" ] @>
