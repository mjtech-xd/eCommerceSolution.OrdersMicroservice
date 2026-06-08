FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OrdersMicroserservice.API/OrdersMicroserservice.API.csproj", "OrdersMicroserservice.API/"]
RUN dotnet restore "OrdersMicroserservice.API/OrdersMicroserservice.API.csproj"
COPY . .
WORKDIR "/src/OrdersMicroserservice.API"
RUN dotnet build "./OrdersMicroserservice.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OrdersMicroserservice.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV MONGODB_HOST=localhost
ENV MONGODB_PORT=27017
ENTRYPOINT ["dotnet", "OrdersMicroserservice.API.dll"]
