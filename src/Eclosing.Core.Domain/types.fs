namespace EClosing.Core.Domain

open System

[<AutoOpen>]
module Types =

    type Agent<'a> = MailboxProcessor<'a>

    type Logger = 
        {
            Debug : string -> unit
        }
    
    type Enveloppe = 
        {
            MessageId:Guid
            CorrelationId : Guid
            AggregateId :Guid
            Version : int
        }

    type Message<'a> = 
        {
            Enveloppe : Enveloppe
            PayLoad: 'a
        }

    type  Persistence<'a> =
        {
            get : Guid -> 'a option
            set : Guid -> 'a -> unit
        }
