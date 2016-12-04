namespace EClosing.Core.Domain

open System



module Persistence =

    type Memory<'a>() =
        let dict = new  System.Collections.Generic.Dictionary<Guid,'a> ()
        
        member this.get id = 
            if dict.ContainsKey(id) then Some(dict.[id])
            else None
        
        member this.set id agg =
            if (dict.ContainsKey(id)) then dict.[id] <- agg
            else dict.Add(id, agg)
               

    