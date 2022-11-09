FROM mcr.microsoft.com/dotnet/sdk:7.0.100 as build

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
FROM mcr.microsoft.com/dotnet/aspnet:7.0.0-alpine3.16

WORKDIR /app

RUN apk -U upgrade && addgroup --system app_group && \
    adduser --system --no-create-home --disabled-password --gecos "App user" --ingroup app_group app_user

COPY --from=publish --chown=app_user:app_group /app/ ./

USER app_user:app_group

ENV ASPNETCORE_ENVIRONMENT="Deployment"

EXPOSE 80

ENTRYPOINT ["dotnet", "WebApi.dll"]
