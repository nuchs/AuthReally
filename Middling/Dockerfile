#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ARG USERNAME
WORKDIR /app
RUN useradd --create-home --shell /bin/bash $USERNAME
COPY certs/ca-chain.crt /usr/local/share/ca-certificates/ca-chain.crt
RUN chmod 644 /usr/local/share/ca-certificates/ca-chain.crt && update-ca-certificates
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
ARG SERVICE_NAME
COPY ["$SERVICE_NAME.csproj", "."]
RUN dotnet restore "$SERVICE_NAME.csproj"
COPY . .
RUN dotnet build "$SERVICE_NAME.csproj" -c Release -o /app/build
RUN dotnet publish "$SERVICE_NAME.csproj" -c Release -o /app/publish

FROM base AS final
ARG USERNAME
WORKDIR /app
COPY --from=build /app/publish .
RUN chown -R $USERNAME:$USERNAME /app
USER $USERNAME
ARG SERVICE_NAME
# values passed in via ARG are not available to ENTRYPOINT, but ENV values are if you run the shell form of the command
ENV SERVICE_NAME="$SERVICE_NAME.dll"
ENTRYPOINT dotnet $SERVICE_NAME