module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Model =
    { GameId: string
      Game: Deferred<Game>
      PlayerName: string
      CurrentVote: Effort option }

type Msg =
    | LoadGame of AsyncOperationStatus<Game>
    | Vote of Effort

let gameApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IGameApi>

let init(): Model * Cmd<Msg> =
    let model =
        { GameId = "default"
          Game = NotStartedYet
          PlayerName = "Derpy"
          CurrentVote = None }

    let cmd = Cmd.ofMsg (LoadGame Started)
    model, cmd

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | LoadGame (Started) ->
        let cmd = Cmd.OfAsync.perform gameApi.getGame model.GameId (fun r -> LoadGame (Finished r))

        { model with Game = InProgress }, cmd

    | LoadGame (Finished game) ->
        { model with Game = Resolved game }, Cmd.none

    | Vote effort ->
        let cmd = Cmd.OfAsync.perform gameApi.vote (model.GameId, model.PlayerName, effort) (fun _ -> LoadGame Started)

        { model with CurrentVote = Some effort }, cmd

open Fable.React
open Fable.React.Props
open Feliz
open Fulma

let view (model : Model) (dispatch : Msg -> unit) =
    let choices game =
        Html.div [
            prop.children [
                for choice in game.VoteChoices ->
                    match choice with
                    | Symbolic label ->
                        Html.button [
                            prop.onClick (fun _ -> dispatch (Vote choice))
                            prop.text label
                        ]
            ]
        ]

    Html.div [
        prop.style [
            style.padding 20
        ]
        prop.children [
            Html.p [
                prop.className "title"
                prop.text "HERP DERP TURTLES"
            ]

            Html.div [
                prop.children [
                    match model.Game with
                    | NotStartedYet -> Html.i [ prop.text "Game not loaded." ]
                    | InProgress -> Html.i [ prop.text "Loading..." ]
                    | Resolved game -> choices game
                ]
            ]
        ]
    ]
