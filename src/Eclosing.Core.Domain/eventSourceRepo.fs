namespace EClosing.Core.Domain

module EventSourceRepo =

    open System
    open System.Threading
    open System.Text
    open System.Threading.Tasks

    open Newtonsoft.Json
    open EventStore.ClientAPI
    open EventStore.ClientAPI.Common.Log

    open System.Collections.Generic

    type Status =
        | Connected
        | Closed

    let createNameAgg streamName id = sprintf "%s-%s" streamName (id.ToString())

    let save (conn:IEventStoreConnection) name id version (evts:Message<_> list) =
        async {
            

            let events = 
                evts
                |> List.mapi (fun index event ->
                    let typeEvts = event.GetType().Name
                    let data=
                        event.PayLoad 
                        |> JsonConvert.SerializeObject
                        |> Encoding.UTF8.GetBytes 
                    let enveloppeMetadata =  
                        event.Enveloppe 
                        |> JsonConvert.SerializeObject
                        |> Encoding.UTF8.GetBytes 
                    new EventData(event.Enveloppe.AggregateId,typeEvts,true, data,enveloppeMetadata)
                )


            let expectedVersion = version
            let streamName = createNameAgg name id

            do! conn.AppendToStreamAsync(streamName,-1,events) 
                |> Async.AwaitTask 
                |> Async.Ignore

        }

    let hydrate (conn:IEventStoreConnection) streamName applyTo initialState id =
        let rec readEventsSequentially start step state = async {

            let n = createNameAgg streamName id

            let! slice = conn.ReadStreamEventsForwardAsync(n,start,step,false) |> Async.AwaitTask

            let evts =
                slice.Events
                |> Seq.map (fun (e:ResolvedEvent) -> JsonConvert.DeserializeObject<'TEvent>(System.Text.Encoding.UTF8.GetString(e.Event.Data)))
                |> Seq.toList

            let (aggregate:'TAgg) = List.fold applyTo state evts

            if (slice.IsEndOfStream) then 
                return aggregate,slice.NextEventNumber-1
            else return! readEventsSequentially slice.NextEventNumber step aggregate
        }

        readEventsSequentially 0 99 initialState
   

    let create eventStoreConnectionString eventStoreUserName eventStorePassword = 
        async {
            let defaultUserCredentials = new SystemData.UserCredentials(eventStoreUserName,eventStorePassword)
            let settingsBuilder = ConnectionSettings.Create()
                                    .KeepRetrying()
                                    .KeepReconnecting()
                                    .SetDefaultUserCredentials(defaultUserCredentials)
                                    .SetOperationTimeoutTo(TimeSpan.FromSeconds(5 |> float))
                                    .EnableVerboseLogging()
                                    .FailOnNoServerResponse()
                                    .SetReconnectionDelayTo(TimeSpan.FromSeconds(15 |> float))

            let conn =  EventStore.ClientAPI.EventStoreConnection.Create(settingsBuilder.Build(),new Uri(eventStoreConnectionString))

            do! conn.ConnectAsync() |> Async.AwaitTask
            
            return conn
        }
