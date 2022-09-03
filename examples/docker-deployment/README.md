# Docker Deployment Example

This example demonstrates how to deploy systemd-service-monitor using Docker Compose.

## Quick Start

1. Build the Docker image:
   ```bash
   docker-compose build
   ```

2. Start the service:
   ```bash
   docker-compose up -d
   ```

3. Access the web interface:
   ```bash
   open http://localhost
   ```

## Configuration

The example uses the following configuration:

- Port 80 exposed to host
- D-Bus system socket mounted as read-only
- Production environment
- Alerts enabled

## Customization

To customize the deployment:

1. Edit the `docker-compose.yml` file
2. Update environment variables as needed
3. Rebuild the image

## Production Considerations

For production deployments:
- Add authentication
- Configure HTTPS
- Set appropriate resource limits
- Enable proper logging