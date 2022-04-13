FROM node:16 as build

ARG mode=production
WORKDIR /out

# Install openssl
RUN apt update && apt upgrade -y && apt install openssl

# Install pnpm
RUN npm install -g pnpm@6.32.6

# Install dependencies
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# Copy all files
COPY . ./

# Build the project
RUN pnpm run build -- --mode ${mode}

# Generate self signed certificate
RUN openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -out ./cert.pem -keyout ./key.pem  \
    -subj "/CN=0.0.0.0"

# Final image
FROM nginx:1.21.6-alpine

# Copy main content
COPY --from=build /out/dist/ /data/www/
# Copy self signed certificate, private key, and configuration files
COPY --from=build /out/cert.pem /out/key.pem /out/nginx/ /etc/nginx/

EXPOSE 443
