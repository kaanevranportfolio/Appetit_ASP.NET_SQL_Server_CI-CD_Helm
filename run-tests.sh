#!/bin/bash
echo "🧪 Starting Restaurant Menu API Automated Tests..."
echo "📋 Using self-contained Dockerfile and 'docker compose up'"
echo ""

# Ensure we start from a clean slate
docker compose -f docker-compose.test.yml down --volumes

echo "🚀 Building and running test environment..."

# 'up' will build the image, start the database, wait for it to be healthy,
# and then start the test container.
# '--abort-on-container-exit' ensures that as soon as the test container
# finishes (passes or fails), all other containers are stopped.
docker compose -f docker-compose.test.yml up --build --abort-on-container-exit

# Capture the exit code of the test container
TEST_EXIT_CODE=$?

echo ""
echo "=================================================="
echo "📊 FINAL TEST RESULTS SUMMARY"
echo "=================================================="

if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo "🎉 OVERALL RESULT: ALL TESTS PASSED! ✅"
    final_exit_code=0
else
    echo "💥 OVERALL RESULT: SOME TESTS FAILED! ❌"
    echo "Check the detailed output above to see which specific tests failed."
    final_exit_code=1
fi

echo ""
echo "🧹 Cleaning up test environment..."
docker compose -f docker-compose.test.yml down --volumes

echo ""
if [ $final_exit_code -eq 0 ]; then
    echo "✅ Test execution completed successfully!"
else
    echo "❌ Test execution failed with exit code: $final_exit_code"
fi

exit $final_exit_code