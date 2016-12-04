namespace CommandHandlerTests

open System 
open Xunit
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
        logger.Debug "Quand je reussit l'execution d'une commande , je sauve un evenement"
        let idAgg = Guid.NewGuid()
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())

        let hydrate id version= State.Initial

        let mutable closureval = true

        let save (enveloppe:Enveloppe) id evts =
            Assert.Equal(idAgg,id)
            Assert.Equal<Events list>([SequencePlanifiee(signataires,document)], evts)
            closureval<- false
        
        let log id version cmd reason=
            Assert.Empty(reason)
            Assert.False(true)

        let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
            { 
                MessageId = Guid.NewGuid()
                CorrelationId = Guid.NewGuid()
                AggregateId=id
                Version = version
            } 

        let cmdProcessor = new CommandProcessor<Commands,State,Events>(logger, hydrate,save,log,execute,apply,seal, idAgg)
        
        let msg = 
            {
                Enveloppe= seal idAgg 0
                PayLoad = Commands.PlanifierSequence(signataires,document)
            } 
        
        (cmdProcessor:>MsgProcessor<Commands>).Post(msg)

        let timeOut = DateTime.Now.AddSeconds(5 |> float)
        while closureval do
            if DateTime.Now>timeOut then Assert.Equal("time Out", "boom!")
            Threading.Thread.Sleep(new TimeSpan(0,0,0,0,500)) 
            
        logger.Debug ""
        logger.Debug ""
        logger.Debug ""

    
    [<Fact>]
    let ``Quand l'execution d'une commande échoue, je logue une erreur`` ()=

        logger.Debug "Quand l'execution d'une commande échoue, je logue une erreur"
        
        let idAgg = Guid.NewGuid()
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())

        let hydrate id version= { State.Initial with EstCommmencee=true}

        let mutable closureval = true

        let save (enveloppe:Enveloppe) id evts =
            Assert.False(true)
            
        
        let log id version cmd reason=
            Assert.Equal(idAgg,id)
            Assert.Equal(0,version)
            Assert.Equal("Sequence déjà commencée", reason)
            closureval<- false

        let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
            { 
                MessageId = Guid.NewGuid()
                CorrelationId = Guid.NewGuid()
                AggregateId=id
                Version = version
            }

        

        let cmdProcessor = new CommandProcessor<Commands,State,Events>(logger,hydrate,save,log,execute,apply,seal, idAgg)
        
        let msg = 
            {
                Enveloppe= seal idAgg 0
                PayLoad = Commands.PlanifierSequence(signataires,document)
            } 
        
        (cmdProcessor:>MsgProcessor<Commands>).Post(msg)

        let timeOut = DateTime.Now.AddSeconds(5 |> float)
        while closureval do
            if DateTime.Now>timeOut then Assert.Equal("time Out", "boom!")
            Threading.Thread.Sleep(new TimeSpan(0,0,0,0,500))

        logger.Debug ""
        logger.Debug ""
        logger.Debug ""
        

    [<Fact>]
    let ``Quand je recupere un agent, je cree un agent`` ()=
        logger.Debug "Quand je reussit l'execution d'une commande , je sauve un evenement"
        let idAgg = Guid.NewGuid()
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())

        let hydrate id version= State.Initial

        let mutable closureval = true

        let save (enveloppe:Enveloppe) id evts =
            Assert.Equal(idAgg,id)
            Assert.Equal<Events list>([SequencePlanifiee(signataires,document)], evts)
            closureval<- false
        
        let log id version cmd reason=
            Assert.Empty(reason)
            Assert.False(true)

        let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
            { 
                MessageId = Guid.NewGuid()
                CorrelationId = Guid.NewGuid()
                AggregateId=id
                Version = version
            } 

        let cmdProcessor = new CommandProcessor<Commands,State,Events>(logger, hydrate,save,log,execute,apply,seal, idAgg)
        
        let msg = 
            {
                Enveloppe= seal idAgg 0
                PayLoad = Commands.PlanifierSequence(signataires,document)
            } 
        
        (cmdProcessor:>MsgProcessor<Commands>).Post(msg)

        let timeOut = DateTime.Now.AddSeconds(5 |> float)
        while closureval do
            if DateTime.Now>timeOut then Assert.Equal("time Out", "boom!")
            Threading.Thread.Sleep(new TimeSpan(0,0,0,0,500)) 
            
        logger.Debug ""
        logger.Debug ""
        logger.Debug ""

    