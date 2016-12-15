namespace CommandHandlerTests

open System 
open Xunit
open EClosing.Core.Domain.Types
open EClosing.Core.Domain.Sequence
open EClosing.Core.Domain.CommandProcessingAgent


module Tests =

    
    
    [<Fact>]
    let ``Quand je reussit l'execution d'une commande , je sauve un evenement`` ()=
        
        let logger  = 
            {
                Debug = fun s -> ()//Console.WriteLine(s)
            }

        logger.Debug "Quand je reussit l'execution d'une commande , je sauve un evenement"
        let idAgg = Guid.NewGuid()
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())

        let hydrate id version= 
            async {
                return State.Initial
            }
            

        let mutable closureval = true

        let save id version evts =
            async {
                Assert.Equal(idAgg,id)
                Assert.Equal<Events list>([SequencePlanifiee(signataires,document)], evts)
                closureval<- false
            }
            
        
        let log id version cmd reason=
            async {
                Assert.Empty(reason)
                Assert.False(true)
            }
            

        let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
            { 
                MessageId = Guid.NewGuid()
                CorrelationId = Guid.NewGuid()
                AggregateId=id
                Version = version
            }
            
        

        let cmdProcessor = new CommandProcessor<Commands,State,Events>(logger, save,log,execute,apply,seal, State.Initial,0, idAgg)
        
        let msg = 
            {
                Enveloppe= seal idAgg 0
                PayLoad = Commands.PlanifierSequence(signataires,document)
            } 
        
        (cmdProcessor:>MsgProcessor<Commands>).Post(msg)

        let timeOut = DateTime.Now.AddSeconds(5 |> float)
        while closureval do
            if DateTime.Now>timeOut then Assert.Equal("time Out", "boom!")
            Threading.Thread.Sleep(new TimeSpan(0,0,0,0,10)) 
            
        logger.Debug ""
        logger.Debug ""
        logger.Debug ""

    
    [<Fact>]
    let ``Quand l'execution d'une commande échoue, je logue une erreur`` ()=

        let logger  = 
            {
                Debug = fun s -> ()//Console.WriteLine(s)
            }

        logger.Debug "Quand l'execution d'une commande échoue, je logue une erreur"
        
        let idAgg = Guid.NewGuid()
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())

        let state = { State.Initial with EstCommmencee=true}

        let mutable closureval = true

        let save id v evts =
            async {
                Assert.False(true)    
            }
        
        let log id version cmd reason=
            async {
                Assert.Equal(idAgg,id)
                Assert.Equal(1,version)
                Assert.Equal("Sequence déjà commencée", reason)
                closureval<- false
            }
            

        let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
            { 
                MessageId = Guid.NewGuid()
                CorrelationId = Guid.NewGuid()
                AggregateId=id
                Version = version
            }

        

        let cmdProcessor = new CommandProcessor<Commands,State,Events>(logger,save,log,execute,apply,seal,state,1, idAgg)
        
        let msg = 
            {
                Enveloppe= seal idAgg 0
                PayLoad = Commands.PlanifierSequence(signataires,document)
            } 
        
        (cmdProcessor:>MsgProcessor<Commands>).Post(msg)

        let timeOut = DateTime.Now.AddSeconds(5 |> float)
        while closureval do
            if DateTime.Now>timeOut then Assert.Equal("time Out", "boom!")
            Threading.Thread.Sleep(new TimeSpan(0,0,0,0,10))

        logger.Debug ""
        logger.Debug ""
        logger.Debug ""
        

    [<Fact>]
    let ``Quand je recupere un agent, je cree un agent`` ()=

        let logger  = 
            {
                Debug = fun s -> ()//Console.WriteLine(s)
            }
            
        logger.Debug "Quand je reussit l'execution d'une commande , je sauve un evenement"
        let idAgg = Guid.NewGuid()
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())

        let hydrate id version= State.Initial

        let mutable closureval = true

        let save id v evts =
            async {
                Assert.Equal(idAgg,id)
                Assert.Equal<Events list>([SequencePlanifiee(signataires,document)], evts)
                closureval<- false
            }
            
        
        let log id version cmd reason=
            async {
                Assert.Empty(reason)
                Assert.False(true)
            }
            

        let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
            { 
                MessageId = Guid.NewGuid()
                CorrelationId = Guid.NewGuid()
                AggregateId=id
                Version = version
            } 

        let cmdProcessor = new CommandProcessor<Commands,State,Events>(logger, save,log,execute,apply,seal,State.Initial, 0, idAgg)
        
        let msg = 
            {
                Enveloppe= seal idAgg 0
                PayLoad = Commands.PlanifierSequence(signataires,document)
            } 
        
        (cmdProcessor:>MsgProcessor<Commands>).Post(msg)

        let timeOut = DateTime.Now.AddSeconds(5 |> float)
        while closureval do
            if DateTime.Now>timeOut then Assert.Equal("time Out", "boom!")
            Threading.Thread.Sleep(new TimeSpan(0,0,0,0,10)) 
            
        logger.Debug ""
        logger.Debug ""
        logger.Debug ""

    