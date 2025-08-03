#!/bin/bash
echo "🔍 Debugging SQL Server..."

# Start SQL Server
docker compose -f docker-compose.test.yml up -d sqlserver-test

# Wait a bit
sleep 30

# Check container status
echo "📊 Container Status:"
docker compose -f docker-compose.test.yml ps

# Check logs
echo "📋 SQL Server Logs:"
docker compose -f docker-compose.test.yml logs sqlserver-test

# Try to connect
echo "🔌 Testing Connection:"
docker compose -f docker-compose.test.yml exec sqlserver-test /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT @@VERSION"

# Cleanup
docker compose -f docker-compose.test.yml down --volumes