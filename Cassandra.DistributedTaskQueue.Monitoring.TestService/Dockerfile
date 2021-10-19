FROM mcr.microsoft.com/dotnet/aspnet:6.0.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0.101 AS build
WORKDIR /src
COPY ["Cassandra.DistributedTaskQueue.Monitoring.TestService/Cassandra.DistributedTaskQueue.Monitoring.TestService.csproj", "Cassandra.DistributedTaskQueue.Monitoring.TestService/"]
RUN dotnet restore "Cassandra.DistributedTaskQueue.Monitoring.TestService/Cassandra.DistributedTaskQueue.Monitoring.TestService.csproj"
COPY . .
WORKDIR "/src/Cassandra.DistributedTaskQueue.Monitoring.TestService"
RUN dotnet build "Cassandra.DistributedTaskQueue.Monitoring.TestService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cassandra.DistributedTaskQueue.Monitoring.TestService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TestService.dll"]
