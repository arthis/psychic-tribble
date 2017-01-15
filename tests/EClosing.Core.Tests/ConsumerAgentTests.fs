namespace ConsumwAgentTests

open System 
open System.Threading
open System.Threading.Tasks
open Xunit
open Types
open EClosing.Core.Domain.Types
open EClosing.Core.Domain.Sequence
open EClosing.Core.Domain.CommandProcessingAgent
open EClosing.Core.Domain.Persistence
   

module Tests =


    type fakeProcessor<'a>(f) = 
        
        interface MsgProcessor<'a> with 
            member this.Post cmd = 
                f cmd

    
    let createMemoryPersistence<'a>() =
        let memory = new Memory<fakeProcessor<'a>>()
        { 
            get= memory.get
            set= memory.set
        }
        
    let logger  = 
        {
            Debug = fun s -> () //Console.WriteLine(s)
        }

    [<Fact>]
    let ``Quand je recupere un agent, il est posté vers le commandProcessor`` ()=
        toFact <| async {

            logger.Debug "Quand je recupere un agent, il est posté vers le commandProcessor"
            
            let idAgg = Guid.NewGuid()
            let signataires = [ Signataire(Guid.NewGuid())]
            let document = Document(Guid.NewGuid())
            let mutable closureval = true

            let persistence = createMemoryPersistence<string>()

            let msg = 
                {
                    Enveloppe= seal idAgg 0
                    PayLoad = "test Message"
                } 
            
            let (tcs, waitingForMessage) = waitingFor 5000 msg

            let mutable count = 0
            let closure = fun msg ->
                tcs.SetResult(msg)

            let createProcessor = fun id -> async {
                return new fakeProcessor<string>(closure)
            }  

            let c = new ConsumerAgent<string,string,string,fakeProcessor<string>>(logger, persistence, createProcessor,  ignore)

            let msg = 
                {
                    Enveloppe= seal idAgg 0
                    PayLoad = "test Message"
                } 

            (c:>MsgProcessor<string>).Post(msg)

            let! result = waitingForMessage
                
            Assert.Equal(msg,result)

            logger.Debug ""
            logger.Debug ""
            logger.Debug ""

        }

    [<Fact>]
    let ``Quand je recois un message deux fois, il n'est executé qu'une seule fois`` ()=
        toFact <| async {

            logger.Debug "Quand je recois un message deux fois, il n'est executé qu'une seule fois"
            
            let idAgg = Guid.NewGuid()
            let signataires = [ Signataire(Guid.NewGuid())]
            let document = Document(Guid.NewGuid())

            let memory = new Memory<fakeProcessor<string>>()
            let memoryPersistence = 
                { 
                    get= memory.get
                    set= memory.set
                }

            let (tcs, waitingForMessages) = waitingFor 5000 0
            let discardingMessage = fun msg ->
                tcs.SetResult(2)
                    

            let createProcessor = fun id ->  async {
                return new fakeProcessor<string>(ignore)
            }

            let c = new ConsumerAgent<string,string,string,fakeProcessor<string>>(logger, memoryPersistence, createProcessor, discardingMessage)

            let msg = 
                {
                    Enveloppe= seal idAgg 0
                    PayLoad = "test Message"
                } 

            logger.Debug <| sprintf "post message 1 "
            (c:>MsgProcessor<string>).Post(msg)
            logger.Debug <| sprintf "post message 2 "
            (c:>MsgProcessor<string>).Post(msg)

            let! result = waitingForMessages

            Assert.Equal(2,result)


        }