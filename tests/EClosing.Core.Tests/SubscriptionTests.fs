namespace SubscriptionTests

open System 
open System.Threading
open System.Threading.Tasks
open Xunit
open Types
open EClosing.Core.Domain.Types
open EClosing.Core.Domain.Sequence
open EClosing.Core.Domain.CommandProcessingAgent
open EClosing.Core.Domain.Persistence
open EClosing.Core.Domain.RabbitMq
open RabbitMQ.Client 
   

module Tests =
    
    
    let logger  = 
        {
            Debug = fun s -> ()//Console.WriteLine(s)
        }

    
    
    [<Fact>]
    let ``Quand je dispatch un message, j'execute la fonction associée`` ()= 
            toFact <|async {
        
            logger.Debug "Quand je dispatch un message, j'execute la fonction associée"
            
            let idAgg = Guid.NewGuid()

            let msg = 
                {
                    Enveloppe= seal idAgg 0
                    PayLoad = "some message of hope"
                }  

            let mutable subscriptionNotFound= true
            let react (message: Message<string>) =
                Assert.Equal(msg,message)
                subscriptionNotFound <- false
                ()
        
            let exchangeName =  sprintf "MyExchangeName%A" (Guid.NewGuid())
            let routingKey =  sprintf "MyRoutingKey%A" (Guid.NewGuid())
            let hostName = "localhost"
            let username = "guest"
            let password = "guest"
            let appId = "testApp"

            use dispatcher = new Dispatcher(hostName, username,password, appId, logger, exchangeName)

            let cts = new CancellationTokenSource(5000)
            let waitingforResult = dispatcher.First(routingKey, react,cts.Token )
            dispatcher.Publish(routingKey, msg)

            let! result = waitingforResult
                
            Assert.False(subscriptionNotFound)

            logger.Debug ""
            logger.Debug ""
            logger.Debug ""
        }

    [<Fact>]
    let ``Quand je dispatch deux messages differents, j'execute deux fois la fonction associée`` ()=
        toFact <| async {
            logger.Debug "Quand je dispatch deux messages, j'execute deux fois la fonction associée"
        
            let idAgg = Guid.NewGuid()

            let waitingTwoTimes (token: CancellationToken)=
                let tcs = new TaskCompletionSource<'a>()
                let mutable count = 0
                let f msg =
                    logger.Debug <| sprintf "waiting and found msg %i" count
                    count <- count + 1
                    if count = 2 then 
                        tcs.SetResult(count)  
                token.Register(fun (_) ->  tcs.SetCanceled()) |> ignore 
                (Async.AwaitTask tcs.Task, f)

            let msg1 = 
                {
                    Enveloppe= seal idAgg 0
                    PayLoad = "some message of hope"
                }  

            let msg2 = 
                {
                    Enveloppe= seal idAgg 0
                    PayLoad = "some message of joy"
                }  

       
            let exchangeName =  sprintf "MyExchangeName%A" (Guid.NewGuid())
            let routingKey =  sprintf "MyRoutingKey%A" (Guid.NewGuid())
            let hostName = "localhost"
            let username = "guest"
            let password = "guest"
            let appId = "testApp"

            use dispatcher = new Dispatcher(hostName, username,password, appId, logger, exchangeName)

            let cts = new CancellationTokenSource(5000)
            let (waiting,f) = waitingTwoTimes(cts.Token)
            dispatcher.Subscribe(routingKey,f )|> ignore

            dispatcher.Publish(routingKey, msg1)
            dispatcher.Publish(routingKey, msg2)

            let! result = waiting
                
            Assert.Equal(2, result)

            logger.Debug ""
            logger.Debug ""
            logger.Debug ""
        }
        