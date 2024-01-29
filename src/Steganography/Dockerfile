FROM node:20-slim as build_frontend

WORKDIR /build

RUN <<EOF
corepack enable
corepack prepare pnpm@latest --activate
EOF

COPY src/Frontend/package.json src/Frontend/pnpm-lock.yaml src/Frontend/.npmrc ./
RUN pnpm install --frozen-lockfile

COPY src/Frontend/ ./

RUN pnpm run generate

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0.101-jammy-amd64 as build_server

ARG TARGETARCH

RUN <<EOF
apt-get update
apt-get install -y clang zlib1g-dev
EOF

WORKDIR /build

COPY .editorconfig ./.editorconfig
COPY Directory.Build.props ./Directory.Build.props
COPY src/Steganography/src/Steganography.csproj ./Steganography/
COPY src/Domain/src/Domain.csproj ./Domain/

RUN dotnet restore --runtime linux-${TARGETARCH} ./Steganography/Steganography.csproj

COPY src/Steganography/src/ ./Steganography/
COPY --from=build_frontend /build/.output/public/ ./Steganography/wwwroot/
COPY src/Domain/src/ ./Domain/

RUN dotnet publish --configuration Release --no-restore --self-contained \
  --runtime linux-${TARGETARCH} --output /app ./Steganography/Steganography.csproj

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/nightly/runtime-deps:8.0.1-jammy-chiseled-aot-amd64
WORKDIR /app
COPY --from=build_server /app/Steganography ./app.exe
COPY --from=build_server /app/wwwroot/ ./wwwroot/
ENTRYPOINT ["./app.exe"]
