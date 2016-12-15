namespace EClosing.Core.Domain

open System
open RabbitMQ.Client
open EClosing.Core.Domain.RabbitMq
open EClosing.Core.Domain.EventSourceRepo
open EClosing.Core.Domain.Persistence
open EventStore.ClientAPI
open EClosing.Core.Domain.CommandProcessingAgent




module App =

    type App = 
        {
            Dispatcher : Dispatcher
            EventStoreConnection : IEventStoreConnection
            Subscriptions : IModel list  
            Logger : Logger
        }
    let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
        { 
            MessageId = Guid.NewGuid()
            CorrelationId = Guid.NewGuid()
            AggregateId=id
            Version = version
        } 

    let addSubscription<'cmd,'state,'evt> (s:Subscription<'cmd,'state,'evt>) app =
        let memory = new Memory<CommandProcessor<'cmd,'state,'evt>>()
        let memoryPersistence = 
            { 
                get= memory.get
                set= memory.set
            }

        let hydrate = hydrate app.EventStoreConnection s.AggregateName s.Apply s.InitialState
        let save = save app.EventStoreConnection seal s.AggregateName
        let loggerBusiness id version cmd reason = async { 
            sprintf "business error id %A version%A cmd %A reason %A" id version cmd reason
            |> app.Logger.Debug
        }  

        let createProcessor = fun id ->  createCommandProcessor app.Logger hydrate save loggerBusiness s.Execute s.Apply seal id

        let c = new ConsumerAgent<'cmd,'state,'evt,CommandProcessor<'cmd,'state,'evt>>(app.Logger, memoryPersistence, createProcessor)

        let  subscription = app.Dispatcher.Subscribe(s.RoutingKey,fun msg -> (c:>MsgProcessor<'cmd>).Post(msg))

        { app with Subscriptions= subscription::app.Subscriptions } 
 

    let create (dispatcher:Dispatcher) connection logger= 
        { Dispatcher= dispatcher; EventStoreConnection = connection;Subscriptions= []; Logger=logger }

    
        