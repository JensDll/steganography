FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-jammy as build

ARG TARGETARCH

RUN apt-get update && apt-get install clang zlib1g-dev -y

WORKDIR /build

COPY .editorconfig ./.editorconfig
COPY Directory.Build.props ./Directory.Build.props
COPY src/Steganography/src/Steganography.csproj ./Steganography/
COPY src/AspNetShared/src/AspNetShared.csproj ./AspNetShared/
COPY src/Domain/src/Domain.csproj ./Domain/

RUN dotnet restore --runtime linux-${TARGETARCH} ./Steganography/Steganography.csproj

COPY src/Steganography/src/ ./Steganography/
COPY src/AspNetShared/src/ ./AspNetShared/
COPY src/Domain/src/ ./Domain/

RUN dotnet publish --configuration Release --no-restore --self-contained \
  --runtime linux-${TARGETARCH} --output /app ./Steganography/Steganography.csproj

RUN ls -lah /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled
WORKDIR /app
COPY --from=build /app/Steganography ./app.exe
USER app
ENTRYPOINT ["./app.exe"]
