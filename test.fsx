open System 
open FSharp
open System.Threading
open System.Threading.Tasks

   

module MyModule =

    type Agent<'a> = MailboxProcessor<'a>
    let waitingFor timeOut (v:'a)= 
        let cts = new CancellationTokenSource(timeOut|> int)
        let tcs = new TaskCompletionSource<'a>()
        cts.Token.Register(fun (_) ->  tcs.SetCanceled()) |> ignore
        tcs ,Async.AwaitTask tcs.Task

    type MyProcessor<'a>(f:'a->unit) =
        let agent = Agent<'a>.Start(fun inbox -> 
             let rec loop() = async {

                let! msg = inbox.Receive()
                // some more complex should be used here
                f msg
                return! loop() 
             }
             loop()
        )

        member this.Post(msg:'a) = 
            agent.Post msg

    
open MyModule

let myTest =
    async {

        let (tcs,waitingFor) = waitingFor 5000 0

        let doThatWhenMessagepostedWithinAgent msg =
            tcs.SetResult(msg)

        let p = new MyProcessor<int>(doThatWhenMessagepostedWithinAgent)

        p.Post 3

        let! result = waitingFor

        return result

    }

myTest 
|> Async.RunSynchronously
|> System.Console.WriteLine 
    

    