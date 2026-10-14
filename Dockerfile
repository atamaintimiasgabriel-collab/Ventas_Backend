FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SistemaVentasMotorrepuestos.API.csproj ./
RUN dotnet restore SistemaVentasMotorrepuestos.API.csproj

COPY . ./
RUN dotnet publish SistemaVentasMotorrepuestos.API.csproj --no-restore -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./
RUN mkdir -p /app/App_Data/Facturacion && chown -R $APP_UID:$APP_UID /app

USER $APP_UID
ENTRYPOINT ["dotnet", "SistemaVentasMotorrepuestos.API.dll"]
