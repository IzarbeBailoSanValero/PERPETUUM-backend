# Etapa 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS buildapp
WORKDIR /src
COPY . .
RUN dotnet publish "PERPETUUM.csproj" -c Release -o /consoleapp

# Etapa 2: Ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=buildapp /consoleapp ./
COPY perpetuum.sql ./perpetuum.sql 
ENTRYPOINT ["dotnet", "PERPETUUM.dll"]
EXPOSE 8080
