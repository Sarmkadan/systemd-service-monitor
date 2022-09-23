# Build stage - compile the application
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS builder
WORKDIR /src

# Copy project files and restore dependencies (layer caching)
COPY ["systemd-service-monitor.csproj", "."]
RUN dotnet restore "systemd-service-monitor.csproj"

# Copy source code and build
COPY . .
RUN dotnet build "systemd-service-monitor.csproj" -c Release -o /app/build

# Publish stage
FROM builder AS publish
RUN dotnet publish "systemd-service-monitor.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Install required system packages for D-Bus and systemd monitoring
RUN apk add --no-cache \
    dbus \
    systemd \
    curl \
    jq \
    iputils \
    && rm -rf /var/cache/apk/*

# Create non-root user for security
RUN addgroup -g 1001 appgroup && \
    adduser -D -u 1001 -G appgroup appuser && \
    mkdir -p /app/logs && \
    chown -R appuser:appgroup /app/logs

# Copy published application
COPY --from=publish /app/publish .

# Change to non-root user
USER appuser

# Expose port
EXPOSE 5001

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f -k https://localhost:5001/health || exit 1

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=https://0.0.0.0:5001

# Run the application
ENTRYPOINT ["dotnet", "systemd-service-monitor.dll"]