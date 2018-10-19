// Learn more about F# at http://fsharp.org

module Main

open System.IO
open System.Threading

open Suave
open Suave.Filters
open Suave.Operators
open Elmish
open Elmish.Bridge

type State = {
  running: bool
  next: int
}

let init _ _ = ({running = false; next = 0}, Cmd.none)

let emitCmd  = Cmd.ofAsync (fun _ -> async{Async.Sleep 1000 |> ignore}) () (fun _ -> Messages.ControlMsg.Emit) (fun _ -> failwith "exc, no msg")

let update client msg model = 
  match msg with 
  | Messages.ControlMsg.Start -> ({model with running = true}, emitCmd)
  | Messages.ControlMsg.Stop -> ({model with running = false}, Cmd.none)
  | Messages.ControlMsg.Emit -> 
    if model.running then
      client (sprintf "This is message #%d" model.next |> Messages.ClientMsg.Data)
      ({model with next = model.next + 1}, emitCmd)
    else
      (model, Cmd.none)

[<EntryPoint>]
let main argv =
  printfn "dir: %s" System.Environment.CurrentDirectory

  let server =
    Bridge.mkServer Messages.endpoint init update
    |> Bridge.run Suave.server

  let config =
    { defaultConfig with homeFolder = Some (Path.GetFullPath "./client/public") }
  let webPart =
    choose [
      server
      Filters.path "/" >=> Files.browseFileHome "index.html"
      GET >=> Files.browseHome
    ]
  startWebServer config webPart

  // let cts = new CancellationTokenSource()
  // let conf = { defaultConfig with cancellationToken = cts.Token }
  // let listening, server = startWebServerAsync conf (Successful.OK "Hello World")
    
  // Async.Start(server, cts.Token)
  // printfn "Make requests now"
  // Console.ReadKey true |> ignore
    
  // cts.Cancel()

  0 // return an integer exit code


