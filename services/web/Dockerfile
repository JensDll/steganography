FROM node:16 as build

ARG mode=production
WORKDIR /app

# Install pnpm
RUN curl https://get.pnpm.io/v6.16.js | node - add --global pnpm

# Install dependencies
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# Copy all files
COPY . ./

# Build the project
RUN pnpm run build -- --mode ${mode}

# Final image
FROM nginx:1.21.6-alpine

WORKDIR /data/www

COPY --from=build /app/dist ./
COPY nginx/default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80