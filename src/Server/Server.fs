module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open System.Collections.Generic

open Shared

let defaultGameTitle = "default title"

let defaultVoteChoices = [
    Symbolic "1"
    Symbolic "2"
    Symbolic "3"
    Symbolic "5"
    Symbolic "8"
    Symbolic "13"
    Symbolic "21"
    Symbolic "?"
    Symbolic "Coffee"
]

type Storage () =
    let games = Dictionary<string, Game>()

    member __.GetGame gameId =
        match games.TryGetValue(gameId) with
        | (true, game) -> game
        | (false, _) -> Game.create defaultGameTitle defaultVoteChoices

let storage = Storage()

let gameApi =
    { getGame = fun gameId -> async { return storage.GetGame gameId } }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue gameApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
