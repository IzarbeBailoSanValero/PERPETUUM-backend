Docke# Etapa 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS buildapp
WORKDIR /src
COPY . .
RUN dotnet publish "PERPETUUM.csproj" -c Release -o /consoleapp

# Etapa 2: Ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=buildapp /consoleapp ./
ENTRYPOINT ["dotnet", "PERPETUUM.dll"]
EXPOSE 8080
