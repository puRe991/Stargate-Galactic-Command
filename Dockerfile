# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln .
COPY StargateGalacticCommand.Core/*.csproj StargateGalacticCommand.Core/
COPY StargateGalacticCommand.Data/*.csproj StargateGalacticCommand.Data/
COPY StargateGalacticCommand.Web/*.csproj StargateGalacticCommand.Web/
COPY StargateGalacticCommand.Tests/*.csproj StargateGalacticCommand.Tests/
RUN dotnet restore StargateGalacticCommand.sln

COPY . .
RUN dotnet publish StargateGalacticCommand.Web -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# The SQLite file lives here so it survives container restarts when this path is
# mounted as a volume (see README "SQLite-Betriebsrisiko" for the single-writer caveat).
RUN mkdir -p /data
ENV ConnectionStrings__DefaultConnection="Data Source=/data/stargate-galactic-command.db;Default Timeout=30"
ENV ASPNETCORE_URLS=http://+:8080
VOLUME ["/data"]
EXPOSE 8080

# Admin:Password must be overridden via the Admin__Password env var — the app refuses
# to start with the default value outside Development (see Startup.CheckAdminPassword).
ENTRYPOINT ["dotnet", "StargateGalacticCommand.Web.dll"]
