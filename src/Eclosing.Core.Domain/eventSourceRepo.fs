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

    

    type Repository(eventStoreConnectionString, eventStoreUserName, eventStorePassword) =
        member this.Connect() =
            ()

//        let mutable status = Closed
//        let defaultUserCredentials = new SystemData.UserCredentials(eventStoreUserName,eventStorePassword)
//        let settingsBuilder = ConnectionSettings.Create()
//                                         .KeepRetrying()
//                                         .KeepReconnecting()
//    //                                     .SetDefaultUserCredentials(defaultUserCredentials)
//    //                                     .SetOperationTimeoutTo(TimeSpan.FromSeconds(5 |> float))
//    //                                     .EnableVerboseLogging()
//    //                                     .FailOnNoServerResponse()
//    //                                     .SetReconnectionDelayTo(TimeSpan.FromSeconds(15 |> float))
                                    
   
//        let conn =  EventStore.ClientAPI.EventStoreConnection.Create(settingsBuilder.Build(),new Uri(eventStoreConnectionString),"Bear2BearESConnection")
//        let createNameAgg streamName id = sprintf "%s-%s" streamName (id.ToString())

//        let subscriptionsFactory = new Dictionary<Guid, unit -> EventStoreStreamCatchUpSubscription>()
//        let activeSubscriptions = new Dictionary<Guid, EventStoreStreamCatchUpSubscription>()
   
//        let onClosed (evtArgs:ClientClosedEventArgs) = 
//            status <- Closed
//        let onConnected (evtArgs:ClientConnectionEventArgs) = 
//            status <- Connected

//        let onDisconnected (evtArgs:ClientConnectionEventArgs) = 
//            status <- Closed

//        let onErrorOccurred (evtArgs:ClientErrorEventArgs) =  status <- Closed
   
//        let onReconnecting (evtArgs:ClientReconnectingEventArgs) = () 

//        let runSubscription subscriptionId = 
//            if activeSubscriptions.ContainsKey subscriptionId then
//                activeSubscriptions.[subscriptionId].Stop()
//                activeSubscriptions.Remove(subscriptionId)|> ignore

//            if subscriptionsFactory.ContainsKey(subscriptionId) then
//                activeSubscriptions.Add(subscriptionId,subscriptionsFactory.[subscriptionId]()) 
   
       
//        let onError subscriptionId onerror (escus:EventStoreCatchUpSubscription) (sdr:SubscriptionDropReason) (e:Exception) = 
//            onerror escus sdr e
   
        

//         member this.Connect()=
//             conn.Closed.Add(onClosed)
//             conn.Connected.Add(onConnected)
//             conn.Disconnected.Add(onDisconnected)
//             conn.ErrorOccurred.Add(onErrorOccurred)
//             conn.Reconnecting.Add(onReconnecting)

//             conn.ConnectAsync().Wait()
//             status <- Connected
//             conn

//         member this.IsCommandProcessed idCommand =
        

//             let sql = " select count(*) from commandProcessed where commandId=@cmdId"
//             use sqlCmd = new SQLiteCommand(sql, connection) 

//             let add (name:string, value: string) = 
//                 sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

//             add("@cmdId", idCommand.ToString())

//             use reader = sqlCmd.ExecuteReader() 

//             (reader.Read() && Int32.Parse(reader.[0].ToString())>0)

//         member this.SaveCommandProcessedAsync (cmd:Command<_>) = async {

//             let sql = " insert into  commandProcessed (commandId) values(@cmdId);"
//             use sqlCmd = new SQLiteCommand(sql, connection) 

//             let add (name:string, value: string) = 
//                 sqlCmd.Parameters.Add(new SQLiteParameter(name,value)) |> ignore

//             add("@cmdId", cmd.idCommand.ToString())

//             sqlCmd.ExecuteNonQuery()  |> ignore

//             sqlCmd.Dispose()
//         }
    
//         member this.SaveEvtsAsync  name  enveloppe evts =  async {
            
//                 let envInitial = enveloppe 0;

//                 let events = evts
//                                 |> List.mapi (fun index e ->
//                                             let typeEvts = e.GetType().Name
//                                             let data=  toJson e 
//                                             let enveloppeMetadata = toJson(enveloppe(index))
//                                             new EventData(envInitial.aggregateId,typeEvts,true, data,enveloppeMetadata)
//                                             )


//                 let expectedVersion = envInitial.version
//                 let streamName = createNameAgg name (envInitial.aggregateId.ToString())


            
//                 let! writeResult = conn.AppendToStreamAsync(streamName,expectedVersion,events) |> Async.AwaitTask

//                 ()

//             }

//         member this.HydrateAggAsync<'TAgg,'TEvent>  streamName (applyTo:'TAgg -> 'TEvent ->'TAgg) (initialState:'TAgg)  (id:Guid) =

        
//             let rec readEventsSequentially start step state = async {

            
//                 let n = createNameAgg streamName id

//                 let! slice = conn.ReadStreamEventsForwardAsync(n,start,step,false) |> Async.AwaitTask

//                 let evts =
//                     slice.Events
//                     |> Seq.map (fun (e:ResolvedEvent) -> JsonConvert.DeserializeObject<'TEvent>(System.Text.Encoding.UTF8.GetString(e.Event.Data)))
//                     |> Seq.toList
            
//                 let (aggregate:'TAgg) = List.fold applyTo state evts

//                 if (slice.IsEndOfStream) then 
//                     return aggregate,slice.NextEventNumber-1
//                 else return! readEventsSequentially slice.NextEventNumber step aggregate
//             }

//             readEventsSequentially 0 99 initialState
           

   

 
       
//         member this.SubscribeToStreamFrom name (lastCheckPoint:Nullable<int>) (resolveLinkTo:bool) (projection:Projection) =

//             let subscriptionId = Guid.NewGuid()
        
//             let evtAppeared =projection.eventAppeared connection
//             let evtAppeared = Action<EventStoreCatchUpSubscription,ResolvedEvent> evtAppeared
//             let catchUp = Action<EventStoreCatchUpSubscription> projection.catchup
//             let error = Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> (onError subscriptionId projection.onError)

//             subscriptionsFactory.Add(subscriptionId , (fun () -> 
//                 conn.SubscribeToStreamFrom(name, lastCheckPoint, resolveLinkTo, evtAppeared, catchUp, error, defaultUserCredentials, 500) )
//             )

//             //deals with reconnection
//             let onReconnection (evArgs:ClientReconnectingEventArgs) = () 

//             conn.Reconnecting.Add onReconnection
//             runSubscription subscriptionId
   
//        interface IDisposable with 
//            member this.Dispose()  =
//                for entry in activeSubscriptions do entry.Value.Stop()
           

//    let create dbConnection cs username password = (new Repository(dbConnection,cs,username,password))
