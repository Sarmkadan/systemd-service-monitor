#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Nagios/Icinga Plugin for systemd-service-monitor
# Checks service status via REST API
#
# Usage: check_systemd_service.sh -s SERVICE [-h HOST] [-p PORT] [-w WARN] [-c CRIT]
# Example: check_systemd_service.sh -s nginx.service -h monitor.example.com -p 5001

set -u

# Default values
HOST="localhost"
PORT="5001"
SERVICE=""
WARN_UPTIME=0
CRIT_UPTIME=0
TIMEOUT=10
PROTOCOL="https"

# Nagios exit codes
OK=0
WARNING=1
CRITICAL=2
UNKNOWN=3

# Usage information
usage() {
    echo "Nagios/Icinga plugin for systemd-service-monitor"
    echo ""
    echo "Usage: $0 -s SERVICE [-h HOST] [-p PORT] [-w WARN_UPTIME] [-c CRIT_UPTIME]"
    echo ""
    echo "Options:"
    echo "  -s SERVICE      Service name (required, e.g., nginx.service)"
    echo "  -h HOST         Monitor host (default: localhost)"
    echo "  -p PORT         Monitor port (default: 5001)"
    echo "  -w WARN         Warning uptime in minutes (default: 0, disabled)"
    echo "  -c CRIT         Critical uptime in minutes (default: 0, disabled)"
    echo "  -t TIMEOUT      HTTP timeout in seconds (default: 10)"
    echo "  --http          Use HTTP instead of HTTPS"
    echo "  --help          Show this help message"
    echo ""
    echo "Examples:"
    echo "  # Check if service is running"
    echo "  $0 -s nginx.service"
    echo ""
    echo "  # Check with warnings for short uptime"
    echo "  $0 -s nginx.service -w 5 -c 1"
    echo ""
    echo "  # Check on remote host"
    echo "  $0 -s mysql.service -h monitor.internal -p 5001"
    exit "$UNKNOWN"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -s|--service)
            SERVICE="$2"
            shift 2
            ;;
        -h|--host)
            HOST="$2"
            shift 2
            ;;
        -p|--port)
            PORT="$2"
            shift 2
            ;;
        -w|--warn)
            WARN_UPTIME="$2"
            shift 2
            ;;
        -c|--crit)
            CRIT_UPTIME="$2"
            shift 2
            ;;
        -t|--timeout)
            TIMEOUT="$2"
            shift 2
            ;;
        --http)
            PROTOCOL="http"
            shift
            ;;
        --help|-h)
            usage
            ;;
        *)
            echo "Unknown option: $1"
            usage
            ;;
    esac
done

# Validate required arguments
if [ -z "$SERVICE" ]; then
    echo "UNKNOWN: Service name required (-s option)"
    exit "$UNKNOWN"
fi

# Build API URL
API_URL="${PROTOCOL}://${HOST}:${PORT}/api/services/${SERVICE}"

# Make API request
RESPONSE=$(curl -s -k --max-time "$TIMEOUT" "$API_URL" 2>&1)
CURL_EXIT=$?

# Handle curl errors
if [ $CURL_EXIT -ne 0 ]; then
    echo "UNKNOWN: Cannot connect to monitor at ${HOST}:${PORT}"
    exit "$UNKNOWN"
fi

# Parse response
IS_ACTIVE=$(echo "$RESPONSE" | jq -r '.data.status.isActive // "unknown"' 2>/dev/null)
UPTIME_SECONDS=$(echo "$RESPONSE" | jq -r '.data.status.uptimeSeconds // 0' 2>/dev/null)
STATE=$(echo "$RESPONSE" | jq -r '.data.state // "unknown"' 2>/dev/null)
SUBSTATE=$(echo "$RESPONSE" | jq -r '.data.subState // "unknown"' 2>/dev/null)

# Validate response
if [ "$IS_ACTIVE" = "unknown" ]; then
    echo "UNKNOWN: Invalid response from monitor (service not found or API error)"
    exit "$UNKNOWN"
fi

# Convert uptime to minutes
UPTIME_MINUTES=$((UPTIME_SECONDS / 60))

# Check service status
if [ "$IS_ACTIVE" != "true" ]; then
    echo "CRITICAL: Service $SERVICE is not active (state: $STATE, substate: $SUBSTATE)"
    exit "$CRITICAL"
fi

# Check uptime thresholds
if [ "$CRIT_UPTIME" -gt 0 ] && [ "$UPTIME_MINUTES" -lt "$CRIT_UPTIME" ]; then
    echo "CRITICAL: Service uptime only $UPTIME_MINUTES minutes (threshold: $CRIT_UPTIME) | uptime=${UPTIME_MINUTES}min"
    exit "$CRITICAL"
fi

if [ "$WARN_UPTIME" -gt 0 ] && [ "$UPTIME_MINUTES" -lt "$WARN_UPTIME" ]; then
    echo "WARNING: Service uptime $UPTIME_MINUTES minutes (threshold: $WARN_UPTIME) | uptime=${UPTIME_MINUTES}min"
    exit "$WARNING"
fi

# Service is healthy
echo "OK: Service $SERVICE is running (uptime: $UPTIME_MINUTES minutes, state: $STATE) | uptime=${UPTIME_MINUTES}min"
exit "$OK"
