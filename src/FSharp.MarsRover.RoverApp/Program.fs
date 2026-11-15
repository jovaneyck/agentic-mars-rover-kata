module FSharp.MarsRover.RoverApp.Program

open System
open FSharp.MarsRover.Core
open FSharp.MarsRover.RoverApp.Adapters

[<EntryPoint>]
let main args =
    match args |> List.ofArray |> Startup.parse with
    | Ok (grid, rover) ->
        stdinCommands Console.In
        |> RoverController.run (stdoutReporter Console.Out) grid rover
        0
    | Error error ->
        eprintfn "%s" error
        1
