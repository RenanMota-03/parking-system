FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .

RUN dotnet publish src/services/webapi/ParkingSystem.WebApi.Bff.WebApp/ParkingSystem.WebApi.Bff.WebApp.csproj \
    -c Release -o /app/webapi --no-self-contained

RUN dotnet publish src/services/workers/MigrateWorker/ParkingSystem.Workers.Migrate.csproj \
    -c Release -o /app/migrate --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/webapi ./webapi
COPY --from=build /app/migrate ./migrate

EXPOSE 8080

WORKDIR /app/webapi
ENTRYPOINT ["dotnet", "ParkingSystem.WebApi.Bff.WebApp.dll"]
