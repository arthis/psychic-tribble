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
                    Debug = fun s -> Console.WriteLine(s)
                }        

            logger.Debug "Quand je recupere un repo, je peux sauvegarde un evenement"


            let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
                { 
                    MessageId = Guid.NewGuid()
                    CorrelationId = Guid.NewGuid()
                    AggregateId=id
                    Version = version
                }


            let connectionstrings = "tcp://127.0.0.1:1113"
            let username = "yoann"
            let password = "yoann"

            use! conn = create connectionstrings username password

            logger.Debug <| sprintf "connection openned"

            let streamName = sprintf "testAgg-%A" (Guid.NewGuid())
            let enveloppe = seal <| Guid.NewGuid()

            let msg = 
                {
                    Enveloppe= enveloppe 0
                    PayLoad = "test Message"
                }

            do! save conn streamName enveloppe [msg]

            logger.Debug <| sprintf "message saved"

        }  |> Async.RunSynchronously

            