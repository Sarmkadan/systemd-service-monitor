# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help clean build publish run test docker-build docker-run docker-push \
        format lint install-tools watch logs stop

# Project information
PROJECT_NAME := systemd-service-monitor
PROJECT_VERSION := 1.0.0
BUILD_CONFIG := Release
OUTPUT_DIR := ./publish
DOCKER_REGISTRY := docker.io
DOCKER_IMAGE := $(PROJECT_NAME):latest

# Colors for output
RED := \033[0;31m
GREEN := \033[0;32m
YELLOW := \033[1;33m
NC := \033[0m

# Default target
.DEFAULT_GOAL := help

help: ## Show this help message
	@echo "$(GREEN)$(PROJECT_NAME) - Makefile Targets$(NC)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  $(YELLOW)%-20s$(NC) %s\n", $$1, $$2}'
	@echo ""

# Build targets
clean: ## Clean build artifacts
	@echo "$(YELLOW)Cleaning build artifacts...$(NC)"
	dotnet clean -c $(BUILD_CONFIG)
	rm -rf $(OUTPUT_DIR)
	rm -rf bin obj
	@echo "$(GREEN)✓ Clean complete$(NC)"

restore: ## Restore NuGet dependencies
	@echo "$(YELLOW)Restoring dependencies...$(NC)"
	dotnet restore
	@echo "$(GREEN)✓ Restore complete$(NC)"

build: restore ## Build the project
	@echo "$(YELLOW)Building $(PROJECT_NAME)...$(NC)"
	dotnet build -c $(BUILD_CONFIG)
	@echo "$(GREEN)✓ Build complete$(NC)"

publish: clean restore ## Publish release build
	@echo "$(YELLOW)Publishing $(PROJECT_NAME)...$(NC)"
	dotnet publish -c $(BUILD_CONFIG) -o $(OUTPUT_DIR)
	@echo "$(GREEN)✓ Publish complete to $(OUTPUT_DIR)$(NC)"

# Development targets
run: ## Run in development mode
	@echo "$(YELLOW)Running $(PROJECT_NAME) in development mode...$(NC)"
	dotnet run

watch: ## Run with file watching
	@echo "$(YELLOW)Running with file watching...$(NC)"
	dotnet watch run

test: restore ## Run tests
	@echo "$(YELLOW)Running tests...$(NC)"
	dotnet test --no-build
	@echo "$(GREEN)✓ Tests complete$(NC)"

# Code quality targets
format: ## Format code with dotnet format
	@echo "$(YELLOW)Formatting code...$(NC)"
	dotnet format
	@echo "$(GREEN)✓ Format complete$(NC)"

lint: ## Run code analysis
	@echo "$(YELLOW)Running code analysis...$(NC)"
	dotnet build -c $(BUILD_CONFIG) /p:EnforceCodeStyleInBuild=true
	@echo "$(GREEN)✓ Analysis complete$(NC)"

# Docker targets
docker-build: ## Build Docker image
	@echo "$(YELLOW)Building Docker image...$(NC)"
	docker build -t $(DOCKER_IMAGE) .
	@echo "$(GREEN)✓ Docker image built: $(DOCKER_IMAGE)$(NC)"

docker-run: docker-build ## Run Docker container
	@echo "$(YELLOW)Starting Docker container...$(NC)"
	docker run -d \
		--name $(PROJECT_NAME) \
		--privileged \
		-p 5001:5001 \
		-v /run/dbus/system_bus_socket:/run/dbus/system_bus_socket \
		-v /var/log/journal:/var/log/journal:ro \
		$(DOCKER_IMAGE)
	@echo "$(GREEN)✓ Container started$(NC)"
	@echo "   Access at: https://localhost:5001"

docker-stop: ## Stop Docker container
	@echo "$(YELLOW)Stopping Docker container...$(NC)"
	docker stop $(PROJECT_NAME) || true
	docker rm $(PROJECT_NAME) || true
	@echo "$(GREEN)✓ Container stopped$(NC)"

docker-logs: ## View Docker container logs
	docker logs -f $(PROJECT_NAME)

docker-push: ## Push Docker image to registry
	@echo "$(YELLOW)Pushing Docker image...$(NC)"
	docker tag $(DOCKER_IMAGE) $(DOCKER_REGISTRY)/$(DOCKER_IMAGE)
	docker push $(DOCKER_REGISTRY)/$(DOCKER_IMAGE)
	@echo "$(GREEN)✓ Image pushed$(NC)"

# Compose targets
compose-up: ## Start with Docker Compose
	@echo "$(YELLOW)Starting services with Docker Compose...$(NC)"
	docker-compose up -d
	@echo "$(GREEN)✓ Services started$(NC)"
	@echo "   Access at: https://localhost:5001"

compose-down: ## Stop Docker Compose services
	@echo "$(YELLOW)Stopping services...$(NC)"
	docker-compose down
	@echo "$(GREEN)✓ Services stopped$(NC)"

compose-logs: ## View Docker Compose logs
	docker-compose logs -f

# Installation targets
install-tools: ## Install development tools
	@echo "$(YELLOW)Installing development tools...$(NC)"
	dotnet tool update -g dotnet-ef
	dotnet tool update -g dotnet-format
	@echo "$(GREEN)✓ Tools installed$(NC)"

install-service: publish ## Install as systemd service
	@echo "$(YELLOW)Installing as systemd service...$(NC)"
	sudo mkdir -p /opt/$(PROJECT_NAME)
	sudo cp -r $(OUTPUT_DIR)/* /opt/$(PROJECT_NAME)/
	sudo cp examples/systemd-service-monitor.service /etc/systemd/system/
	sudo systemctl daemon-reload
	sudo systemctl enable $(PROJECT_NAME)
	@echo "$(GREEN)✓ Service installed$(NC)"
	@echo "   To start: sudo systemctl start $(PROJECT_NAME)"
	@echo "   To view logs: sudo journalctl -u $(PROJECT_NAME) -f"

uninstall-service: ## Uninstall systemd service
	@echo "$(YELLOW)Uninstalling systemd service...$(NC)"
	sudo systemctl stop $(PROJECT_NAME) || true
	sudo systemctl disable $(PROJECT_NAME) || true
	sudo rm -rf /opt/$(PROJECT_NAME)
	sudo rm -f /etc/systemd/system/$(PROJECT_NAME).service
	sudo systemctl daemon-reload
	@echo "$(GREEN)✓ Service uninstalled$(NC)"

# Monitoring targets
logs: ## View application logs
	tail -f logs/systemd-monitor-*.txt

status: ## Check systemd service status
	sudo systemctl status $(PROJECT_NAME)

restart-service: ## Restart systemd service
	sudo systemctl restart $(PROJECT_NAME)

stop: ## Stop the application
	@echo "$(YELLOW)Stopping application...$(NC)"
	sudo systemctl stop $(PROJECT_NAME) || true
	docker stop $(PROJECT_NAME) || true
	@echo "$(GREEN)✓ Application stopped$(NC)"

# Documentation targets
docs: ## Generate API documentation
	@echo "$(YELLOW)Documentation available at:$(NC)"
	@echo "  - README.md"
	@echo "  - docs/getting-started.md"
	@echo "  - docs/architecture.md"
	@echo "  - docs/api-reference.md"
	@echo "  - docs/deployment.md"
	@echo "  - docs/faq.md"

examples: ## Show examples
	@echo "$(YELLOW)Example files:$(NC)"
	@ls -lh examples/
	@echo ""
	@echo "Run examples:"
	@echo "  - bash examples/monitoring-script.sh"
	@echo "  - bash examples/check_systemd_service.sh -s nginx.service"
	@echo "  - dotnet examples/ServiceMonitorClient.cs"

# Utility targets
version: ## Show project version
	@echo "$(GREEN)$(PROJECT_NAME) v$(PROJECT_VERSION)$(NC)"

info: ## Show build information
	@echo "$(GREEN)Build Information$(NC)"
	@echo "  Project: $(PROJECT_NAME)"
	@echo "  Version: $(PROJECT_VERSION)"
	@echo "  Config: $(BUILD_CONFIG)"
	@echo "  Output: $(OUTPUT_DIR)"
	@dotnet --version

deps: ## Show dependencies
	@echo "$(YELLOW)NuGet Dependencies:$(NC)"
	@grep "PackageReference" systemd-service-monitor.csproj | sed 's/.*Include="/  /' | sed 's/" Version.*//'

# Verification targets
verify: build test lint ## Run all verification checks
	@echo "$(GREEN)✓ All verifications passed$(NC)"

pre-commit: format lint test ## Pre-commit checks
	@echo "$(GREEN)✓ Ready to commit$(NC)"

# CI/CD targets
ci: clean restore build test lint ## Run CI pipeline
	@echo "$(GREEN)✓ CI pipeline complete$(NC)"

release: publish docker-build ## Build release artifacts
	@echo "$(GREEN)✓ Release built$(NC)"
	@echo "  - Binary: $(OUTPUT_DIR)"
	@echo "  - Docker: $(DOCKER_IMAGE)"

# Debugging
debug: ## Run with debug configuration
	@echo "$(YELLOW)Building with Debug configuration...$(NC)"
	dotnet build -c Debug
	@echo "$(YELLOW)Running with debugging enabled...$(NC)"
	dotnet run --configuration Debug

# Performance
profile: ## Profile application startup
	@echo "$(YELLOW)Running startup profile...$(NC)"
	time dotnet run
