# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["FamilyHub.sln", "./"]

# Copy project files
COPY ["FamilyHub.Api/FamilyHub.Api.csproj", "FamilyHub.Api/"]
COPY ["FamilyHub.SharedKernel/FamilyHub.SharedKernel.csproj", "FamilyHub.SharedKernel/"]
COPY ["FamilyHub.Infrastructure/FamilyHub.Infrastructure.csproj", "FamilyHub.Infrastructure/"]
COPY ["Modules/FamilyHub.Modules.Auth/FamilyHub.Modules.Auth.csproj", "Modules/FamilyHub.Modules.Auth/"]

# Restore dependencies
RUN dotnet restore "FamilyHub.Api/FamilyHub.Api.csproj"

# Copy all source files
COPY . .

# Build and publish
WORKDIR "/src/FamilyHub.Api"
RUN dotnet build "FamilyHub.Api.csproj" -c Release -o /app/build
RUN dotnet publish "FamilyHub.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup --system --gid 1000 familyhub \
    && adduser --system --uid 1000 --ingroup familyhub --shell /bin/sh familyhub

# Copy published app
COPY --from=build /app/publish .

# Set ownership
RUN chown -R familyhub:familyhub /app

# Switch to non-root user
USER familyhub

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

# Entry point
ENTRYPOINT ["dotnet", "FamilyHub.Api.dll"]
