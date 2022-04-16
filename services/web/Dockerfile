FROM node:16 as build

ARG mode=production
WORKDIR /out

# Install openssl
RUN apt update && apt upgrade -y && apt install openssl

# Install pnpm
RUN npm install -g pnpm@6.32.8

# Install dependencies
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# Copy all files
COPY . ./

# Build the project
RUN pnpm run build -- --mode ${mode}

# Final image
FROM nginx:1.21.6-alpine

# Copy main content
COPY --from=build /out/dist/ /data/www/
# Copy configuration files
COPY nginx/ /etc/nginx/

EXPOSE 80
