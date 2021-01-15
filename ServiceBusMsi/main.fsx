#r "nuget: Microsoft.Azure.ServiceBus"

open Microsoft.Azure.ServiceBus
open Microsoft.Azure.ServiceBus.Primitives

let connectAndPublish (endpoint:string) =
    let topicClient = TopicClient(endpoint, "foo", ManagedIdentityTokenProvider())
    let msg = Message(sprintf "Hello %O" System.DateTime.Now |> System.Text.Encoding.UTF8.GetBytes )
    msg.ContentType <- "text/plain"
    topicClient.SendAsync msg |> Async.AwaitTask |> Async.RunSynchronously

while true do
    try
        printfn "About to connect and send message."
        connectAndPublish "farmer-service-bus.servicebus.windows.net"
        System.Threading.Thread.Sleep (System.TimeSpan.FromSeconds 30.)
        printfn "Sent message."
    with ex -> eprintfn "Error sending message: %O" ex
