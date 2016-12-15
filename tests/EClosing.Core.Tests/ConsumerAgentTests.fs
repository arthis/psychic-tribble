namespace ConsumwAgentTests

open System 
open Xunit
open EClosing.Core.Domain.Types
open EClosing.Core.Domain.Sequence
open EClosing.Core.Domain.CommandProcessingAgent
open EClosing.Core.Domain.Persistence
   

module Tests =


    type fakeProcessor<'a>(f) = 
        
        
        interface MsgProcessor<'a> with 
            member this.Post cmd = 
                f()


    [<Fact>]
    let ``Quand je recupere un agent, il est posté vers le commandProcessor`` ()=

        let logger  = 
            {
                Debug = fun s -> () //Console.WriteLine(s)
            }

        logger.Debug "Quand je recupere un agent, il est posté vers le commandProcessor"
        
        let idAgg = Guid.NewGuid()
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())
        let mutable closureval = true

        let memory = new Memory<fakeProcessor<string>>()
        let memoryPersistence = 
            { 
                get= memory.get
                set= memory.set
            }

        let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
            { 
                MessageId = Guid.NewGuid()
                CorrelationId = Guid.NewGuid()
                AggregateId=id
                Version = version
            } 

        let mutable count = 0
        let closure = fun () ->
            count <- count + 1
            closureval <- false

        let createProcessor = fun id -> async {
            return new fakeProcessor<string>(closure)
        }  

        let c = new ConsumerAgent<string,string,string,fakeProcessor<string>>(logger, memoryPersistence, createProcessor)

        let msg = 
            {
                Enveloppe= seal idAgg 0
                PayLoad = "test Message"
            } 

        (c:>MsgProcessor<string>).Post(msg)

        let timeOut = DateTime.Now.AddSeconds(5 |> float)
        while closureval do
            if DateTime.Now>timeOut then Assert.Equal("time Out", "boom!")
            Threading.Thread.Sleep(new TimeSpan(0,0,0,0,10))
            
        Assert.Equal(1,count)

        logger.Debug ""
        logger.Debug ""
        logger.Debug ""



    [<Fact>]
    let ``Quand je recois un message deux fois, il n'est executé qu'une seule fois`` ()=

        let logger  = 
            {
                Debug = fun s -> ()//Console.WriteLine(s)
            }

        logger.Debug "Quand je recois un message deux fois, il n'est executé qu'une seule fois"
        
        let idAgg = Guid.NewGuid()
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())
        let mutable closureval = true

        let memory = new Memory<fakeProcessor<string>>()
        let memoryPersistence = 
            { 
                get= memory.get
                set= memory.set
            }

        let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
            { 
                MessageId = Guid.NewGuid()
                CorrelationId = Guid.NewGuid()
                AggregateId=id
                Version = version
            } 

        let mutable count = 0
        let closure = fun () ->
            logger.Debug <| sprintf "closure count before %i " count
            count <- count + 1
            logger.Debug <| sprintf "closure count after %i " count
            if count=2 then
                closureval <- false

        let createProcessor = fun id ->  async {
            return new fakeProcessor<string>(closure)
        }

        let c = new ConsumerAgent<string,string,string,fakeProcessor<string>>(logger, memoryPersistence, createProcessor)

        let msg = 
            {
                Enveloppe= seal idAgg 0
                PayLoad = "test Message"
            } 

        logger.Debug <| sprintf "post message 1 "
        (c:>MsgProcessor<string>).Post(msg)
        logger.Debug <| sprintf "post message 2 "
        (c:>MsgProcessor<string>).Post(msg)

        let timeOut = DateTime.Now.AddMilliseconds(900 |> float)

        logger.Debug <| sprintf "timeout %A" timeOut
        while  closureval  && DateTime.Now<timeOut do
            Threading.Thread.Sleep(new TimeSpan(0,0,0,0,50))

        Assert.Equal(1,count)

        logger.Debug ""
        logger.Debug ""
        logger.Debug ""