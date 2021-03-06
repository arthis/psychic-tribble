namespace EClosing.Core.Domain

open System


/// correspond a une sequence de signataire unitaire d'un document par plusieurs signataires concommitents

module CommandProcessingAgent =

    type MsgProcessor<'a> =
        abstract member Post : Message<'a> -> unit

    

    // type CommandProcessor<'a>(hydrate, save, log, execCmd, applyTo, enveloppe) =
    type CommandProcessor<'a,'b,'c>(l : Logger,save: Guid -> int-> 'c list-> Async<unit>,log: Guid-> int-> Message<'a>  ->string -> Async<unit>, execCmd :'b -> 'a -> Choice<'c list,string> , applyTo : 'b-> 'c -> 'b, seal: Guid-> int -> Enveloppe, agg : 'b, v : int, id) =
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
                    do! save id newVersion evts

                    return!  loop newState newVersion
                |Choice2Of2(reason) ->
                    sprintf "result reason... %A" reason
                    |> l.Debug
                    do! log id version msg reason
                    return! loop state version 
  
             }
             loop agg v
        )

        member this.AgentId = agentId
        
        interface MsgProcessor<'a> with 
            member this.Post(value) =
                agent.Post(value)
 
    let createCommandProcessor l hydrate save (businessLog: Guid-> int-> Message<'a>  ->string -> Async<unit>) execCmd applyTo seal id =
        async {
            
            let! (agg,version) = hydrate id
            let agent = new CommandProcessor<_,_,_>(l, save,businessLog,execCmd,applyTo,seal,agg,version, id)
            return agent
        }
         
    
    type ConsumerAgent<'a,'b,'c,'d when 'd:> MsgProcessor<'a>>(l : Logger,p:Persistence<'d>,f: Guid ->Async<'d>, discardingMessage : Message<'a> -> unit) =

        let agentId = Guid.NewGuid()
        

        let hydrateAgent id = async {
            match p.get id with
                | Some(a) -> return a
                | None -> 
                    let! a  = f id
                    p.set id a
                    return a
        }
                

        let agent = Agent<Message<'a>>.Start(fun inbox -> 
             let rec loop cmdsProcessed= async {

                let! msg = inbox.Receive()

                sprintf "Consumer agent receiving msg... %A" msg
                |> l.Debug

                if (cmdsProcessed |> List.contains msg.Enveloppe.MessageId) then
                    sprintf "discarding msg... %A" msg
                    |> l.Debug
                    discardingMessage msg
                    return! loop cmdsProcessed     
                
                let! a = hydrateAgent msg.Enveloppe.AggregateId

                a.Post(msg)

                let newCmds = msg.Enveloppe.MessageId::cmdsProcessed

                return! loop newCmds
             }

             loop [] 
              
        )

        member this.AgentId = agentId

        interface MsgProcessor<'a> with 
            member this.Post(value) =
                agent.Post(value)
 
    
