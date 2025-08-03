#!/bin/bash
set -e

# Fix for Windows Git Bash path conversion
export MSYS_NO_PATHCONV=1
export MSYS2_ARG_CONV_EXCL="*"

# Colors for better output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Load environment variables with proper line ending handling
if [ -f .env ]; then
    echo -e "${BLUE}📋 Loading environment variables from .env file...${NC}"
    export $(cat .env | sed 's/\r$//' | grep -v '^#' | grep -v '^$' | xargs)
else
    echo -e "${YELLOW}⚠️  No .env file found. Using default settings.${NC}"
    DB_PASSWORD="YourStrong@Passw0rd"
fi

# Cleanup function
cleanup() {
    echo -e "${BLUE}🧹 Cleaning up test environment...${NC}"
    docker-compose --profile testing down --volumes 2>/dev/null || true
}

# Set trap for cleanup on script exit
trap cleanup EXIT

echo -e "${BLUE}🧪 Running Restaurant Menu API Tests with Docker...${NC}"
echo "=================================================="

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}❌ Error: Docker is not running. Please start Docker Desktop and try again.${NC}"
    exit 1
fi

echo -e "${YELLOW}🔨 Building test images...${NC}"
if ! docker-compose --profile testing build --no-cache restaurant-api-tests; then
    echo -e "${RED}❌ Failed to build test image. Check Dockerfile.tests for issues.${NC}"
    exit 1
fi

echo -e "${YELLOW}🗄️ Starting test database...${NC}"
docker-compose --profile testing up -d sqlserver-test

echo -e "${YELLOW}⏳ Waiting for SQL Server to be ready...${NC}"

# Wait for the specific "ready for client connections" message
max_wait=180  # 3 minutes
elapsed=0
ready=false

while [ $elapsed -lt $max_wait ] && [ "$ready" = false ]; do
    # Check if the container is still running
    if ! docker-compose --profile testing ps sqlserver-test | grep -q "Up"; then
        echo -e "${RED}❌ SQL Server container stopped unexpectedly!${NC}"
        docker-compose --profile testing logs sqlserver-test
        exit 1
    fi
    
    # Check for the ready message in logs
    if docker-compose --profile testing logs sqlserver-test 2>/dev/null | grep -q "SQL Server is now ready for client connections"; then
        echo -e "${GREEN}✅ SQL Server reports ready for client connections!${NC}"
        
        # Wait for full initialization
        echo -e "${YELLOW}   Waiting 20 seconds for full initialization...${NC}"
        sleep 20
        
        # Test the connection with Windows-compatible path
        echo -e "${BLUE}   Testing database connection...${NC}"
        if docker-compose --profile testing exec -T sqlserver-test //opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${DB_PASSWORD:-YourStrong@Passw0rd}" -Q "SELECT 'Test successful!' AS Result" >/dev/null 2>&1; then
            ready=true
            echo -e "${GREEN}✅ Database connection test successful!${NC}"
        else
            echo -e "${YELLOW}   Connection test failed, waiting longer...${NC}"
            sleep 15
        fi
    fi
    
    # Show progress every 30 seconds
    if [ $((elapsed % 30)) -eq 0 ] && [ $elapsed -gt 0 ]; then
        echo -e "${BLUE}   Still waiting... (${elapsed}s elapsed)${NC}"
    fi
    
    sleep 5
    elapsed=$((elapsed + 5))
done

if [ "$ready" = false ]; then
    echo -e "${RED}❌ SQL Server failed to become ready within ${max_wait} seconds${NC}"
    echo -e "${RED}Container logs:${NC}"
    docker-compose --profile testing logs --tail=20 sqlserver-test
    exit 1
fi

echo ""
echo -e "${BLUE}🏃‍♂️ Executing tests...${NC}"
echo "===================="

# Run the tests
docker-compose --profile testing run --rm restaurant-api-tests
TEST_EXIT_CODE=$?

echo ""
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}✅ All tests passed successfully!${NC}"
    echo ""
    echo -e "${GREEN}🎉 Your Restaurant Menu API is working perfectly!${NC}"
else
    echo -e "${RED}❌ Some tests failed. Please check the output above.${NC}"
fi

exit $TEST_EXIT_CODE