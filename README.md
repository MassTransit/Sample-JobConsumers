# Job Consumer Sample

To run the app update the connection strings within appsettings.json and run the service application.

Navigate to https://localhost:5001 and hit the POST.

``` json
  "ConnectionStrings": {
    "AzureServiceBus": "<ADDASBConnectionStringHere>",
    "AzureTable": "<AddTableConnectionStringHere>"
  }
```