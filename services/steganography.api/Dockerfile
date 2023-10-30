FROM node:21.1 as build_web

WORKDIR /out

# Install pnpm
RUN corepack enable && corepack prepare pnpm@8.5.1 --activate

# Install dependencies
COPY steganography.web/package.json steganography.web/pnpm-lock.yaml steganography.web/.npmrc ./
RUN pnpm install --frozen-lockfile

# Copy all files
COPY steganography.web ./

# Build the project
RUN pnpm run build

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0.400-alpine3.17 as build

ARG TARGETARCH

WORKDIR /build

# Copy csproj and restore as distinct layers
COPY aspnet.common/src/aspnet.common/aspnet.common.csproj ./aspnet.common/
COPY steganography.api/src/steganography.domain/steganography.domain.csproj ./steganography.domain/
COPY steganography.api/src/steganography.api/steganography.api.csproj ./steganography.api/

RUN dotnet restore --arch $TARGETARCH ./steganography.api/steganography.api.csproj

# Copy everything else
COPY aspnet.common/src/aspnet.common/ ./aspnet.common/
COPY steganography.api/src/steganography.domain/ ./steganography.domain/
COPY steganography.api/src/steganography.api/ ./steganography.api/
COPY --from=build_web /out/dist/ ./steganography.api/wwwroot/

# Build the project
RUN dotnet publish --configuration Release --arch $TARGETARCH \
  --no-self-contained --output /app --no-restore \
  ./steganography.api/steganography.api.csproj

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:7.0.10-alpine3.17

WORKDIR /app

RUN addgroup --system appgroup && \
  adduser --system --no-create-home --disabled-password --gecos "App user" --ingroup appgroup appuser

COPY --from=build --chown=appuser:appgroup /app/ ./

USER appuser:appgroup

ENV ASPNETCORE_URLS=""

EXPOSE 8080 8443

ENTRYPOINT ["dotnet", "steganography.api.dll"]
