FROM node:18.12 as build

ARG DOMAIN
ENV DOMAIN=${DOMAIN}

WORKDIR /out

# Install pnpm
RUN corepack enable && corepack prepare pnpm@7.26.3 --activate

# Install dependencies
COPY package.json pnpm-lock.yaml .npmrc ./
RUN pnpm install --frozen-lockfile

# Copy all files
COPY . ./

# Build the project
RUN pnpm run build

# Final image
FROM jensdll/bases:nginx-unprivileged-brotli-1.23.3-alpine

COPY --from=build /out/dist/ /data/www/

EXPOSE 8433
