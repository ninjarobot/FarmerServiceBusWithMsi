#r "nuget: Azure.Messaging.ServiceBus"
#r "nuget: Azure.Identity, 1.3.0"

open Azure.Identity
open Azure.Messaging.ServiceBus

let connectAndPublish (endpoint:string) =
    let sbClient = ServiceBusClient(endpoint, DefaultAzureCredential())
    let msg = ServiceBusMessage($"Hello {System.DateTime.Now}")
    msg.ContentType <- "text/plain"
    sbClient.CreateSender("foo").SendMessageAsync msg |> Async.AwaitTask |> Async.RunSynchronously

while true do
    try
        printfn "About to connect and send message."
        connectAndPublish "farmer-service-bus.servicebus.windows.net"
        System.Threading.Thread.Sleep (System.TimeSpan.FromSeconds 30.)
        printfn "Sent message."
    with ex -> eprintfn "Error sending message: %O" ex
