namespace CommandHandlerTests

open System 
open Xunit
open Types
open EClosing.Core.Domain.Types
open EClosing.Core.Domain.Sequence
open EClosing.Core.Domain.CommandProcessingAgent


module Tests =

    let logger  = 
        {
            Debug = fun s -> ()//Console.WriteLine(s)
        }    
    
    [<Fact>]
    let ``Quand je reussit l'execution d'une commande , je sauve un evenement`` ()=
        toFact <| async {

            logger.Debug "Quand je reussit l'execution d'une commande , je sauve un evenement"
            let idAgg = Guid.NewGuid()
            let signataires = [ Signataire(Guid.NewGuid())]
            let document = Document(Guid.NewGuid())

            let hydrate id version= 
                async {
                    return State.Initial
                }
                

            let (tcs, waitingForMessages) = waitingFor 5000 []

            let save id version evts =
                async {
                    tcs.SetResult(evts)
                }
                
            
            let log id version cmd reason= async { () }


            let cmdProcessor = new CommandProcessor<Commands,State,Events>(logger, save,log,execute,apply,seal, State.Initial,0, idAgg)
            
            let msg = 
                {
                    Enveloppe= seal idAgg 0
                    PayLoad = Commands.PlanifierSequence(signataires,document)
                } 
            
            (cmdProcessor:>MsgProcessor<Commands>).Post(msg)

            let! results = waitingForMessages

            Assert.Equal<Events list>([SequencePlanifiee(signataires,document)], results)

        }
        

    
    [<Fact>]
    let ``Quand l'execution d'une commande échoue, je logue une erreur`` ()=

        toFact <| async { 
            logger.Debug "Quand l'execution d'une commande échoue, je logue une erreur"
            
            let idAgg = Guid.NewGuid()
            let signataires = [ Signataire(Guid.NewGuid())]
            let document = Document(Guid.NewGuid())

            let state = { State.Initial with EstCommmencee=true}

            let (tcs, waitingForMessages) = waitingFor 5000 ""

            let save id v evts = async { () }
            
            let log id version cmd reason=
                async {
                    tcs.SetResult(reason) |> ignore
                }
                

            let cmdProcessor = new CommandProcessor<Commands,State,Events>(logger,save,log,execute,apply,seal,state,1, idAgg)
            
            let msg = 
                {
                    Enveloppe= seal idAgg 0
                    PayLoad = Commands.PlanifierSequence(signataires,document)
                } 
            
            (cmdProcessor:>MsgProcessor<Commands>).Post(msg)

            let! results=  waitingForMessages

            Assert.Equal("Sequence déjà commencée", results)

            
        }
        

    [<Fact>]
    let ``Quand je recupere un agent, je cree un agent`` ()=

        toFact <| async { 
            
            logger.Debug "Quand je reussit l'execution d'une commande , je sauve un evenement"
            let idAgg = Guid.NewGuid()
            let signataires = [ Signataire(Guid.NewGuid())]
            let document = Document(Guid.NewGuid())

            let hydrate id version= State.Initial

            let (tcs, waitingForMessages) = waitingFor 5000 []

            let save id v evts =
                async {
                    tcs.SetResult(evts)
                }
                
            
            let log id version cmd reason= async { () }
                

            let cmdProcessor = new CommandProcessor<Commands,State,Events>(logger, save,log,execute,apply,seal,State.Initial, 0, idAgg)
            
            let msg = 
                {
                    Enveloppe= seal idAgg 0
                    PayLoad = Commands.PlanifierSequence(signataires,document)
                } 
            
            (cmdProcessor:>MsgProcessor<Commands>).Post(msg)

            let! results = waitingForMessages

            Assert.Equal<Events list>([SequencePlanifiee(signataires,document)], results)
                
            
        }
