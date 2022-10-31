ARG ALPINE_VERSION=1.23.2

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

# Build ngx_brotli from source
FROM nginx:${ALPINE_VERSION}-alpine as ngx_brotli

ARG ALPINE_VERSION
WORKDIR /compile

RUN apk -U upgrade && apk add git g++ pcre-dev zlib-dev make
RUN wget https://nginx.org/download/nginx-${ALPINE_VERSION}.tar.gz && tar xzf nginx-${ALPINE_VERSION}.tar.gz
RUN git clone https://github.com/google/ngx_brotli --recursive

WORKDIR /compile/nginx-${ALPINE_VERSION}

RUN ./configure --with-compat --add-dynamic-module=../ngx_brotli
RUN make modules

# Final image
FROM nginx:${ALPINE_VERSION}-alpine

ARG ALPINE_VERSION

# Copy build output
COPY --from=build /out/dist/ /data/www/
# Copy ngx_brotli modules
COPY --from=ngx_brotli /compile/nginx-${ALPINE_VERSION}/objs/ngx_http_brotli*.so /usr/lib/nginx/modules/

RUN apk -U upgrade && addgroup --system app_user_group && \
    adduser --system --no-create-home --disabled-password --gecos "App user" --ingroup app_user_group app_user && \
    # Create necessary folders and files to avoid permission errors
    mkdir /var/cache/nginx/client_temp /var/cache/nginx/proxy_temp /var/cache/nginx/fastcgi_temp \
          /var/cache/nginx/uwsgi_temp /var/cache/nginx/scgi_temp && \
    touch /var/run/nginx.pid && \
    chown app_user /var/run/nginx.pid

USER app_user:app_user_group

EXPOSE 80
