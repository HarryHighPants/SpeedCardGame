# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /app

COPY ./Engine/Engine.csproj ./Engine/Engine.csproj
COPY ./Server/Server.csproj ./Server/Server.csproj

# Copy csproj and restore as distinct layers
RUN dotnet restore ./Server/Server.csproj

# Copy everything else and build
COPY ./Engine ./Engine
COPY ./Server ./Server
RUN dotnet publish -c Release -o out ./Server/Server.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim-arm64v8
WORKDIR /app
COPY --from=build-env /app/out .
ENV ASPNETCORE_URLS=http://*:5169
ENTRYPOINT ["dotnet", "./Server.dll"]
