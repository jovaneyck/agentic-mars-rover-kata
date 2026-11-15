[<AutoOpen>]
module FSharp.MarsRover.Tests.TestHelpers

open FSharp.MarsRover.Core

let at x y facing = { Position = { X = x; Y = y }; Facing = facing }

let ok x y facing = Completed (at x y facing)

let gridOf width height obstacles =
    Grid.create width height obstacles |> Result.defaultWith failwith

let gridWith obstacles = gridOf Grid.defaultSize Grid.defaultSize obstacles

let empty = gridWith []
