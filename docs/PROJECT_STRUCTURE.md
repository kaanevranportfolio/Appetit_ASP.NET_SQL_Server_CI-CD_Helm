# .NET Solution Structure

This project follows professional .NET solution standards with separate projects for the main application and tests.

## Project Structure

```
RestaurantMenuAPI/
├── RestaurantMenuAPI.sln              # Solution file
├── src/                               # Source code
│   └── RestaurantMenuAPI/             # Main API project
│       ├── Controllers/               # API controllers
│       ├── Data/                      # Database context
│       ├── DTOs/                      # Data transfer objects
│       ├── Models/                    # Entity models
│       ├── Services/                  # Business logic services
│       ├── Program.cs                 # Application entry point
│       ├── appsettings.json           # Configuration
│       └── RestaurantMenuAPI.csproj   # Project file
├── tests/                             # Test projects
│   └── RestaurantMenuAPI.Tests/       # Unit/Integration tests
│       ├── RestaurantMenuAPI.Tests.csproj
│       └── *.cs                       # Test files
├── docker-compose.yml                 # Production Docker setup
├── docker-compose.test.yml            # Test environment
├── Dockerfile                         # Main app container
├── Dockerfile.tests                   # Test container
└── run-tests.sh                       # Test execution script
```

## Building and Running

### Using Docker (Recommended)
```bash
# Run the application
docker compose up --build

# Run tests
./run-tests.sh
```

### Using .NET CLI (requires .NET 8.0 SDK)
```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run the application
dotnet run --project src/RestaurantMenuAPI/RestaurantMenuAPI.csproj
```

## Professional Standards

This project structure follows .NET best practices:

- **Separation of Concerns**: Clear separation between application code and tests
- **Solution-based**: Uses `.sln` file to manage multiple projects
- **Test Project**: Dedicated test project with proper references
- **Docker Integration**: Full containerization support
- **CI/CD Ready**: Structure supports automated testing and deployment
