#!/bin/bash

echo "🏗️ Building Restaurant Menu API Solution..."

# Build the solution
echo "📦 Restoring packages..."
dotnet restore RestaurantMenuAPI.sln

echo "🔨 Building solution..."
dotnet build RestaurantMenuAPI.sln --configuration Release

if [ $? -eq 0 ]; then
    echo "✅ Build completed successfully!"
    
    echo "🧪 Running tests..."
    dotnet test tests/RestaurantMenuAPI.Tests/RestaurantMenuAPI.Tests.csproj --configuration Release --verbosity normal
    
    if [ $? -eq 0 ]; then
        echo "✅ All tests passed!"
    else
        echo "❌ Some tests failed!"
        exit 1
    fi
else
    echo "❌ Build failed!"
    exit 1
fi

echo ""
echo "🎉 Solution build and test completed successfully!"
