# Build arg for Docker Hub pull-through cache (empty = direct pull)
ARG DOCKERHUB_REGISTRY=""

# ==============================================================================
# Development target — ng serve with live reload
# ==============================================================================
FROM ${DOCKERHUB_REGISTRY}node:22-alpine AS dev

WORKDIR /app

# Install dependencies first (layer cache)
COPY package.json package-lock.json ./
COPY .npmrc* ./
RUN npm ci

# Source code is volume-mounted at runtime
EXPOSE 4200
ENTRYPOINT ["npx", "ng", "serve", \
  "--host", "0.0.0.0", \
  "--port", "4200", \
  "--poll", "2000"]

# ==============================================================================
# Build stage
# ==============================================================================
FROM ${DOCKERHUB_REGISTRY}node:22-alpine AS build

WORKDIR /app
COPY package.json package-lock.json ./
COPY .npmrc* ./
RUN npm ci
COPY . .
RUN npx ng build --configuration production

# ==============================================================================
# Production image — nginx static serving
# ==============================================================================
FROM ${DOCKERHUB_REGISTRY}nginx:alpine AS prod

COPY --from=build /app/dist/family-hub-web/browser /usr/share/nginx/html
EXPOSE 80
