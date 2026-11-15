/// Starts both executables as independent processes and plays the shell pipe between them:
/// command center stdout -> rover stdin. The rover's stdout is what mission control sees.
module FSharp.MarsRover.Tests.EndToEndTests

open System
open System.Diagnostics
open System.IO
open System.Threading
open System.Threading.Tasks
open Xunit
open Swensen.Unquote

let private start assembly args redirectInput =
    let info =
        ProcessStartInfo(
            "dotnet",
            RedirectStandardInput = redirectInput,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        )

    info.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, assembly))
    args |> List.iter info.ArgumentList.Add

    match Process.Start info with
    | null -> failwith $"could not start {assembly}"
    | proc -> proc

[<Fact>]
let ``Command center drives the rover over a stdin stdout pipe`` () : Task =
    task {
        use timeout = new CancellationTokenSource(TimeSpan.FromSeconds 30.)
        let cancel = timeout.Token
        use rover = start "FSharp.MarsRover.RoverApp.dll" [ "0"; "0"; "N"; "1,3" ] true
        use commandCenter = start "FSharp.MarsRover.CommandCenterApp.dll" [ "ffrff"; "bLff"; "x?!"; "fff" ] false

        do! commandCenter.StandardOutput.BaseStream.CopyToAsync(rover.StandardInput.BaseStream, cancel)
        rover.StandardInput.Close()
        let! replies = rover.StandardOutput.ReadToEndAsync cancel
        do! Task.WhenAll(commandCenter.WaitForExitAsync cancel, rover.WaitForExitAsync cancel)

        test <@ (commandCenter.ExitCode, rover.ExitCode) = (0, 0) @>

        test <@ replies.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                  = [| "OK 2 2 E"; "OBSTACLE 1 2 N 1 3"; "OK 1 2 N"; "OBSTACLE 1 2 N 1 3" |] @>
    }
