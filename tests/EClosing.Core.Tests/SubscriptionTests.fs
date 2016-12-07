namespace SubscriptionTests

open System 
open Xunit
open EClosing.Core.Domain.Types
open EClosing.Core.Domain.Sequence
open EClosing.Core.Domain.CommandProcessingAgent
open EClosing.Core.Domain.Persistence
open EClosing.Core.Domain.RabbitMq
open RabbitMQ.Client 
   

module Tests =

    let logger  = 
        {
            Debug = fun s -> () //Console.WriteLine(s)
        }

    
        

    
    [<Fact>]
    let ``Quand je souscrit à un exchange et un routing key , j'execute la fonction associée`` ()=
        logger.Debug "Quand je souscrit à un exchange et un routing key , j'execute la fonction associée"
        
        let idAgg = Guid.NewGuid()

        let seal (id:Guid) (version:int) : EClosing.Core.Domain.Types.Enveloppe = 
            { 
                MessageId = Guid.NewGuid()
                CorrelationId = Guid.NewGuid()
                AggregateId=id
                Version = version
            }

        let msg = 
            {
                Enveloppe= seal idAgg 0
                PayLoad = "some message of hope"
            }  

        let mutable subscriptionNotFound= true
        let react (message: Message<string>) =
            Assert.Equal(msg,message)
            subscriptionNotFound <- false
            ()


        let factory = new ConnectionFactory()
        factory.HostName <- "localhost"
        factory.UserName <- "guest"
        factory.Password <- "guest"
        factory.VirtualHost <- "/"
        factory.Protocol <- Protocols.AMQP_0_9_1
        factory.AutomaticRecoveryEnabled <- true
        factory.RequestedHeartbeat <- 60 |> uint16
          
        use conn = factory.CreateConnection() 
        
        let exchangeName =  sprintf "MyExchangeName%A" (Guid.NewGuid())
        let routingKey =  sprintf "MyRoutingKey%A" (Guid.NewGuid())

        use subscriptionChannel = subscribe logger "testApp" conn exchangeName routingKey react

        publish logger "testApp" conn exchangeName routingKey msg

        let timeOut = DateTime.Now.AddSeconds(5 |> float)

        while (subscriptionNotFound && DateTime.Now<timeOut) do
            Threading.Thread.Sleep(new TimeSpan(0,0,0,0,500))
            
        Assert.False(subscriptionNotFound)

        logger.Debug ""
        logger.Debug ""
        logger.Debug ""