# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore por capas (cachea si los .csproj no cambian)
COPY DivisaP2P.Library/*.csproj DivisaP2P.Library/
COPY DivisaP2P.WebApi/*.csproj DivisaP2P.WebApi/
RUN dotnet restore DivisaP2P.WebApi/DivisaP2P.WebApi.csproj

COPY . .
RUN dotnet publish DivisaP2P.WebApi/DivisaP2P.WebApi.csproj -c Release -o /app

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Railway inyecta PORT en runtime; bind dinámico (8080 por defecto local).
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet DivisaP2P.WebApi.dll"]
