# HPA Load Testing Documentation

## Overview
This document describes how to test the Horizontal Pod Autoscaler (HPA) functionality for the Restaurant Menu API.

## Current HPA Configuration

### Scaling Metrics
- **CPU Target**: 70% utilization
- **Memory Target**: 80% utilization
- **Min Replicas**: 2
- **Max Replicas**: 10

### Scaling Behavior
- **Scale Up**: 100% increase every 60 seconds (aggressive)
- **Scale Down**: 50% decrease every 60 seconds with 5-minute stabilization window (conservative)

## Load Testing Strategy

### Tools Options
1. **hey** - Simple HTTP load testing tool
2. **Apache Bench (ab)** - Classic load testing tool
3. **k6** - Modern load testing tool with JavaScript
4. **Artillery** - Node.js based load testing

### Test Scenarios

#### Scenario 1: CPU Stress Test
```bash
# High concurrent requests to trigger CPU scaling
hey -z 5m -c 50 http://SERVICE_IP/api/menu
```

#### Scenario 2: Memory Stress Test
```bash
# Requests that consume memory (e.g., large data operations)
hey -z 5m -c 20 http://SERVICE_IP/api/reservations
```

#### Scenario 3: Mixed Load Test
```bash
# Combination of different endpoints
./scripts/test-hpa-scaling.sh
```

## Monitoring Commands

### Watch HPA Status
```bash
kubectl get hpa --watch
```

### Monitor Pod Count
```bash
kubectl get pods -l app.kubernetes.io/name=restaurantmenuapi --watch
```

### Check Resource Usage
```bash
kubectl top pods -l app.kubernetes.io/name=restaurantmenuapi
```

### View HPA Events
```bash
kubectl describe hpa restaurantmenuapi
```

## Expected Behavior

### Scale Up Trigger
- When CPU > 70% or Memory > 80%
- Should add pods within 60 seconds
- Maximum 100% increase per minute

### Scale Down Trigger
- When CPU < 70% and Memory < 80%
- Should wait 5 minutes before scaling down
- Maximum 50% decrease per minute

## Future CI/CD Integration

### Planned Workflow Steps
1. **Deploy Application** ✅
2. **Wait for Deployment Ready**
3. **Get Service External IP**
4. **Run Load Test**
5. **Verify Scale Up**
6. **Stop Load Test**
7. **Verify Scale Down**
8. **Assert Test Results**

### Success Criteria
- [ ] Pods scale up under load
- [ ] CPU/Memory metrics trigger scaling
- [ ] Pods scale down after load stops
- [ ] Application remains responsive during scaling
- [ ] No pod crashes during scaling events

## Manual Testing Steps

1. **Deploy the application**
   ```bash
   helm upgrade --install restaurantmenuapi ./helm/restaurantmenuapi
   ```

2. **Get service IP**
   ```bash
   kubectl get service restaurantmenuapi
   ```

3. **Run load test**
   ```bash
   chmod +x scripts/test-hpa-scaling.sh
   ./scripts/test-hpa-scaling.sh
   ```

4. **Monitor scaling**
   ```bash
   # In separate terminals
   kubectl get hpa --watch
   kubectl get pods --watch
   kubectl top pods --watch
   ```

## Troubleshooting

### Common Issues
- **Metrics Server**: Ensure metrics-server is installed in cluster
- **Resource Requests**: Pods must have CPU/Memory requests defined
- **LoadBalancer**: Service needs external IP for load testing

### Debug Commands
```bash
# Check metrics server
kubectl get deployment metrics-server -n kube-system

# Check pod resource requests
kubectl describe pod <pod-name>

# Check HPA status
kubectl describe hpa restaurantmenuapi
```
