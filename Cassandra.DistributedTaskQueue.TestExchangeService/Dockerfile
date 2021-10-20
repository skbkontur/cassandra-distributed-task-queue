FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0.401 AS build
WORKDIR /src
COPY ["Cassandra.DistributedTaskQueue.TestExchangeService/Cassandra.DistributedTaskQueue.TestExchangeService.csproj", "Cassandra.DistributedTaskQueue.TestExchangeService/"]
RUN dotnet restore "Cassandra.DistributedTaskQueue.TestExchangeService/Cassandra.DistributedTaskQueue.TestExchangeService.csproj"
COPY . .
WORKDIR "/src/Cassandra.DistributedTaskQueue.TestExchangeService"
RUN dotnet build "Cassandra.DistributedTaskQueue.TestExchangeService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cassandra.DistributedTaskQueue.TestExchangeService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.dll"]
