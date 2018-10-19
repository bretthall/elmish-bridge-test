module Messages

type ControlMsg = 
    | Start 
    | Stop
    | Emit

type ClientMsg = 
    | Data of string
    | Start
    | Stop

let endpoint = "/socket"