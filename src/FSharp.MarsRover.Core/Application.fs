namespace FSharp.MarsRover.Core

/// Application services. Adapters are injected as plain functions; command sources are (lazy) sequences.
module RoverController =
    let run (report: Report -> unit) grid start (commands: string seq) =
        Navigation.executeAll grid start commands |> Seq.iter report

module CommandCenter =
    let run (transmit: string -> unit) (commands: string seq) =
        commands |> Seq.map Protocol.asSingleLine |> Seq.iter transmit
