#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
COPY certs/ca-chain.crt /usr/local/share/ca-certificates/ca-chain.crt
RUN chmod 644 /usr/local/share/ca-certificates/ca-chain.crt && update-ca-certificates
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AuthReally.csproj", "."]
RUN dotnet restore "AuthReally.csproj"
COPY . .
RUN dotnet build "AuthReally.csproj" -c Release -o /app/build
RUN dotnet publish "AuthReally.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuthReally.dll"]