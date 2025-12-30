# Stage 1: Build Angular app
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files for dependency installation
COPY package*.json ./

# Install dependencies (npm ci for clean install)
RUN npm ci

# Copy all source files
COPY . .

# Build Angular app for production
RUN npm run build:prod

# Stage 2: Serve with Nginx
FROM nginx:alpine

# Copy built Angular app from build stage
COPY --from=build /app/dist/family-hub-web/browser /usr/share/nginx/html

# Copy custom Nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]
