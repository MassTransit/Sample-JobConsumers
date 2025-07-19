# Job Consumer Sample

Generate certificate and configure local machine:

PC:

```PowerShell
dotnet dev-certs https --clean
dotnet dev-certs https -ep "$env:USERPROFILE\.aspnet\https\aspnetapp.pfx" -p Passw0rd
dotnet dev-certs https --trust
```

MAC:

```
dotnet dev-certs https --clean
dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p Passw0rd
dotnet dev-certs https --trust
```

Read more here:
https://learn.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-6.0#starting-a-container-with-https-support-using-docker-compose

Use the `docker-compose.yml` file to startup JobService, and PostgreSQL. Or run `docker compose up --build`.    
Navigate to `https://localhost:5003/swagger` to post a video to convert.

The SQL transport is using PostgreSQL in this scenario. To use SQL Server, switch to the `sql-server` branch.

Supports local debugging from VS/Rider. Ctrl + F5 and navigate to:  
`http://localhost:5002/swagger`  
`https://localhost:5003/swagger`

Enjoy the show!
