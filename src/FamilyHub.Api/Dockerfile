# Build context: src/ (parent directory, needed for sibling project references)

# ==============================================================================
# Development target — hot-reload with dotnet watch
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dev

WORKDIR /src
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true

# Source code is volume-mounted at runtime, no COPY needed for dev
EXPOSE 5152
ENTRYPOINT ["dotnet", "watch", "run", \
  "--project", "FamilyHub.Api/FamilyHub.Api.csproj", \
  "--urls", "http://0.0.0.0:5152"]

# ==============================================================================
# Build stage — restore and publish
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY . .
RUN dotnet restore FamilyHub.Api/FamilyHub.Api.csproj
RUN dotnet publish FamilyHub.Api/FamilyHub.Api.csproj \
  -c Release -o /app/publish --no-restore

# ==============================================================================
# Final production image
# ==============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5152
ENTRYPOINT ["dotnet", "FamilyHub.Api.dll"]
