module FSharp.MarsRover.CommandCenterApp.Program

open System
open FSharp.MarsRover.Core
open FSharp.MarsRover.CommandCenterApp.Adapters

[<EntryPoint>]
let main args =
    args |> CommandCenter.run (stdoutTransmitter Console.Out)
    0
