# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Build stage - compile the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

# Copy project files
COPY ["systemd-service-monitor.csproj", "."]
RUN dotnet restore "systemd-service-monitor.csproj"

# Copy source code
COPY . .

# Build release
RUN dotnet build "systemd-service-monitor.csproj" -c Release -o /app/build

# Publish stage
FROM builder AS publish
RUN dotnet publish "systemd-service-monitor.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

# Install required system packages
RUN apt-get update && apt-get install -y \
    libdbus-1-dev \
    dbus \
    systemd \
    curl \
    jq \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create log directory
RUN mkdir -p /app/logs && chmod 755 /app/logs

# Expose port
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f -k https://localhost:5001/health || exit 1

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=https://0.0.0.0:5001

# Run the application
ENTRYPOINT ["dotnet", "systemd-service-monitor.dll"]
