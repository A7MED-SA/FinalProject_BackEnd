FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy project files for layer caching
COPY Directory.Build.props Directory.Packages.props ./
COPY src/Athary.Domain/Athary.Domain.csproj src/Athary.Domain/
COPY src/Athary.Application/Athary.Application.csproj src/Athary.Application/
COPY src/Athary.Infrastructure/Athary.Infrastructure.csproj src/Athary.Infrastructure/
COPY src/Athary.API/Athary.API.csproj src/Athary.API/

# Restore
RUN dotnet restore src/Athary.API/Athary.API.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/src/Athary.API
RUN dotnet publish -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime

# Create non-root user
RUN adduser -u 1000 -D appuser

WORKDIR /app
COPY --from=build /app .
RUN chown -R appuser /app

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "Athary.API.dll"]
