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
  num: int
}

let hub = ServerHub<State, Messages.ControlMsg, Messages.ClientMsg> ()

type MgrMsg = 
    | NewClient of Dispatch<Messages.ClientMsg>
    | Start of Dispatch<Messages.ClientMsg>
    | Stop of Dispatch<Messages.ClientMsg>
    | Emit

type MgrState = {
    running: bool
    next: int
    messages: List<string>
}

let clientMgr = MailboxProcessor<MgrMsg>.Start (fun inbox ->
    let rec loop state = 
        let sendMsg () = 
            let msg = sprintf "This is message #%d" state.next 
            hub.BroadcastClient(msg |> Messages.ClientMsg.Data)
            async {
                do! Async.Sleep 1000
                do inbox.Post Emit
                return ()
            } |> Async.Start
            {state with next = state.next + 1; messages = msg :: state.messages}

        async {
            match! inbox.Receive () with
            | NewClient client ->
                client (Messages.ClientMsg.Current {running = state.running; messages = state.messages})
                return! loop state
            | Start client ->
                if not state.running then
                    hub.BroadcastClient (Messages.ClientMsg.Startted)
                    client (Messages.ClientMsg.Data "I started it")
                    return! loop {sendMsg () with running = true}
                else 
                    return! loop state
            | Stop client ->
                if state.running then
                    hub.BroadcastClient (Messages.ClientMsg.Stopped)
                    client (Messages.ClientMsg.Data "I stopped it")
                    return! loop {state with running = false}
                else 
                    return! loop state
            | Emit ->
                if state.running then
                    return! loop (sendMsg ())
                else
                    return! loop state
        }
    loop {running = false; next = 0; messages = []}
)

let mutable num = 0
let init client _ = 
    let n = num
    num <- num + 1
    clientMgr.Post (NewClient client)
    ({num = n}, Cmd.none)

let update client msg model = 
  match msg with 
  | Messages.ControlMsg.Start -> clientMgr.Post (Start client)
  | Messages.ControlMsg.Stop -> clientMgr.Post (Stop client)
  (model, Cmd.none)

[<EntryPoint>]
let main argv =
  printfn "dir: %s" System.Environment.CurrentDirectory

  let server =
    Bridge.mkServer Messages.endpoint init update
    |> Bridge.withServerHub hub
    |> Bridge.run Suave.server

  let config =
    { 
        defaultConfig with 
            homeFolder = Some (Path.GetFullPath "../../../../client/public")
            SuaveConfig.bindings= [HttpBinding.createSimple Protocol.HTTP "127.0.0.1" 50555]
    }
  printfn "config: %A" config
  let webPart =
    choose [
      server
      Filters.path "/" >=> Files.browseFileHome "index.html"
      GET >=> Files.browseHome
    ]
  startWebServer config webPart

  //startWebServer defaultConfig (Successful.OK "Hi")

  // let cts = new CancellationTokenSource()
  // let conf = { defaultConfig with cancellationToken = cts.Token }
  // let listening, server = startWebServerAsync conf (Successful.OK "Hello World")
    
  // Async.Start(server, cts.Token)
  // printfn "Make requests now"
  // Console.ReadKey true |> ignore
    
  // cts.Cancel()

  0 // return an integer exit code


