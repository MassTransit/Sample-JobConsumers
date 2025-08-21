FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/JobService.Service/JobService.Service.csproj", "src/JobService.Service/"]
COPY ["src/JobService.Components/JobService.Components.csproj", "src/JobService.Components/"]
COPY ["src/JobService.Contracts/JobService.Contracts.csproj", "src/JobService.Contracts/"]

RUN dotnet restore "src/JobService.Service/JobService.Service.csproj"
COPY . .
WORKDIR "/src/src/JobService.Service"
RUN dotnet build "JobService.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JobService.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobService.Service.dll"]
