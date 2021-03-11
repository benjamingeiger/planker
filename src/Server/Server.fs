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

    member __.UpdateGame gameId game =
        games.[gameId] <- game

        games.[gameId]

    member __.Vote gameId playerName effort =
        let game = __.GetGame gameId

        let currentRound = game.CurrentRound

        let newRound = { currentRound with Votes = (Map.add playerName effort currentRound.Votes) }

        __.UpdateGame gameId { game with CurrentRound = newRound }

let storage = Storage()

let gameApi =
    { getGame = fun (gameId) -> async { return storage.GetGame gameId }
      vote = fun (gameId, playerName, effort) -> async { return storage.Vote gameId playerName effort } }

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
