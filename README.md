# LogAnalytics Client for .NET Core

The easiest way to send logs to Azure Log Analytics from your apps.
Construct a custom object and send it to Log Analytics. It will be represented as a log entry in the logs. This helps make logging easy in your applications, and you can focus on more important business logic.

## Build Status
[![CI - Integration Tests](https://github.com/Zimmergren/LogAnalytics.Client/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/Zimmergren/LogAnalytics.Client/actions/workflows/publish-nuget.yml)
[![CodeQL](https://github.com/Zimmergren/LogAnalytics.Client/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/Zimmergren/LogAnalytics.Client/actions/workflows/github-code-scanning/codeql)
[![Defender for DevOps - GitHub](https://github.com/Zimmergren/LogAnalytics.Client/actions/workflows/defenderfordevops.yaml/badge.svg)](https://github.com/Zimmergren/LogAnalytics.Client/actions/workflows/defenderfordevops.yaml)

## NuGet

The [LogAnalytics.Client](https://www.nuget.org/packages/loganalytics.client) is available on NuGet.

## Support for .NET 6

The LogAnalytics.Client project has been upgraded to .NET 6, and bumped the major version to 6.x. 

LogAnalytics.Client currently support the below versions of .NET.

| Version | Supported          |
| ------- | ------------------ |
| .NET 6.x   | :white_check_mark: |
| .NET 5.x   | :white_check_mark: |
| .NET Core 3.x | :white_check_mark: |

## How to use the LogAnalytics Client

### Installing the package

#### Install with the dotnet CLI

```
dotnet add package LogAnalytics.Client
```

#### Install with NuGet Package Manager

```
Install-Package LogAnalytics.Client
```

#### Install by adding a PackageReference to csproj

```xml
<PackageReference Include="LogAnalytics.Client" Version="6.2.3" />
```

#### Install using Paket CLI

```
paket add LogAnalytics.Client
```

### Initialize the LogAnalyticsClient

Initialize a new `LogAnalyticsClient` object with a Workspace Id and a Key:
```csharp
LogAnalyticsClient logger = new LogAnalyticsClient(
                workspaceId: "LAW ID",
                sharedKey: "LAW KEY");
```

### Send a single log entry

Synchronous execution (non-HTTP applications):
```csharp
logger.SendLogEntry(new TestEntity
{
    Category = GetCategory(),
    TestString = $"String Test",
    TestBoolean = true,
    TestDateTime = DateTime.UtcNow,
    TestDouble = 2.1,
    TestGuid = Guid.NewGuid()
}, "demolog").Wait();
```

Asynchronous execution (HTTP-based applications):
```csharp
await logger.SendLogEntry(new TestEntity
{
    Category = GetCategory(),
    TestString = $"String Test",
    TestBoolean = true,
    TestDateTime = DateTime.UtcNow,
    TestDouble = 2.1,
    TestGuid = Guid.NewGuid()
}, "demolog")
.ConfigureAwait(false); // Optionally add ConfigureAwait(false) here, depending on your scenario
```

### Send a batch of log entries with one request

If you need to send a lot of log entries at once, it makes better sense to send them as a batch/collection instead of sending them one by one. This saves on requests, resources and eventually costs. 

```csharp
// Example: Wiring up 5000 entities into an "entities" collection.
List<DemoEntity> entities = new List<DemoEntity>();
for (int ii = 0; ii < 5000; ii++)
{
    entities.Add(new DemoEntity
    {
        Criticality = GetCriticality(),
        Message = "lorem ipsum dolor sit amet",
        SystemSource = GetSystemSource()
    });
}

// Send all 5000 log entries at once, in a single request.
await logger.SendLogEntries(entities, "demolog").ConfigureAwait(false);
```

### Ship logs to another Azure Sovereign Cloud

To send logs to another Azure Sovereign Cloud, for example the Government cloud, you can specify an enum value for the the optional parameter `azureSovereignCloud`.

Here's an example: 

```csharp
LogAnalyticsClient logger = new LogAnalyticsClient(
                workspaceId: "LAW ID",
                sharedKey: "LAW KEY",
                EndPointOverride: "ods.opinsights.azure.us"); // Use Azure Government instead of the (default) Azure Public cloud.
```

The available sovereign clouds are currently:

- Azure Public Cloud (Commercial): ods.opinsights.azure.com
- Azure Government: ods.opinsights.azure.us
- Azure China: ods.opinsights.azure.cn

## Development 

If you want to develop the project locally and enhance it with your custom logic, or want to contribute to the GitHub repository with a PR, it's a good idea to verify that the code works and tests are flying. 

### Configure Tests secrets

If you want to develop and run local tests, it is a good idea to set up your custom Log Analytics Workspace Id and Key in the project. This can be done using user secrets.

Using the `dotnet` CLI from the `LogAnalyticsClient.Tests` project directory:
```
dotnet user-secrets set "LawConfiguration:LawId" "YOUR LOG ANALYTICS INSTANCE ID"
dotnet user-secrets set "LawConfiguration:LawKey" "YOUR LOG ANALYTICS WORKSPACE KEY"
dotnet user-secrets set "LawServicePrincipalCredentials:ClientId" "CLIENT ID HERE"
dotnet user-secrets set "LawServicePrincipalCredentials:ClientSecret" "CLIENT SECRET HERE"
dotnet user-secrets set "LawServicePrincipalCredentials:Domain" "TENANT NAME OR DOMAIN ID HERE"
``` 

You should now have a `secrets.json` file in your local project, with contents similar to this:

```json
{
  "LawConfiguration": {
    "LawId": "YOUR LOG ANALYTICS INSTANCE ID",
    "LawKey": "YOUR LOG ANALYTICS WORKSPACE KEY"
  },
  "LawServicePrincipalCredentials": {
    "ClientId": "Principal Client ID Here",
    "ClientSecret": "Principal Client Secret Here",
    "Domain": "Your tenant guid here, or tenant name"
  }
}
```

#### Where can I get the credentials?

To add the secrets with the correct values, you need to add the Workspace Id (LawId), and the Key. 
You can find these from the Log Analytics Workspace in the Azure Portal, for example.

To add the Service Principal secrets, you should create a new service principal and add a secret, then grant it reader access on the Log Analytics resource. 
There are steps outlines for that part here: https://zimmergren.net/retrieve-logs-from-application-insights-programmatically-with-net-core-c/ - refer to "Step 1".


Read more about configuring user secrets in .NET Core projects: https://docs.microsoft.com/aspnet/core/security/app-secrets

## Additional notes

This project is a spin-off from code samples. The examples and usage grew over time, and an interest was shown of a wrapper or client for Log Analytics.

Related blog posts:

- https://zimmergren.net/building-custom-data-collectors-for-azure-log-analytics/
- https://zimmergren.net/log-custom-application-security-events-log-analytics-ingested-in-azure-sentinel/

Keeping it simple.
