// Creates a deployment for a service bus application using managed identity to connect to the service bus.

open Farmer
open Farmer.Builders
open Farmer.Arm.RoleAssignment

// Create an Azure Service Bus Namespace
let sb = serviceBus {
    name "farmer-service-bus"
    sku ServiceBus.Sku.Standard // Standard SKU so topics are available
    add_topics [
        topic {
            name "foo"
        }
    ]
}

// The managed identity that the application will use to connect to the service bus topic.
let sbUser = createUserAssignedIdentity "sbUser"

// Create a role assignment to give the identity access to the service bus in this resource group.
let sbAppRole = {
    Name = (sprintf "guid(concat(resourceGroup().id, '%O'))" Roles.AzureServiceBusDataSender.Id
                |> ArmExpression.create).Eval()
                |> ResourceName
    PrincipalId = sbUser.PrincipalId
    PrincipalType = PrincipalType.ServicePrincipal
    Scope = ResourceGroup
    RoleDefinitionId = Roles.AzureServiceBusDataSender // Needed to send messages.
    Dependencies = Set.empty
}

// Load an F# script that uses the dotnet 5.0 fsi to load necessary libraries to connect to the service bus.
let script = System.IO.File.ReadAllText "main.fsx"
printfn "%s" script

// Create a container group that runs the dotnet fsi script and runs under the managed identity with service bus access.
let sbApp = containerGroup {
    name "sb-client-app"
    add_identity sbUser
    add_instances [
        containerInstance {
            name "fsi"
            image "mcr.microsoft.com/dotnet/sdk:5.0.102"
            add_volume_mount "script-source" "/app/src"
            command_line ("dotnet fsi /app/src/main.fsx".Split null |> List.ofArray)
        }
    ]
    add_volumes [
        // A secret volume can be used to mount a base64 encoded string as a file in the container group.
        // This one will hold the F# script that connects to the service bus.
        volume_mount.secret_string "script-source" "main.fsx" script
    ]
}

let deployment = arm {
    location Location.EastUS
    add_resources [
        sb
        sbUser
        sbApp
    ]
    add_resource sbAppRole
}

[<EntryPoint>]
let main _ =
    deployment |> Writer.quickWrite "sb-app"
    0
