namespace Types

open System 
open System.Threading
open System.Threading.Tasks
open EClosing.Core.Domain.Types
   
[<AutoOpen>]
module Types =

    let toFact computation : Task = Async.StartAsTask computation :> _


    let waitingFor timeOut (v:'a)= 
        let cts = new CancellationTokenSource(timeOut|> int)
        let tcs = new TaskCompletionSource<'a>()
        cts.Token.Register(fun (_) ->  tcs.SetCanceled()) |> ignore
        tcs ,Async.AwaitTask tcs.Task