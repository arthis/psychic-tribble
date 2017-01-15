namespace EClosing.Core.Domain

open System
open System.Linq
open RabbitMQ.Client
open RabbitMQ.Client.Framing
open RabbitMQ.Client.Events
open Newtonsoft.Json
open System.Text


/// correspond a une sequence de signataire unitaire d'un document par plusieurs signataires concommitents

module RabbitMq =

    open System.Threading
    open System.Threading.Tasks

    let subscribe<'a> logger appId (conn:IConnection) exchangeName routingKey react =
        sprintf "starting subscription on exchangeName:%s routingKey:%s" exchangeName routingKey 
        |> logger.Debug

        let channel = conn.CreateModel()
        channel.ExchangeDeclare(exchangeName,"direct")
        let queue = channel.QueueDeclare()

        sprintf "subscription queue:%s" queue.QueueName 
        |> logger.Debug

        channel.QueueBind(queue.QueueName,exchangeName,routingKey)

        let consumer = new EventingBasicConsumer(channel)

        consumer.Received.Add(fun (ea) ->
             logger.Debug("receiving message")

             ea.Body
             |> Encoding.UTF8.GetString
             |> JsonConvert.DeserializeObject<'a>
             |> react 
        )
        
        channel.BasicConsume(queue.QueueName,true,consumer) |> ignore
         
        channel

    let publish logger appId (conn:IConnection) exchangeName routingKey msg =
        sprintf "starting publishing on exchangeName:%s routingKey:%s message:%A" exchangeName routingKey msg
        |> logger.Debug

        use channel = conn.CreateModel()
        channel.ExchangeDeclare(exchangeName,"direct")
        let queue = channel.QueueDeclare()

        sprintf "publishing queue:%s" queue.QueueName 
        |> logger.Debug

        channel.QueueBind(queue.QueueName,exchangeName,routingKey)

        

        let props = new BasicProperties()
        props.AppId <- appId 
        props.MessageId <- msg.Enveloppe.MessageId.ToString()
        props.Type <- msg.PayLoad.GetType().FullName

        let message = JsonConvert.SerializeObject(msg)
        let body = Encoding.UTF8.GetBytes(message)
        
        channel.BasicPublish(exchangeName,routingKey,props,body) |> ignore

        logger.Debug("message published") 

    type Dispatcher(hostName, userName,password, appId, logger, exchangeName) =

        let factory = new ConnectionFactory()

        do  
            factory.HostName <- hostName
            factory.UserName <- userName
            factory.Password <- password
            factory.VirtualHost <- "/"
            factory.Protocol <- Protocols.AMQP_0_9_1
            factory.AutomaticRecoveryEnabled <- true
            factory.RequestedHeartbeat <- 60 |> uint16
          
        let conn = factory.CreateConnection()
 
        member this.Subscribe<'a>(routingKey, react: 'a-> unit) =
            subscribe<'a> logger appId conn exchangeName routingKey react
        member this.First<'a>(routingKey, react: 'a-> unit, token: CancellationToken) =
            let tcs = new TaskCompletionSource<'a>()
            let f msg =
                react msg
                tcs.SetResult(msg)

            token.Register(fun (_) ->  tcs.SetCanceled()) |> ignore
            subscribe<'a> logger appId conn exchangeName routingKey f |> ignore
            Async.AwaitTask tcs.Task
            
        member this.Publish(routingKey, msg) =
            publish logger appId conn exchangeName routingKey msg

        interface IDisposable with
            member this.Dispose() =
                conn.Dispose()