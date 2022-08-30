#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Monitoring script for systemd-service-monitor
# Polls the API and tracks service health changes
# Logs alerts and maintains history file

set -euo pipefail

API_BASE="https://localhost:5001/api"
LOG_FILE="/var/log/systemd-monitor-alerts.log"
STATE_FILE="/tmp/systemd-monitor-states.json"
CHECK_INTERVAL=5
ALERT_EMAIL=""
CRITICAL_SERVICES=("nginx.service" "mysql.service" "redis.service")

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Initialize state file if not exists
init_state_file() {
    if [ ! -f "$STATE_FILE" ]; then
        echo "{}" > "$STATE_FILE"
    fi
}

# Log alert message
log_alert() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    local message="$1"
    echo "[$timestamp] $message" >> "$LOG_FILE"
    echo -e "${RED}[ALERT]${NC} $message"
}

# Log info message
log_info() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    local message="$1"
    echo "[$timestamp] $message" >> "$LOG_FILE"
    echo -e "${GREEN}[INFO]${NC} $message"
}

# Get service status from API
get_service_status() {
    local service=$1
    local response=$(curl -s -k "$API_BASE/services/$service" 2>/dev/null || echo "{}")
    echo "$response"
}

# Check if service is active
is_service_active() {
    local response=$1
    echo "$response" | jq -r '.data.status.isActive // false'
}

# Send alert notification (email, webhook, etc)
send_alert() {
    local service=$1
    local status=$2
    local message="Service $service is now $status"

    # Log the alert
    log_alert "$message"

    # Optional: Send email alert
    if [ -n "$ALERT_EMAIL" ]; then
        echo "$message" | mail -s "Service Alert: $service" "$ALERT_EMAIL"
    fi

    # Optional: Send webhook
    # curl -X POST https://alerts.example.com/webhook -d "service=$service&status=$status"
}

# Monitor single service
monitor_service() {
    local service=$1
    local prev_state=$(jq -r ".\"$service\" // \"unknown\"" "$STATE_FILE")

    local response=$(get_service_status "$service")
    local current_state=$(is_service_active "$response")

    if [ "$current_state" = "true" ]; then
        current_state="active"
    else
        current_state="inactive"
    fi

    # State changed
    if [ "$prev_state" != "$current_state" ]; then
        send_alert "$service" "$current_state"
        jq ".\"$service\" = \"$current_state\"" "$STATE_FILE" > "$STATE_FILE.tmp"
        mv "$STATE_FILE.tmp" "$STATE_FILE"
    fi

    # Display current status
    local symbol="✓"
    local color=$GREEN
    if [ "$current_state" != "active" ]; then
        symbol="✗"
        color=$RED
    fi

    printf "${color}%s${NC} %-40s %s\n" "$symbol" "$service" "$current_state"
}

# Monitor all critical services
monitor_all() {
    clear
    echo "=== systemd Service Monitor - $(date '+%Y-%m-%d %H:%M:%S') ==="
    echo ""

    for service in "${CRITICAL_SERVICES[@]}"; do
        monitor_service "$service"
    done

    echo ""
    echo "Press Ctrl+C to stop. Checking every ${CHECK_INTERVAL}s."
}

# Get system metrics
show_metrics() {
    echo ""
    echo "=== System Metrics ==="

    local response=$(curl -s -k "$API_BASE/system/resources" 2>/dev/null || echo "{}")

    if echo "$response" | jq -e '.data' > /dev/null 2>&1; then
        local cpu=$(echo "$response" | jq -r '.data.cpuPercent // "N/A"')
        local mem_used=$(echo "$response" | jq -r '.data.memoryUsedMb // "N/A"')
        local mem_total=$(echo "$response" | jq -r '.data.memoryTotalMb // "N/A"')
        local disk_used=$(echo "$response" | jq -r '.data.diskUsedGb // "N/A"')
        local disk_total=$(echo "$response" | jq -r '.data.diskTotalGb // "N/A"')

        echo "CPU: ${cpu}%"
        echo "Memory: ${mem_used}/${mem_total} MB"
        echo "Disk: ${disk_used}/${disk_total} GB"
    fi
}

# Check health endpoint
check_health() {
    local response=$(curl -s -k "https://localhost:5001/health" 2>/dev/null || echo "{}")
    local status=$(echo "$response" | jq -r '.status // "Unknown"')
    echo "API Health: $status"
}

# Generate report
generate_report() {
    echo ""
    echo "=== Alert History (Last 24 hours) ==="
    if [ -f "$LOG_FILE" ]; then
        grep "$(date -d 'yesterday' '+%Y-%m-%d')" "$LOG_FILE" | tail -20
    else
        echo "No alerts recorded"
    fi
}

# Main loop
main() {
    init_state_file

    echo "Starting systemd Service Monitor..."
    echo "Monitoring services: ${CRITICAL_SERVICES[@]}"
    echo ""

    while true; do
        monitor_all
        show_metrics
        check_health
        sleep "$CHECK_INTERVAL"
    done
}

# Handle signals
trap 'echo ""; log_info "Monitor stopped"; exit 0' SIGINT SIGTERM

# Parse arguments
case "${1:-}" in
    report)
        generate_report
        ;;
    *)
        main
        ;;
esac
