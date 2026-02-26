# Build context: src/ (parent directory, needed for sibling project references)

# Build arg for MCR pull-through cache (default = direct pull from MCR)
ARG MCR_REGISTRY=mcr.microsoft.com/

# ==============================================================================
# Development target — hot-reload with dotnet watch
# ==============================================================================
FROM ${MCR_REGISTRY}dotnet/sdk:10.0 AS dev

ARG UID=1000
ARG GID=1000

WORKDIR /src
ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true

# Create non-root user matching host UID/GID for correct file ownership
# Use getent to skip creation if the GID/UID already exists in the base image
RUN (getent group ${GID} || groupadd -g ${GID} devuser) \
 && (getent passwd ${UID} || useradd -m -u ${UID} -g ${GID} -s /bin/bash devuser)
# Ensure HOME is writable by the non-root user (mkdir runs as root)
ENV HOME=/home/devuser
RUN mkdir -p ${HOME} && chown ${UID}:${GID} ${HOME}

# Source code is volume-mounted at runtime, no COPY needed for dev
EXPOSE 5152
USER ${UID}:${GID}
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
# Migration image — SDK with dotnet-ef for running EF Core migrations
# ==============================================================================
FROM ${MCR_REGISTRY}dotnet/sdk:10.0 AS migrate

WORKDIR /src
COPY --from=build /src .
RUN dotnet tool install --global dotnet-ef --version 10.*
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ef", "database", "update", \
  "--project", "FamilyHub.Api/FamilyHub.Api.csproj", \
  "--no-build"]

# ==============================================================================
# Final production image
# ==============================================================================
FROM ${MCR_REGISTRY}dotnet/aspnet:10.0 AS final

WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5152
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD ["sh", "-c", "wget -qO- http://localhost:5152/health || exit 1"]
ENTRYPOINT ["dotnet", "FamilyHub.Api.dll"]
