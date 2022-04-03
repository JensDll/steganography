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

COPY --from=build /app/dist /usr/share/nginx/html
COPY --from=build /app/nginx/default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80