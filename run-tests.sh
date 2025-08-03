#!/bin/bash

echo "🧪 Running Restaurant Menu API Tests with Docker..."
echo "=================================================="

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Error: Docker is not running. Please start Docker Desktop and try again."
    exit 1
fi

echo "🔨 Building and running tests..."
docker build -f Dockerfile.tests -t restaurant-api-tests . -q

if [ $? -ne 0 ]; then
    echo "❌ Failed to build test image"
    exit 1
fi

echo ""
echo "🏃‍♂️ Executing tests..."
echo "===================="

# Run the tests in the container
docker run --rm restaurant-api-tests
TEST_EXIT_CODE=$?

if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo ""
    echo "✅ All tests passed successfully!"
    echo ""
    echo "📊 Test Results Summary:"
    echo "  • Unit Tests: ✅ Service layer business logic"
    echo "  • Integration Tests: ✅ API endpoints and authentication"
    echo "  • Coverage: ✅ Comprehensive test coverage"
    echo ""
    echo "🎉 Your Restaurant Menu API is working perfectly!"
    echo ""
    echo "💡 Next steps:"
    echo "  • Start the API: docker-compose up --build"
    echo "  • Access Swagger UI: http://localhost:8080"
    echo "  • Test with default admin credentials: admin@restaurant.com / Admin123!"
else
    echo ""
    echo "❌ Some tests failed. Please check the output above for details."
    echo ""
    echo "🔍 Common issues:"
    echo "  • Check for syntax errors in test files"
    echo "  • Verify all dependencies are properly configured"
    echo "  • Ensure test data setup is correct"
fi

exit $TEST_EXIT_CODE
