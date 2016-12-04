namespace EClosing.Core.Domain

open System


/// correspond a une sequence de signataire unitaire d'un document par plusieurs signataires concommitents

module CommandProcessingAgent =

    type MsgProcessor<'a> =
        abstract member Post : Message<'a> -> unit

    

    // type CommandProcessor<'a>(hydrate, save, log, execCmd, applyTo, enveloppe) =
    type CommandProcessor<'a,'b,'c>(l : Logger,hydrate:Guid-> int-> 'b,save:Enveloppe-> Guid -> 'c list-> unit,log, execCmd :'b -> 'a -> Choice<'c list,string> , applyTo : 'b-> 'c -> 'b, seal: Guid-> int -> Enveloppe, id) =
        let agentId = Guid.NewGuid()

        
        let agent = Agent<Message<'a>>.Start(fun inbox -> 
             let rec loop state version= async {

                let! msg = inbox.Receive()

                sprintf "receiving msg... %A" msg
                |> l.Debug
                

                let result = execCmd state msg.PayLoad

                match result with
                |Choice1Of2(evts) ->
                    sprintf "result evts... %A" evts
                    |> l.Debug
                    let newState = List.fold applyTo state evts
                    let newVersion = version + evts.Length
                    
                    let enveloppe = seal id newVersion
                    save  enveloppe id evts

                    loop newState newVersion
                |Choice2Of2(reason) ->
                    sprintf "result reason... %A" reason
                    |> l.Debug
                    log id version msg reason
                    loop state version
  
             }

             let agg = hydrate id 0

             loop agg 0
              
        )

        member this.AgentId = agentId
        
        interface MsgProcessor<'a> with 
            member this.Post(value) =
                agent.Post(value)
 
    let hydrateAgent<'a,'b,'c> (p:Persistence<CommandProcessor<'a,'b,'c>>) (f: Guid ->CommandProcessor<'a,'b,'c>)  id =
        match p.get id with
        | Some(a) -> a
        | None -> 
            let a  = f id
            p.set id a
            a
