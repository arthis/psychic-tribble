namespace AgentTests

open System 
open Xunit
open EClosing.Core.Domain.Types
open EClosing.Core.Domain.Sequence
open EClosing.Core.Domain.CommandProcessingAgent
open EClosing.Core.Domain.Persistence
   

module Tests =
    
    let logger  = 
        {
            Debug = fun s -> () //Console.WriteLine(s)
        }

    // [<Fact>]
    // let ``Quand je recupere un agent, je cree un agent`` ()=
    //     logger.Debug "Quand je recupere un agent, je cree un agent"
    //     let idAgg = Guid.NewGuid()
    //     let signataires = [ Signataire(Guid.NewGuid())]
    //     let document = Document(Guid.NewGuid())

    //     let memory = new Memory<CommandProcessor<Commands,State,Events>>()
    //     let memoryPersistence = 
    //         { 
    //             get= memory.get
    //             set= memory.set
    //         }

        

    //     let hydrate id version= State.Initial

    //     let mutable closureval = true

    //     let save (enveloppe:Enveloppe) id evts =
    //         Assert.Equal(idAgg,id)
    //         Assert.Equal<Events list>([SequencePlanifiee(signataires,document)], evts)
    //         closureval<- false
        
    //     let log id version cmd reason=
    //         Assert.Empty(reason)
    //         Assert.False(true)

    //     let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
    //         { 
    //             MessageId = Guid.NewGuid()
    //             CorrelationId = Guid.NewGuid()
    //             AggregateId=id
    //             Version = version
    //         } 

    //     let createProcessor = fun id ->  new CommandProcessor<Commands,State,Events>(logger, hydrate,save,log,execute,apply,seal, id)

    //     let agent = hydrateAgent memoryPersistence createProcessor idAgg

    //     Assert.NotEqual(Guid.Empty, agent.AgentId)

    //     let agent2 = hydrateAgent memoryPersistence createProcessor idAgg

    //     Assert.Equal(agent.AgentId, agent2.AgentId)
            
    //     logger.Debug ""
    //     logger.Debug ""
    //     logger.Debug ""

    