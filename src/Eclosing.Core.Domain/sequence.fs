namespace EClosing.Core.Domain

open System
open System.Linq


/// correspond a une sequence de signataire unitaire d'un document par plusieurs signataires concommitents

module Sequence =

   
   
   type Signataire = | Signataire of Guid
   type Document = | Document of Guid
   
   type State =
       {
           Signataires : Signataire list
           Document : Document
           EstCommmencee : bool
       }
       with static member Initial = { Signataires=[]; Document = Document(Guid.NewGuid()); EstCommmencee = false }


   type Commands =
   | PlanifierSequence of Signataire list * Document
   | AjouterSignataire of Signataire
   | EnleverSignataire of Signataire
   | CommencerSequence

   type Events =
   | SequencePlanifiee of Signataire list * Document
   | SignataireAjoute of Signataire
   | SignataireEnleve of Signataire
   | SequenceCommencee
   | SequenceNonCommenceeParManqueDeSignataire
   | SequenceDejaCommencee

   // module private Assert =

   //     let valideSequenceNonCommencee = validator (fun s-> not <| s.EstCommmencee ) [SequenceDejaCommencee] 
   //     let valideSequenceAvecSignataire = validator (fun s-> s.Signataires.Any() ) [SequenceNonCommenceeParManqueDeSignataire]

   //     let validCommenceSequence state =
   //          valideSequenceNonCommencee state
   //          <* valideSequenceAvecSignataire state

       // let validScheduleGame (command:Contracts.ScheduleGame) state =
       //     validator (fun (cmd:Contracts.ScheduleGame) -> cmd.location <> ""  ) [GamesText.locationUnknow] command
       //     <* validator (fun g -> not<| g.isOpenned ) [GamesText.gameAlreadyScheduled] state
       
       // let validJoinGame bear state = 
       //     validAction state
       //     <* validator (fun g -> g.startDate>DateTime.Now    ) ["err:the game has already started"] state
       //     <* validator (fun (b,g) -> not <| (g.lineUp |> Seq.append g.bench |> Seq.exists (fun x -> x=b.bearId))) ["err:this bear is already part of the game"] (bear,state)
   let execute state command=
       match command with 
       | PlanifierSequence(s,d) -> Choice1Of2([SequencePlanifiee(s,d)])
       | AjouterSignataire(s) -> Choice1Of2([SignataireAjoute(s)])
       | EnleverSignataire(s) -> Choice1Of2([SignataireEnleve(s)])
       | CommencerSequence -> 
           if state.EstCommmencee then Choice1Of2( [SequenceDejaCommencee])
           elif not <| state.Signataires.Any() then Choice1Of2( [SequenceNonCommenceeParManqueDeSignataire])
           else Choice1Of2( [SequenceCommencee])

            
   let apply state event  =
       match event with 
       | SequencePlanifiee(s,d) -> { state with Signataires= s; Document =d }
       | SignataireAjoute(s) ->   { state with Signataires= s::state.Signataires }
       | SignataireEnleve(s) -> { state with Signataires = state.Signataires |> List.filter ( fun x -> x<> s ) }
       | SequenceCommencee ->{state with EstCommmencee = true } 
       

