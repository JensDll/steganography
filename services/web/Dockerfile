FROM node:19.2 as build

WORKDIR /out

# Install pnpm
RUN corepack enable && corepack prepare pnpm@7.16.0 --activate

# Install dependencies
COPY package.json pnpm-lock.yaml .npmrc ./
RUN pnpm install --frozen-lockfile

# Copy all files
COPY . ./

# Build the project
RUN pnpm run build

# Final image
FROM jensdll/image-data-hiding:nginx-base.1.23.2-alpine

COPY --from=build --chown=nginx:nginx /out/dist/ /data/www/

EXPOSE 80
