FROM mcr.microsoft.com/dotnet/sdk:6.0.201 as build
WORKDIR /build
# Copy csproj and restore as distinct layers
COPY src/WebApi/WebApi.csproj ./WebApi/
RUN dotnet restore WebApi/WebApi.csproj
# Copy everything else and build
COPY src/WebApi ./WebApi/
WORKDIR /build/WebApi
RUN dotnet build --configuration Release --no-restore

FROM build AS publish
RUN dotnet publish --configuration Release --no-build --output /app

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.3-alpine3.15
WORKDIR /app
COPY --from=publish /app ./
EXPOSE 80
ENTRYPOINT ["dotnet", "WebApi.dll"]
