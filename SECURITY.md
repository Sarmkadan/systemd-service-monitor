# Security Policy

## Reporting a Vulnerability

We take security seriously. If you discover a security vulnerability in systemd-service-monitor, please report it responsibly.

**DO NOT open a public GitHub issue for security vulnerabilities.**

Instead, please use one of the following methods:

### GitHub Private Vulnerability Reporting (Recommended)

Report the vulnerability using GitHub's Private Vulnerability Reporting feature:
https://github.com/sarmkadan/systemd-service-monitor/security/advisories/new

This allows secure, private communication with the maintainers.

### Email

If you cannot use GitHub's vulnerability reporting, email your report to:
**rutova2@gmail.com**

Please include:
- Description of the vulnerability
- Steps to reproduce (if applicable)
- Potential impact
- Suggested remediation (if you have one)

## Response Timeline

- **Initial Acknowledgment**: We aim to acknowledge your report within 48 hours
- **Assessment**: We will conduct an initial assessment and provide an update within 1 week
- **Resolution**: Timelines vary based on severity, but we prioritize security fixes

## Supported Versions

Security updates are provided for:
- **v1.x** - Current supported version

Older versions are not actively maintained with security updates. We recommend updating to the latest version.

## Security Best Practices

When using systemd-service-monitor:

1. **D-Bus Permissions**: Ensure D-Bus policies restrict access to systemd services appropriately
2. **Network Security**: Use HTTPS and proper authentication in production
3. **Access Control**: Implement authentication and authorization for the web dashboard
4. **Keep Updated**: Apply security updates promptly when released
5. **Audit Logs**: Monitor and review application logs for suspicious activity

## Disclosure Policy

Once a security issue is fixed and released:
1. A security advisory will be published
2. The fix will be included in a release with appropriate version bump
3. Credit will be given to the reporter (unless you prefer to remain anonymous)

Thank you for helping keep systemd-service-monitor secure!
