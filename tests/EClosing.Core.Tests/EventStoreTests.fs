namespace EventStoreTests

open System 
open Xunit
open Types
open EClosing.Core.Domain.Types
open EClosing.Core.Domain.EventSourceRepo
   

module Tests =

    [<Fact>]
    let ``Quand je recupere un repo, je peux sauvegarde un evenement`` ()= 
        toFact <| async {
            let logger  = 
                {
                    Debug = fun s -> () //Console.WriteLine(s)
                }        

            logger.Debug "Quand je recupere un repo, je peux sauvegarde un evenement"

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

        } 

    [<Fact>]
    let ``Quand je recupere un repo, je peux sauvegarde deux evenements dans un unique stream`` ()= 
        toFact <| async {
            let logger  = 
                {
                    Debug = fun s -> () //Console.WriteLine(s)
                }        

            logger.Debug "Quand je recupere un repo, je peux sauvegarde deux evenements dans un unique stream"

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

        }  

            
    [<Fact>]
    let ``Quand je recupere un repo, je peux sauvegarder deux fois consecutive un evenement`` ()= 
        toFact <| async {
            let logger  = 
                {
                    Debug = fun s -> () //Console.WriteLine(s)
                }        

            logger.Debug "Quand je recupere un repo, je peux sauvegarder deux fois consecutive un evenement"


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

        }  


    type FakeEvent =
        | MyFirstType of string
        | MySecondType of int

    [<Fact>]
    let ``Quand je recupere un repo, je peux sauvegarde deux fois consecutive deux evenements de type distinct`` ()= 
        toFact <| async {
            let logger  = 
                {
                    Debug = fun s -> () //Console.WriteLine(s)
                }        

            logger.Debug "Quand je recupere un repo, je peux sauvegarde deux fois consecutive deux evenements de type distinct"


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

        }  

    [<Fact>]
    let ``Quand je sauve un message, je peux lire ce message`` ()= 
        toFact <| async {
            let logger  = 
                {
                    Debug = fun s -> ()//Console.WriteLine(s)
                }        

            logger.Debug "Quand je sauve un message, je peux lire ce message"


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

            

        }  