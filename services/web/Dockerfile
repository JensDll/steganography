FROM node:16 as build
ARG mode=production
WORKDIR /out
# Install openssl
RUN apt update && apt upgrade -y && apt install openssl
# Install pnpm
RUN curl https://get.pnpm.io/v6.16.js | node - add --global pnpm
# Install dependencies
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile
# Copy all files
COPY . ./
# Build the project
RUN pnpm run build -- --mode ${mode}
# Generate self signed certificate
RUN openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -out /out/cert.pem -keyout /out/key.pem  \
    -subj "/CN=0.0.0.0"

# Final image
FROM nginx:1.21.6-alpine
WORKDIR /data/www
COPY --from=build /out/dist ./
COPY --from=build /out/cert.pem /out/key.pem /etc/nginx/
COPY nginx/default.conf /etc/nginx/conf.d/default.conf
EXPOSE 443
