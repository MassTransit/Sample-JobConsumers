FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/JobService.Sagas/JobService.Sagas.csproj", "src/JobService.Sagas/"]

RUN dotnet restore "src/JobService.Sagas/JobService.Sagas.csproj"
COPY . .
WORKDIR "/src/src/JobService.Sagas"
RUN dotnet build "JobService.Sagas.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JobService.Sagas.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobService.Sagas.dll"]
