module FSharp.MarsRover.CommandCenterApp.Adapters

open System.IO

/// Driven adapter: sends one command sequence as one line.
let stdoutTransmitter (output: TextWriter) (commands: string) =
    output.WriteLine commands
    output.Flush()
