# Development Setup Instructions

## Option 1: Docker Setup (Recommended)

### Prerequisites
1. **Install Docker Desktop** from https://www.docker.com/products/docker-desktop
2. **Start Docker Desktop** - Make sure it's running (you'll see the Docker icon in your system tray)

### Quick Start
```bash
# Build and run with Docker Compose
docker-compose up --build

# Or run in detached mode
docker-compose up --build -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Access Points
- **API**: http://localhost:8080
- **Swagger UI**: http://localhost:8080 (root URL)
- **Database**: localhost:1433 (SQL Server)

### Default Credentials
- **Admin User**: admin@restaurant.com / Admin123!
- **Database**: sa / YourStrong@Passw0rd

## Option 2: Local Development (Without Docker)

### Prerequisites
1. **.NET 8 SDK** (already installed ✅)
2. **SQL Server LocalDB** or **SQL Server Express**

### Setup Steps

#### 1. Install SQL Server LocalDB
```bash
# Download SQL Server Express with LocalDB
# https://www.microsoft.com/en-us/sql-server/sql-server-downloads
```

#### 2. Update Connection String
Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RestaurantMenuDB_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

#### 3. Run the Application
```bash
# Restore packages and build
dotnet restore
dotnet build

# Run the application
dotnet run

# Or with hot reload
dotnet watch run
```

### Access Points (Local)
- **API**: https://localhost:7080 or http://localhost:5080
- **Swagger UI**: https://localhost:7080 (root URL)

## Option 3: Cloud Development (Alternative)

### Using GitHub Codespaces or Docker in Cloud
1. **GitHub Codespaces**: Open in Codespaces for instant development environment
2. **Cloud Docker**: Use cloud-based Docker services

## Troubleshooting

### Docker Issues
- **Docker Desktop not running**: Start Docker Desktop from Start menu
- **Port conflicts**: Change ports in docker-compose.yml if 8080 or 1433 are in use
- **Memory issues**: Allocate more memory to Docker in settings

### Local Development Issues
- **Database connection**: Ensure SQL Server LocalDB is installed and running
- **Port conflicts**: Check if ports 5080/7080 are available
- **Certificate issues**: Run `dotnet dev-certs https --trust`

## API Testing

### Using Swagger UI
1. Navigate to the root URL of your running application
2. Use the "Authorize" button to authenticate with JWT tokens
3. Test all endpoints interactively

### Using curl or Postman
```bash
# Register a new user
curl -X POST "http://localhost:8080/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User"
  }'

# Login to get JWT token
curl -X POST "http://localhost:8080/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@restaurant.com",
    "password": "Admin123!"
  }'

# Use the token in subsequent requests
curl -X GET "http://localhost:8080/api/menu/items" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Development Workflow

### Making Changes
1. **Edit code** - Changes will be reflected after rebuild
2. **Database changes** - Modify models and add migrations
3. **Testing** - Use Swagger UI or Postman for API testing
4. **Logs** - Check console output or log files in `logs/` directory

### Database Migrations (if needed)
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

## Next Steps

1. **Start Docker Desktop** and run `docker-compose up --build`
2. **Access Swagger UI** at http://localhost:8080
3. **Test the API** with the default admin credentials
4. **Explore the endpoints** and create test data
5. **Build your frontend** or integrate with existing systems

---

**Choose the setup method that works best for your environment!**
