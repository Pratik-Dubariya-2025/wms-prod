# syntax=docker/dockerfile:1

# --- Build stage ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first (better layer caching): copy only project files.
COPY WMS.API/WMS.API.csproj WMS.API/
COPY WMS.Application/WMS.Application.csproj WMS.Application/
COPY WMS.Infrastructure/WMS.Infrastructure.csproj WMS.Infrastructure/
COPY WMS.Domain/WMS.Domain.csproj WMS.Domain/
RUN dotnet restore WMS.API/WMS.API.csproj

# Copy the rest of the source and publish.
COPY . .
RUN dotnet publish WMS.API/WMS.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage -------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# The Data Protection key ring (encrypts MFA secrets at rest) is written to
# /keys, which docker-compose mounts as a named volume so it survives
# restarts. Docker volumes are created root-owned by default; a fresh named
# volume's *first* mount copies the ownership/contents of whatever already
# exists at that path in the image, so this has to be created and chowned
# here — as root, before the USER switch below — or the non-root process
# gets a permission-denied error trying to write its own key ring.
RUN mkdir -p /keys && chown $APP_UID:$APP_UID /keys

# Run as the non-root user baked into the base image instead of root.
USER $APP_UID

ENTRYPOINT ["dotnet", "WMS.API.dll"]
