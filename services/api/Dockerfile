FROM mcr.microsoft.com/dotnet/sdk:6.0.300 as build

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

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.5-alpine3.15

WORKDIR /app

COPY --from=publish /app/ ./

RUN addgroup --system api && \
    adduser --system --no-create-home --disabled-password --gecos "Api service user" --ingroup api api

USER api:api

ENV ASPNETCORE_ENVIRONMENT="Deployment"

EXPOSE 80

ENTRYPOINT ["dotnet", "WebApi.dll"]
