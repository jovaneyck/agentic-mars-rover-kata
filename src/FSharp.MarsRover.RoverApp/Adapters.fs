module FSharp.MarsRover.RoverApp.Adapters

open System.IO
open FSharp.MarsRover.Core

/// Driving adapter: one command sequence per line, read lazily until end-of-file.
let stdinCommands (input: TextReader) =
    Seq.unfold
        (fun () ->
            match input.ReadLine() with
            | null -> None
            | line -> Some (line, ()))
        ()

/// Driven adapter: publishes each report as one protocol line.
let stdoutReporter (output: TextWriter) report =
    output.WriteLine(Protocol.format report)
    output.Flush()
