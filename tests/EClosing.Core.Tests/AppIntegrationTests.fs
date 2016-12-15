namespace AppIntegrationTests

open System 
open Xunit
open EClosing.Core.Domain.Types
open EClosing.Core.Domain.Sequence
open EClosing.Core.Domain.CommandProcessingAgent
open EClosing.Core.Domain.Persistence
open EClosing.Core.Domain.RabbitMq
open EClosing.Core.Domain
   

module Tests =
    
    let logger  = 
        {
            Debug = fun s -> ()//Console.WriteLine(s)
        }
        
 
    [<Fact>]
    let ``Given a proper app, when  I publish a message,I expect an event to be saved`` ()= 
        async {

            logger.Debug "Given a proper app, when  I publish a message,I expect an event to be saved"

            //settings
            let exchangeName =  sprintf "MyAppExchange-%A" (Guid.NewGuid())

            use dispatcher =  new Dispatcher("localhost","guest","guest", "MyApp", logger, exchangeName)
            use! conn =  EventSourceRepo.create "tcp://localhost:1113" "admin" "changeit"    
        
            let mySubscription =
                {
                    RoutingKey= "Sequence"
                    AggregateName = "Sequence"
                    IsCommandValid = fun  msg -> true
                    InitialState = Sequence.State.Initial
                    Apply = Sequence.apply
                    Execute = Sequence.execute
                } 


            let myApp = 
                App.create dispatcher conn logger
                |> App.addSubscription mySubscription


            let signataires = [ Signataire(Guid.NewGuid())]
            let document = Document(Guid.NewGuid())

            let id = Guid.NewGuid()
            let enveloppe = 
                {
                    MessageId = Guid.NewGuid()
                    CorrelationId = Guid.NewGuid()
                    AggregateId=id
                    Version = -1      
                }
            let msg = 
                {
                    Enveloppe= enveloppe 
                    PayLoad = Commands.PlanifierSequence(signataires,document)
                }  
            dispatcher.Publish("Sequence",msg) 

            let timeOut = DateTime.Now.AddSeconds(5 |> float)

            while DateTime.Now<timeOut do
                //verify event in getEventStore
                //Assert.Equal(e, SequencePlanifiee(signataires,document))
                // timoe out -> boom
                Threading.Thread.Sleep(new TimeSpan(0,0,0,0,10))

            
            
                
            logger.Debug ""
            logger.Debug ""
            logger.Debug ""

        } |> Async.RunSynchronously