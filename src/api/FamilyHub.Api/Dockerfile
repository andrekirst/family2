# Multi-stage build: Frontend + Backend in single container
# Build from repository root: docker build -f src/api/FamilyHub.Api/Dockerfile .

# ============================================
# Stage 1: Build Angular Frontend
# ============================================
FROM node:22-alpine AS frontend-build

WORKDIR /app

# Copy frontend source
COPY src/frontend/family-hub-web/package*.json ./
RUN npm ci

COPY src/frontend/family-hub-web/ ./

# Build for production
RUN npm run build

# ============================================
# Stage 2: Build .NET Backend
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS backend-build

WORKDIR /app

# Copy csproj and restore dependencies
COPY src/api/FamilyHub.Api/FamilyHub.Api.csproj ./
RUN dotnet restore

# Copy source and build
COPY src/api/FamilyHub.Api/ ./
RUN dotnet publish -c Release -o /app/publish

# ============================================
# Stage 3: Final Runtime Image
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime

WORKDIR /app

# Copy backend from build stage
COPY --from=backend-build /app/publish .

# Copy frontend build to wwwroot (ASP.NET Core will serve it)
COPY --from=frontend-build /app/dist/family-hub-web/browser ./wwwroot

# Expose port
EXPOSE 8080

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "FamilyHub.Api.dll"]
