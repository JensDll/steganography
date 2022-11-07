ARG ALPINE_VERSION

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

# Copy ngx_brotli modules
COPY --from=ngx_brotli /compile/nginx-${ALPINE_VERSION}/objs/ngx_http_brotli*.so /usr/lib/nginx/modules/
