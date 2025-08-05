#!/bin/bash

# Load Testing Script for HPA Verification
# This script will be used to test horizontal pod autoscaling

echo "🚀 Starting HPA Load Testing..."

# Configuration
SERVICE_NAME="restaurantmenuapi"
NAMESPACE="default"
DURATION="5m"
CONCURRENT_USERS="50"

# Get service external IP
echo "📍 Getting service external IP..."
EXTERNAL_IP=$(kubectl get service $SERVICE_NAME -o jsonpath='{.status.loadBalancer.ingress[0].ip}')

if [ -z "$EXTERNAL_IP" ]; then
    echo "❌ Could not get external IP. Service might not be ready."
    exit 1
fi

SERVICE_URL="http://$EXTERNAL_IP"
echo "🎯 Service URL: $SERVICE_URL"

# Check initial pod count
echo "📊 Initial pod count:"
kubectl get pods -l app.kubernetes.io/name=restaurantmenuapi

# Check HPA status
echo "📈 HPA Status:"
kubectl get hpa

echo "🔥 Starting load test..."
echo "Duration: $DURATION"
echo "Concurrent Users: $CONCURRENT_USERS"
echo "Target: $SERVICE_URL"

# Install hey load testing tool if not available
if ! command -v hey &> /dev/null; then
    echo "Installing hey load testing tool..."
    wget -O hey https://hey-release.s3.us-east-2.amazonaws.com/hey_linux_amd64
    chmod +x hey
    sudo mv hey /usr/local/bin/
fi

# Run load test
hey -z $DURATION -c $CONCURRENT_USERS $SERVICE_URL/health &
LOAD_TEST_PID=$!

# Monitor scaling every 30 seconds
echo "📊 Monitoring HPA scaling..."
for i in {1..10}; do
    echo "--- Minute $i ---"
    kubectl get hpa
    kubectl get pods -l app.kubernetes.io/name=restaurantmenuapi
    echo ""
    sleep 30
done

# Wait for load test to complete
wait $LOAD_TEST_PID

echo "🔥 Load test completed!"

# Monitor scale down
echo "📉 Monitoring scale down (waiting 10 minutes)..."
for i in {1..20}; do
    echo "--- Scale down check $i ---"
    kubectl get hpa
    kubectl get pods -l app.kubernetes.io/name=restaurantmenuapi
    echo ""
    sleep 30
done

echo "✅ HPA load testing completed!"

# Final results
echo "📊 Final Results:"
kubectl get hpa
kubectl get pods -l app.kubernetes.io/name=restaurantmenuapi
kubectl top pods -l app.kubernetes.io/name=restaurantmenuapi
