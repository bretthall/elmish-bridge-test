module App.View

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Elmish.Bridge
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser
open Types
open App.State
open Global
open Elmish.React
open Elmish.Debug
open Elmish.HMR

// importAll "../sass/main.sass"

open Fable.Helpers.React
open Fable.Helpers.React.Props

// let menuItem label page currentPage =
//     li
//       [ ]
//       [ a
//           [ classList [ "is-active", page = currentPage ]
//             Href (toHash page) ]
//           [ str label ] ]

// let menu currentPage =
//   aside
//     [ ClassName "menu" ]
//     [ p
//         [ ClassName "menu-label" ]
//         [ str "General" ]
//       ul
//         [ ClassName "menu-list" ]
//         [ menuItem "Home" Home currentPage
//           menuItem "Counter sample" Counter currentPage
//           menuItem "About" Page.About currentPage ] ]

// let root model dispatch =

//   let pageHtml =
//     function
//     | Page.About -> Info.View.root
//     | Counter -> Counter.View.root model.counter (CounterMsg >> dispatch)
//     | Home -> Home.View.root model.home (HomeMsg >> dispatch)

//   div
//     []
//     [ div
//         [ ClassName "navbar-bg" ]
//         [ div
//             [ ClassName "container" ]
//             [ Navbar.View.root ] ]
//       div
//         [ ClassName "section" ]
//         [ div
//             [ ClassName "container" ]
//             [ div
//                 [ ClassName "columns" ]
//                 [ div
//                     [ ClassName "column is-3" ]
//                     [ menu model.currentPage ]
//                   div
//                     [ ClassName "column" ]
//                     [ pageHtml model.currentPage ] ] ] ] ]

type State = {
  messages: List<string>
  running: bool
}

let init _ = ({messages = List.empty; running = false}, Cmd.none)

let update msg model = 
  match msg with
  | Messages.ClientMsg.Data text -> ({model with messages = (text :: model.messages) |> List.truncate 10}, Cmd.none)
  | Messages.ClientMsg.Start -> Bridge.Send (Messages.ControlMsg.Start); ({model with running = true}, Cmd.none)
  | Messages.ClientMsg.Stop -> Bridge.Send (Messages.ControlMsg.Stop); ({model with running = false}, Cmd.none)

let view model dispatch = 
  div [] [
    (if model.running then
      button [OnClick (fun _ -> dispatch (Messages.ClientMsg.Stop))] [str "Stop"]
     else 
      button [OnClick (fun _ -> dispatch (Messages.ClientMsg.Start))] [str "Start"] 
    )
    ul [] (model.messages |> Seq.rev |> Seq.map (fun msg ->
      li [] [str msg]
    ))
  ]

Program.mkProgram init update view
|> Program.withBridge Messages.endpoint
|> Program.withReact "elmish-app"
|> Program.run

// App
// Program.mkProgram init update root
// |> Program.toNavigable (parseHash pageParser) urlUpdate
// #if DEBUG
// |> Program.withDebugger
// |> Program.withHMR
// #endif
// |> Program.withReact "elmish-app"
// |> Program.run
