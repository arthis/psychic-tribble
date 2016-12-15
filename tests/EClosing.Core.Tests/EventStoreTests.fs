namespace EventStoreTests

open System 
open Xunit
open EClosing.Core.Domain.Types
open EClosing.Core.Domain.EventSourceRepo
   

module Tests =

    

    [<Fact>]
    let ``Quand je recupere un repo, je peux sauvegarde un evenement`` ()= 
        async {
            let logger  = 
                {
                    Debug = fun s -> () //Console.WriteLine(s)
                }        

            logger.Debug "Quand je recupere un repo, je peux sauvegarde un evenement"

            let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
                { 
                    MessageId = Guid.NewGuid()
                    CorrelationId = Guid.NewGuid()
                    AggregateId=id
                    Version = version
                }

            let connectionstrings = "tcp://localhost:1113"
            let username = "admin"
            let password = "changeit"

            use! conn = create connectionstrings username password

            logger.Debug <| sprintf "connection openned"

            let streamName = "testAgg save 1"
            let id = Guid.NewGuid()
            let version = -1

            let msg = "test Message"
                
            do! save conn seal streamName id version  [msg]

            logger.Debug <| sprintf "message saved"

        }  |> Async.RunSynchronously

    [<Fact>]
    let ``Quand je recupere un repo, je peux sauvegarde deux evenements dans un unique stream`` ()= 
        async {
            let logger  = 
                {
                    Debug = fun s -> () //Console.WriteLine(s)
                }        

            logger.Debug "Quand je recupere un repo, je peux sauvegarde deux evenements dans un unique stream"


            let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
                { 
                    MessageId = Guid.NewGuid()
                    CorrelationId = Guid.NewGuid()
                    AggregateId=id
                    Version = version
                }


            let connectionstrings = "tcp://localhost:1113"
            let username = "admin"
            let password = "changeit"

            use! conn = create connectionstrings username password

            logger.Debug <| sprintf "connection openned"

            let id =Guid.NewGuid()
            let version = -1
            let streamName = "testAgg save 2 once"

            let msg1 = "test Message"
            let msg2 = "test Message2"
                

            do! save conn seal streamName id version [msg1, msg2]

            logger.Debug <| sprintf "message saved"

        }  |> Async.RunSynchronously

            
    [<Fact>]
    let ``Quand je recupere un repo, je peux sauvegarder deux fois consecutive un evenement`` ()= 
        async {
            let logger  = 
                {
                    Debug = fun s -> () //Console.WriteLine(s)
                }        

            logger.Debug "Quand je recupere un repo, je peux sauvegarder deux fois consecutive un evenement"


            let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
                { 
                    MessageId = Guid.NewGuid()
                    CorrelationId = Guid.NewGuid()
                    AggregateId=id
                    Version = version
                }


            let connectionstrings = "tcp://localhost:1113"
            let username = "admin"
            let password = "changeit"

            use! conn = create connectionstrings username password

            logger.Debug <| sprintf "connection openned"

            let id =Guid.NewGuid()
            let version = -1
            let streamName = "testAgg2 in a row"
            let msg1 = "test Message"
            let msg2 = "test Message2"
                

            do! save conn seal streamName id version  [msg1]

            do! save conn seal streamName id (version+1)  [msg2]

            logger.Debug <| sprintf "message saved"

        }  |> Async.RunSynchronously


    type FakeEvent =
        | MyFirstType of string
        | MySecondType of int

    [<Fact>]
    let ``Quand je recupere un repo, je peux sauvegarde deux fois consecutive deux evenements de type distinct`` ()= 
        async {
            let logger  = 
                {
                    Debug = fun s -> () //Console.WriteLine(s)
                }        

            logger.Debug "Quand je recupere un repo, je peux sauvegarde deux fois consecutive deux evenements de type distinct"


            let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
                { 
                    MessageId = Guid.NewGuid()
                    CorrelationId = Guid.NewGuid()
                    AggregateId=id
                    Version = version
                }


            let connectionstrings = "tcp://localhost:1113"
            let username = "admin"
            let password = "changeit"

            use! conn = create connectionstrings username password

            logger.Debug <| sprintf "connection openned"

            let id =Guid.NewGuid()
            let version = -1
            let streamName = "testAgg 2 different in a row"
            let msg1 = MyFirstType("test Message")
            let msg2 = MySecondType(123455)
                

            do! save conn seal streamName id version  [msg1]

            do! save conn seal streamName id (version+1)  [msg2]

            logger.Debug <| sprintf "message saved"

        }  |> Async.RunSynchronously

    [<Fact>]
    let ``Quand je sauve un message, je peux lire ce message`` ()= 
        async {
            let logger  = 
                {
                    Debug = fun s -> ()//Console.WriteLine(s)
                }        

            logger.Debug "Quand je sauve un message, je peux lire ce message"

            let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
                { 
                    MessageId = Guid.NewGuid()
                    CorrelationId = Guid.NewGuid()
                    AggregateId=id
                    Version = version
                }

            let connectionstrings = "tcp://localhost:1113"
            let username = "admin"
            let password = "changeit"

            use! conn = create connectionstrings username password

            logger.Debug <| sprintf "connection openned"

            let id =Guid.NewGuid()
            let version = -1
            let streamName = "testAgg 2 different in a row"
            let msg1 = MyFirstType("test Message")
            let msg2 = MySecondType(123455)
                

            do! save conn seal streamName id version  [msg1]

            let applyto s e = e::s
            let! (agg,newVersion) = hydrate conn streamName applyto [] id

            logger.Debug <| sprintf "hydrated %A version  %i" agg newVersion

            Assert.Equal<FakeEvent list>([msg1], agg)

            

        }  |> Async.RunSynchronously