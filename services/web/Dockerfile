ARG ALPINE_VERSION=1.21.6

FROM node:16 as build

ARG BUILD_MODE=production
WORKDIR /out

# Install pnpm
RUN npm install -g pnpm

# Install dependencies
COPY package.json pnpm-lock.yaml .npmrc ./
RUN pnpm install --frozen-lockfile

# Copy all files
COPY . ./

# Build the project
RUN pnpm run build --mode ${BUILD_MODE}

# Build ngx_brotli from source
FROM nginx:${ALPINE_VERSION}-alpine as ngx_brotli

ARG ALPINE_VERSION
WORKDIR /compile

RUN apk update && apk upgrade && apk add git g++ pcre-dev zlib-dev make
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

EXPOSE 80
