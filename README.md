ServiceBus with Managed Identity
=========

This uses Farmer to generate an ARM template that will deploy an application that connects to the Azure Service Bus using managed identity.

It deploys the following resources:

* Service Bus namespace
* User assigned identity that can be assigned to the application
* Role assignment to grant that identity access to publish to the service bus
* Container group that runs under that identity. An F# script is deployed in a secret volume on the container group.

After deployment, connect to the container to view logs of the script running, or subscribe to the "events" topic on the service bus to see messages sent by the script.
