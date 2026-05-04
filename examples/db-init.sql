-- =============================================================================
-- Author: Vladyslav Zaiets | https://sarmkadan.com
-- CTO & Software Architect
-- =============================================================================

-- Database initialization script for systemd-service-monitor
-- Creates tables for persistent storage of services, logs, and metrics

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Services table
CREATE TABLE IF NOT EXISTS services (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    display_name VARCHAR(255),
    description TEXT,
    unit_file_state VARCHAR(50),
    load_state VARCHAR(50),
    active_state VARCHAR(50),
    sub_state VARCHAR(50),
    is_enabled BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP
);

CREATE INDEX idx_services_name ON services(name);
CREATE INDEX idx_services_active_state ON services(active_state);
CREATE INDEX idx_services_deleted_at ON services(deleted_at);

-- Service status history
CREATE TABLE IF NOT EXISTS service_status_history (
    id SERIAL PRIMARY KEY,
    service_id INTEGER NOT NULL REFERENCES services(id) ON DELETE CASCADE,
    previous_state VARCHAR(50),
    current_state VARCHAR(50),
    uptime_seconds INTEGER,
    pid INTEGER,
    memory_mb DECIMAL(10, 2),
    cpu_percent DECIMAL(5, 2),
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_service_status_service_id ON service_status_history(service_id);
CREATE INDEX idx_service_status_timestamp ON service_status_history(timestamp);

-- Logs table
CREATE TABLE IF NOT EXISTS service_logs (
    id BIGSERIAL PRIMARY KEY,
    service_id INTEGER REFERENCES services(id) ON DELETE CASCADE,
    timestamp TIMESTAMP NOT NULL,
    priority VARCHAR(20),
    process_id INTEGER,
    message TEXT,
    systemd_unit VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_logs_service_id ON service_logs(service_id);
CREATE INDEX idx_logs_timestamp ON service_logs(timestamp);
CREATE INDEX idx_logs_priority ON service_logs(priority);
CREATE INDEX idx_logs_message ON service_logs USING GIN(to_tsvector('english', message));

-- Metrics table
CREATE TABLE IF NOT EXISTS service_metrics (
    id BIGSERIAL PRIMARY KEY,
    service_id INTEGER NOT NULL REFERENCES services(id) ON DELETE CASCADE,
    timestamp TIMESTAMP NOT NULL,
    cpu_percent DECIMAL(5, 2),
    memory_mb DECIMAL(10, 2),
    disk_read_bytes_per_sec BIGINT,
    disk_write_bytes_per_sec BIGINT,
    network_in_bytes_per_sec BIGINT,
    network_out_bytes_per_sec BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_metrics_service_id ON service_metrics(service_id);
CREATE INDEX idx_metrics_timestamp ON service_metrics(timestamp);
CREATE INDEX idx_metrics_service_timestamp ON service_metrics(service_id, timestamp DESC);

-- Metrics aggregated (5-minute buckets)
CREATE TABLE IF NOT EXISTS service_metrics_5m (
    id BIGSERIAL PRIMARY KEY,
    service_id INTEGER NOT NULL REFERENCES services(id) ON DELETE CASCADE,
    timestamp TIMESTAMP NOT NULL,
    cpu_percent_avg DECIMAL(5, 2),
    cpu_percent_max DECIMAL(5, 2),
    memory_mb_avg DECIMAL(10, 2),
    memory_mb_max DECIMAL(10, 2),
    disk_read_avg BIGINT,
    disk_write_avg BIGINT,
    sample_count INTEGER,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_metrics_5m_service_id ON service_metrics_5m(service_id);
CREATE INDEX idx_metrics_5m_timestamp ON service_metrics_5m(timestamp);

-- Health checks table
CREATE TABLE IF NOT EXISTS health_checks (
    id SERIAL PRIMARY KEY,
    service_id INTEGER REFERENCES services(id) ON DELETE CASCADE,
    check_type VARCHAR(50),
    status VARCHAR(20),
    response_time_ms INTEGER,
    error_message TEXT,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_health_checks_service_id ON health_checks(service_id);
CREATE INDEX idx_health_checks_timestamp ON health_checks(timestamp);

-- Events/Alerts table
CREATE TABLE IF NOT EXISTS service_events (
    id SERIAL PRIMARY KEY,
    service_id INTEGER REFERENCES services(id) ON DELETE CASCADE,
    event_type VARCHAR(50),
    severity VARCHAR(20),
    message TEXT,
    metadata JSONB,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    acknowledged BOOLEAN DEFAULT FALSE,
    acknowledged_by VARCHAR(255),
    acknowledged_at TIMESTAMP
);

CREATE INDEX idx_events_service_id ON service_events(service_id);
CREATE INDEX idx_events_timestamp ON service_events(timestamp);
CREATE INDEX idx_events_severity ON service_events(severity);
CREATE INDEX idx_events_acknowledged ON service_events(acknowledged);

-- Audit log
CREATE TABLE IF NOT EXISTS audit_logs (
    id SERIAL PRIMARY KEY,
    action VARCHAR(50),
    service_id INTEGER REFERENCES services(id) ON DELETE SET NULL,
    user_id VARCHAR(255),
    ip_address INET,
    details JSONB,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_audit_logs_timestamp ON audit_logs(timestamp);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);
CREATE INDEX idx_audit_logs_service_id ON audit_logs(service_id);

-- Settings/Configuration table
CREATE TABLE IF NOT EXISTS settings (
    id SERIAL PRIMARY KEY,
    key VARCHAR(255) NOT NULL UNIQUE,
    value TEXT,
    data_type VARCHAR(50),
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Retention policy function
CREATE OR REPLACE FUNCTION cleanup_old_logs()
RETURNS void AS $$
BEGIN
    -- Delete logs older than retention period
    DELETE FROM service_logs
    WHERE timestamp < NOW() - INTERVAL '90 days';

    -- Delete metrics older than retention period
    DELETE FROM service_metrics
    WHERE timestamp < NOW() - INTERVAL '30 days';

    -- Delete aggregated metrics older than retention period
    DELETE FROM service_metrics_5m
    WHERE timestamp < NOW() - INTERVAL '180 days';

    -- Delete old health checks
    DELETE FROM health_checks
    WHERE timestamp < NOW() - INTERVAL '30 days';
END;
$$ LANGUAGE plpgsql;

-- Create scheduled job to cleanup old data (if using pg_cron)
-- SELECT cron.schedule('cleanup-logs', '0 2 * * *', 'SELECT cleanup_old_logs()');

-- Update service status history trigger
CREATE OR REPLACE FUNCTION update_service_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE services SET updated_at = CURRENT_TIMESTAMP WHERE id = NEW.service_id;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_service_status_update
AFTER INSERT ON service_status_history
FOR EACH ROW
EXECUTE FUNCTION update_service_timestamp();

-- Insert default settings
INSERT INTO settings (key, value, data_type) VALUES
    ('log_retention_days', '90', 'integer'),
    ('metric_collection_interval_ms', '5000', 'integer'),
    ('api_rate_limit_per_minute', '100', 'integer'),
    ('enable_audit_logging', 'true', 'boolean'),
    ('enable_health_checks', 'true', 'boolean')
ON CONFLICT (key) DO NOTHING;

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO systemd_monitor;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO systemd_monitor;

-- Create views for reporting
CREATE OR REPLACE VIEW service_status_current AS
SELECT
    s.id,
    s.name,
    s.display_name,
    s.active_state,
    s.sub_state,
    s.is_enabled,
    ssh.uptime_seconds,
    ssh.memory_mb,
    ssh.cpu_percent,
    ssh.timestamp
FROM services s
LEFT JOIN LATERAL (
    SELECT *
    FROM service_status_history
    WHERE service_id = s.id
    ORDER BY timestamp DESC
    LIMIT 1
) ssh ON true
WHERE s.deleted_at IS NULL;

CREATE OR REPLACE VIEW service_metrics_latest AS
SELECT
    s.id,
    s.name,
    sm.cpu_percent,
    sm.memory_mb,
    sm.disk_read_bytes_per_sec,
    sm.disk_write_bytes_per_sec,
    sm.network_in_bytes_per_sec,
    sm.network_out_bytes_per_sec,
    sm.timestamp
FROM services s
LEFT JOIN LATERAL (
    SELECT *
    FROM service_metrics
    WHERE service_id = s.id
    ORDER BY timestamp DESC
    LIMIT 1
) sm ON true
WHERE s.deleted_at IS NULL;

-- Summary statistics
CREATE OR REPLACE VIEW service_summary AS
SELECT
    COUNT(DISTINCT CASE WHEN active_state = 'active' THEN id END) as active_count,
    COUNT(DISTINCT CASE WHEN active_state != 'active' THEN id END) as inactive_count,
    COUNT(*) as total_count,
    COUNT(DISTINCT CASE WHEN is_enabled THEN id END) as enabled_count
FROM services
WHERE deleted_at IS NULL;
