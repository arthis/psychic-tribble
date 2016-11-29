namespace SequenceTests

open System 
open Xunit
open EClosing.Core.Domain.Sequence

module Tests =
    let equals<'a,'b> (a:Choice<'a,'b>) (b:Choice<'a,'b>) =
        match a,b with
        | Choice1Of2(x), Choice1Of2(y) -> Assert.Equal<'a>(x,y)
        | Choice2Of2(x), Choice2Of2(y) ->  Assert.Equal<'b>(x,y)
        | _ -> Assert.False(true)
  
    let Add x y =
        x+y
    
    [<Fact>]
    let ``dummmytest`` ()=  
        let result = Add 2 2
        Assert.Equal(4, result)
    
    [<Fact>]
    let ``Etant donne une sequence, quand je planifie une sequence, j'obtiens une sequence planifiée `` ()=
        
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())

        let evts =  
            Commands.PlanifierSequence(signataires,document)
            |> execute State.Initial 
        
        equals evts <| Choice1Of2([SequencePlanifiee(signataires,document)]) 

    
    [<Fact>]
    let ``Etant donne une sequence planifiée, quand j'ajoute un signataire, j'obtiens un signataire ajouté`` ()=
        
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())

        let newSignataire = Signataire(Guid.NewGuid())

        let pastEvts = [SequencePlanifiee(signataires,document)]

        let evts = 
            Seq.fold apply State.Initial pastEvts
            |> execute
            <| Commands.AjouterSignataire(newSignataire)

        equals evts <| Choice1Of2([SignataireAjoute(newSignataire)])


    
    [<Fact>]
    let ``Etant donne une sequence planifiée et un signatire ajouté, quand j'enlève un signataire, j'obtiens un signataire enlevé`` ()=
        
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())

        let newSignataire = Signataire(Guid.NewGuid())

        let pastEvts = 
            [
                SequencePlanifiee(signataires,document)
                SignataireAjoute(newSignataire)
            ]

        let evts = 
            Seq.fold apply State.Initial pastEvts
            |> execute
            <| Commands.EnleverSignataire(newSignataire) 
        
        equals evts <| Choice1Of2([SignataireEnleve(newSignataire)])


    [<Fact>]
    let ``Etant donne une sequence planifiée et un signatire ajouté, quand je commence la sequence, j'obtiens une sequence commencee`` ()=
        
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())

        let newSignataire = Signataire(Guid.NewGuid())

        let pastEvts = 
            [
                SequencePlanifiee(signataires,document)
                SignataireAjoute(newSignataire)
            ]

        let evts = 
            Seq.fold apply State.Initial pastEvts
            |> execute
            <| Commands.CommencerSequence 
        
        equals evts <| Choice1Of2([SequenceCommencee])

    [<Fact>]
    let ``Etant donne une sequence planifiée et un signatire enlevé, quand je commence la sequence, j'obtiens une sequence non commencee par manque de sighnataire`` ()=
        
        let newSignataire = Signataire(Guid.NewGuid())

        let document = Document(Guid.NewGuid())

        let pastEvts = 
            [
                SequencePlanifiee([ newSignataire],document)
                SignataireEnleve(newSignataire)
            ]

        let evts = 
            Seq.fold apply State.Initial pastEvts
            |> execute
            <| Commands.CommencerSequence 
        
        equals evts <| Choice1Of2([SequenceNonCommenceeParManqueDeSignataire])

    
    [<Fact>]
    let ``Etant donne une sequence planifiée et commencée, quand je commence la sequence, j'obtiens une sequence déja commencee`` ()=
        
        let signataires = [ Signataire(Guid.NewGuid())]
        let document = Document(Guid.NewGuid())

        [
            SequencePlanifiee(signataires,document)
            SequenceCommencee
        ] 
        |> Seq.fold apply State.Initial 
        |> execute
        <| Commands.CommencerSequence 
        |> equals 
        <| Choice1Of2([SequenceDejaCommencee]) 
        