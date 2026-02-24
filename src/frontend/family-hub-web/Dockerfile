# Build arg for Docker Hub pull-through cache (empty = direct pull)
ARG DOCKERHUB_REGISTRY=""

# ==============================================================================
# Development target — ng serve with live reload
# ==============================================================================
FROM ${DOCKERHUB_REGISTRY}node:22-alpine AS dev

ARG UID=1000
ARG GID=1000

WORKDIR /app

# Install dependencies first (layer cache — needs root)
COPY package.json package-lock.json ./
COPY .npmrc* ./
RUN npm ci

# Make node_modules writable for non-root user (compose user: directive).
# The named volume is initialised from this layer on first run.
RUN chmod -R 777 /app/node_modules

# Create non-root user matching host UID/GID for correct file ownership
# Skip creation if the GID/UID already exists in the base image (e.g. node:1000)
RUN (getent group ${GID} || addgroup -g ${GID} devuser) \
 && (getent passwd ${UID} || adduser -D -u ${UID} -G $(getent group ${GID} | cut -d: -f1) -h /home/devuser devuser)
ENV HOME=/home/devuser
RUN mkdir -p ${HOME} && chown ${UID}:${GID} ${HOME}

# Source code is volume-mounted at runtime
EXPOSE 4200
USER ${UID}:${GID}
ENTRYPOINT ["npx", "ng", "serve", \
  "--host", "0.0.0.0", \
  "--port", "4200"]

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
