FROM mcr.microsoft.com/dotnet/sdk:6.0.201 as build

WORKDIR /build

# Copy csproj and restore as distinct layers
COPY src/ApiBuilder/ApiBuilder.csproj ./ApiBuilder/
COPY src/Domain/Domain.csproj ./Domain/
COPY src/WebApi/WebApi.csproj ./WebApi/

RUN dotnet restore WebApi/WebApi.csproj

# Copy everything else and build
COPY src/ApiBuilder ./ApiBuilder/
COPY src/Domain ./Domain/
COPY src/WebApi ./WebApi/

WORKDIR /build/WebApi

RUN dotnet build --configuration Release --no-restore

FROM build AS publish

# Publish in release configuration
RUN dotnet publish --configuration Release --no-build --output /app

# Install openssl
RUN apt update && apt upgrade -y && apt install openssl

# Generate self signed certificate
RUN openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -out /app/cert.pem -keyout /app/key.pem  \
  -subj "/CN=0.0.0.0"

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.3-alpine3.15

WORKDIR /app

COPY --from=publish /app/ ./

ENV ASPNETCORE_ENVIRONMENT="Deployment"
ENV ASPNETCORE_URLS="http://+:80;https://+:443"

EXPOSE 80 443

ENTRYPOINT ["dotnet", "WebApi.dll"]
