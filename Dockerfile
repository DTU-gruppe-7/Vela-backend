# -- Stadie 1: Byg applikationen --
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopier først kun .csproj filerne og kør restore. 
# Dette cacher dependencies og gør dine fremtidige builds i GitHub Actions MEGET hurtigere.
COPY ["src/Vela.API/Vela.API.csproj", "src/Vela.API/"]
COPY ["src/Vela.Application/Vela.Application.csproj", "src/Vela.Application/"]
COPY ["src/Vela.Domain/Vela.Domain.csproj", "src/Vela.Domain/"]
COPY ["src/Vela.Infrastructure/Vela.Infrastructure.csproj", "src/Vela.Infrastructure/"]
RUN dotnet restore "src/Vela.API/Vela.API.csproj"

# Kopier resten af kildekoden ind
COPY . .

# Byg og publicer
WORKDIR "/src/src/Vela.API"
RUN dotnet build "Vela.API.csproj" -c Release -o /app/build
RUN dotnet publish "Vela.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -- Stadie 2: Kør applikationen (Lightweight runtime) --
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Kopier appsettings (vigtigt hvis du har konfigurationer der skal med)
COPY --from=build /src/src/Vela.API/appsettings*.json ./
COPY --from=build /app/publish .

# .NET 10 bruger port 8080 som standard for non-root containere
EXPOSE 8080

ENTRYPOINT ["dotnet", "Vela.API.dll"]