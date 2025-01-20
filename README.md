# netssh101

### Server Credentials
instead of setting the server credentials in the appsettings.json, we can store the credentials in the environment variables
set the environment variables:

```
ASPNETCORE_Source__Username={your-source-server-username}
ASPNETCORE_Source__Password={your-source-server-password}
ASPNETCORE_Destination__Username={your-destination-server-username}
ASPNETCORE_Destination__Password={your-destination-server-password}
```

##### Note:
set the `Source:Username`, `Source:Password`, `Destination:Username`, `Destination:Password` in the `appsettings.json` will override the environment variables