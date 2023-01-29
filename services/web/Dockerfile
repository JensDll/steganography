FROM node:19.5 as build

WORKDIR /out

# Install pnpm
RUN corepack enable && corepack prepare pnpm@7.25.1 --activate

# Install dependencies
COPY package.json pnpm-lock.yaml .npmrc ./
RUN pnpm install --frozen-lockfile

# Copy all files
COPY . ./

# Build the project
RUN pnpm run build

# Final image
FROM jensdll/bases:nginx-unprivileged-brotli-1.23.3-alpine

COPY --from=build --chown=nginx:nginx /out/dist/ /data/www/

EXPOSE 8080
