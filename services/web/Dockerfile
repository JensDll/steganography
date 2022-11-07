FROM node:16 as build

WORKDIR /out

# Install pnpm
RUN corepack enable && corepack prepare pnpm@latest --activate

# Install dependencies
COPY package.json pnpm-lock.yaml .npmrc ./
RUN pnpm install --frozen-lockfile

# Copy all files
COPY . ./

# Build the project
RUN pnpm run build

# Final image
FROM jensdll/image-data-hiding:nginx-base.1.23.2-alpine

ARG ALPINE_VERSION

RUN apk -U upgrade && addgroup --system app_user_group && \
    adduser --system --no-create-home --disabled-password --gecos "App user" --ingroup app_user_group app_user && \
    # Create necessary folders and files to avoid permission errors
    mkdir /var/cache/nginx/client_temp /var/cache/nginx/proxy_temp /var/cache/nginx/fastcgi_temp \
          /var/cache/nginx/uwsgi_temp /var/cache/nginx/scgi_temp && \
    touch /var/run/nginx.pid && \
    chown app_user /var/run/nginx.pid

# Copy build output
COPY --from=build --chown=app_user:app_user_group /out/dist/ /data/www/

USER app_user:app_user_group

EXPOSE 80
