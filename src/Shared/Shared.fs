namespace Shared

open System

// Stuff from the Elmish book
type Deferred<'t> =
    | NotStartedYet
    | InProgress
    | Resolved of 't

type AsyncOperationStatus<'t> =
    | Started
    | Finished of 't

[<RequireQualifiedAccess>]
module Deferred =

    let exists (predicate: 't -> bool) (deferred: Deferred<'t>) : bool =
        match deferred with
        | NotStartedYet -> false
        | InProgress -> false
        | Resolved value -> predicate value

    let map (transform: 't -> 'u) (deferred: Deferred<'t>) : Deferred<'u> =
        match deferred with
        | NotStartedYet -> NotStartedYet
        | InProgress -> InProgress
        | Resolved value -> Resolved (transform value)

    let bind (transform: 't -> Deferred<'u>) (deferred: Deferred<'t>) : Deferred<'u> =
        match deferred with
        | NotStartedYet -> NotStartedYet
        | InProgress -> InProgress
        | Resolved value -> transform value

// Domain definitions

type Player = string

type Effort =
    | Symbolic of string
    // Add "Numeric of int" here eventually

type Round = {
    Votes: Map<Player, Effort>
}

type Game = {
    Title: string
    VoteChoices: Effort list
    CurrentRound: Round
    PreviousRounds: Round list
}

module Game =
    let create (title: string) (voteChoices: Effort list) = {
        Title = title
        VoteChoices = voteChoices
        CurrentRound = {
            Votes = Map.empty
        }
        PreviousRounds = []
    }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type IGameApi =
    { getGame : string -> Async<Game> }
