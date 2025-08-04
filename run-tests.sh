#!/bin/bash

echo "🧪 Starting Restaurant Menu API Automated Tests..."
echo "📋 Using self-contained test project and 'docker compose up'"
echo ""

# Ensure we start from a clean slate
docker compose -f docker-compose.test.yml down --volumes

echo "🚀 Building and starting test environment..."
docker compose -f docker-compose.test.yml up -d --build

# Wait for the test service to be healthy (optional: implement health check logic if needed)
sleep 5

# List all test cases in the test project using the running container
CONTAINER_ID=$(docker compose -f docker-compose.test.yml ps -q restaurant-api-tests)
if [ -z "$CONTAINER_ID" ]; then
    echo "❌ Test container not found!"
    docker compose -f docker-compose.test.yml down --volumes
    exit 1
fi

TESTS=$(docker exec $CONTAINER_ID dotnet test tests/RestaurantMenuAPI.Tests/RestaurantMenuAPI.Tests.csproj --configuration Release --list-tests --no-build --logger "console;verbosity=detailed" | grep -E '^[ ]{4,}[^ ]' | awk '{$1=$1};1')

if [ -z "$TESTS" ]; then
    echo "❌ No tests found!"
    docker compose -f docker-compose.test.yml down --volumes
    exit 1
fi

echo "=================================================="
echo "🧪 Running each test individually in the same container..."
echo "=================================================="

PASS_COUNT=0
FAIL_COUNT=0
FAILED_TESTS=()

for TEST in $TESTS; do
    echo "\n▶️ Running: $TEST"
    docker exec $CONTAINER_ID dotnet test tests/RestaurantMenuAPI.Tests/RestaurantMenuAPI.Tests.csproj --configuration Release --no-build --filter "FullyQualifiedName=$TEST"
    EXIT_CODE=$?
    if [ $EXIT_CODE -eq 0 ]; then
        echo "✅ $TEST PASSED"
        PASS_COUNT=$((PASS_COUNT+1))
    else
        echo "❌ $TEST FAILED"
        FAIL_COUNT=$((FAIL_COUNT+1))
        FAILED_TESTS+=("$TEST")
    fi
    echo "----------------------------------------"
done

echo "\n=================================================="
echo "📊 FINAL TEST RESULTS SUMMARY"
echo "=================================================="
echo "✅ PASSED: $PASS_COUNT"
echo "❌ FAILED: $FAIL_COUNT"
if [ $FAIL_COUNT -ne 0 ]; then
    echo "\nFailed tests:"
    for T in "${FAILED_TESTS[@]}"; do
        echo "  - $T"
    done
    final_exit_code=1
else
    echo "🎉 OVERALL RESULT: ALL TESTS PASSED! ✅"
    final_exit_code=0
fi

echo "\n🧹 Cleaning up test environment..."
docker compose -f docker-compose.test.yml down --volumes

echo ""
if [ $final_exit_code -eq 0 ]; then
    echo "✅ Test execution completed successfully!"
else
    echo "❌ Test execution failed with exit code: $final_exit_code"
fi
exit $final_exit_code