#!/bin/bash

if [ "$KEEP_ALIVE" = "true" ]; then
    echo "[docker-test-entrypoint] KEEP_ALIVE is true: keeping container alive for exec commands."
    tail -f /dev/null
else
    echo "[docker-test-entrypoint] Running tests in normal mode."
    exec dotnet test tests/RestaurantMenuAPI.Tests/RestaurantMenuAPI.Tests.csproj --no-build --configuration Release --verbosity detailed --logger "trx;LogFileName=test-results.trx" --results-directory /app/TestResults
fi
