module Messages

type ControlMsg = 
    | Start 
    | Stop

type CurrentState = {
    running: bool
    messages: List<string>
}

type ClientMsg = 
    | Current of CurrentState
    | Data of string
    | Startted
    | Stopped

let endpoint = "/socket"