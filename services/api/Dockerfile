FROM mcr.microsoft.com/dotnet/sdk:7.0.101-alpine3.16 as build

WORKDIR /build

# Copy csproj and restore as distinct layers
COPY src/Domain/Domain.csproj ./Domain/
COPY src/WebApi/WebApi.csproj ./WebApi/

RUN dotnet restore ./WebApi/WebApi.csproj

# Copy everything else and build
COPY src/Domain ./Domain/
COPY src/WebApi ./WebApi/

WORKDIR /build/WebApi

RUN dotnet publish --configuration Release --output /app --no-restore

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:7.0.1-alpine3.16

WORKDIR /app

RUN apk -U upgrade && addgroup --system app_group && \
    adduser --system --no-create-home --disabled-password --gecos "App user" --ingroup app_group app_user

COPY --from=build --chown=app_user:app_group /app/ ./

USER app_user:app_group

ENV ASPNETCORE_ENVIRONMENT="Deployment"
ENV ASPNETCORE_URLS="http://+:8080"

EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApi.dll"]
