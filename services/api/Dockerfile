FROM mcr.microsoft.com/dotnet/sdk:7.0.102-alpine3.17 as build

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
FROM mcr.microsoft.com/dotnet/aspnet:7.0.2-alpine3.17

WORKDIR /app

RUN addgroup --system appgroup && \
    adduser --system --no-create-home --disabled-password --gecos "App user" --ingroup appgroup appuser

COPY --from=build --chown=appuser:appgroup /app/ ./

USER appuser:appgroup
ENV ASPNETCORE_URLS="http://+:8080"
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApi.dll"]
