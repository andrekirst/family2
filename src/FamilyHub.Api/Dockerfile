# Build context: src/ (parent directory, needed for sibling project references)

# Build arg for MCR pull-through cache (default = direct pull from MCR)
ARG MCR_REGISTRY=mcr.microsoft.com/

# ==============================================================================
# Development target — hot-reload with dotnet watch
# ==============================================================================
FROM ${MCR_REGISTRY}dotnet/sdk:10.0 AS dev

WORKDIR /src
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true

# When compose sets user: to the host UID, HOME still points to /root.
# Redirect to /tmp so dotnet can write NuGet/SDK caches.
ENV HOME=/tmp

# Source code is volume-mounted at runtime, no COPY needed for dev
EXPOSE 5152
ENTRYPOINT ["dotnet", "watch", "run", \
  "--project", "FamilyHub.Api/FamilyHub.Api.csproj", \
  "--urls", "http://0.0.0.0:5152"]

# ==============================================================================
# Build stage — restore and publish
# ==============================================================================
FROM ${MCR_REGISTRY}dotnet/sdk:10.0 AS build

ARG BAGET_URL=""
WORKDIR /src
COPY . .
RUN if [ -n "$BAGET_URL" ]; then \
      dotnet nuget add source "$BAGET_URL" -n baget-cache --allow-insecure-connections; \
    fi
RUN dotnet restore FamilyHub.Api/FamilyHub.Api.csproj
RUN dotnet publish FamilyHub.Api/FamilyHub.Api.csproj \
  -c Release -o /app/publish --no-restore

# ==============================================================================
# Final production image
# ==============================================================================
FROM ${MCR_REGISTRY}dotnet/aspnet:10.0 AS final

WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5152
ENTRYPOINT ["dotnet", "FamilyHub.Api.dll"]
