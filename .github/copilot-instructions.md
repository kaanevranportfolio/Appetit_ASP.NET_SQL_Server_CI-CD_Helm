<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# Restaurant Menu & Reservation API - Copilot Instructions

## Project Overview
This is a comprehensive ASP.NET Core 8.0 Web API for restaurant management with the following key features:
- Menu and category management with CRUD operations
- Reservation system with table scheduling and availability tracking
- User authentication with JWT and role-based authorization (Guest, Staff, Admin)
- Booking limits and menu item availability tracking
- Fully containerized with Docker and SQL Server

## Architecture Guidelines

### Code Organization
- Follow clean architecture principles with separation of concerns
- Use DTOs for data transfer between layers
- Implement services for business logic
- Controllers should be thin and focused on HTTP concerns
- Use Entity Framework Core for data access with proper relationships

### Authentication & Authorization
- JWT Bearer token authentication is configured
- Three roles: Guest, Staff, Admin with hierarchical permissions
- Use `[Authorize]` and `[Authorize(Roles = "...")]` attributes
- Always validate user permissions in services when needed

### Database Design
- Entity Framework Core with SQL Server
- All entities have audit fields (CreatedAt, UpdatedAt)
- Use proper foreign key relationships and constraints
- Implement soft deletes where appropriate (IsActive flags)

### Error Handling
- Use try-catch blocks in controllers
- Return appropriate HTTP status codes
- Provide meaningful error messages
- Log errors using Serilog

### API Design
- Follow RESTful conventions
- Use appropriate HTTP verbs (GET, POST, PUT, DELETE, PATCH)
- Return DTOs instead of entity models
- Include proper status codes and responses
- Document all endpoints with XML comments for Swagger

## Development Standards

### Naming Conventions
- Use PascalCase for classes, methods, and properties
- Use camelCase for parameters and local variables
- Prefix interfaces with 'I'
- Use descriptive names for variables and methods

### Async/Await Patterns
- Always use async/await for database operations
- Return Task<T> from service methods
- Use ConfigureAwait(false) where appropriate
- Handle async exceptions properly

### Validation
- Use Data Annotations for model validation
- Implement custom validation logic in services
- Validate business rules before database operations
- Return validation errors to controllers

### Security Best Practices
- Never expose sensitive data in responses
- Validate all inputs
- Use parameterized queries (EF handles this)
- Implement proper CORS policies
- Hash passwords using Identity framework

## Docker & Deployment

### Containerization
- Multi-stage Dockerfile for optimized builds
- Docker Compose for local development with SQL Server
- Environment-specific configuration files
- Health checks and logging configuration

### Configuration
- Use appsettings.json for configuration
- Environment variables for sensitive data
- Separate settings for Development/Production
- JWT settings should be configurable

## Testing Guidelines

### Unit Testing
- Test business logic in services
- Mock dependencies using interfaces
- Test both success and failure scenarios
- Use xUnit as the testing framework

### Integration Testing
- Test API endpoints end-to-end
- Use TestServer for integration tests
- Test authentication and authorization
- Verify database operations

## Common Patterns

### Service Pattern
```csharp
public interface IMenuService
{
    Task<IEnumerable<MenuItemDto>> GetMenuItemsAsync();
    Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto dto);
}
```

### Controller Pattern
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuItems()
{
    var items = await _menuService.GetMenuItemsAsync();
    return Ok(items);
}
```

### Error Handling Pattern
```csharp
try
{
    var result = await _service.SomeOperationAsync();
    return Ok(result);
}
catch (ArgumentException ex)
{
    return BadRequest(new { message = ex.Message });
}
catch (Exception ex)
{
    return StatusCode(500, new { message = "Internal server error" });
}
```

## Key Models and Relationships

### Core Entities
- ApplicationUser (extends IdentityUser)
- Category -> MenuItems (1:many)
- Table -> Reservations (1:many)
- User -> Reservations (1:many)
- Reservation -> Order (1:1 optional)
- Order -> OrderItems (1:many)
- MenuItem -> OrderItems (1:many)

### Business Rules
- Reservations have time slots and capacity limits
- Menu items can have availability quantities
- Users have reservation limits
- Tables have capacity constraints
- Restaurant has operating hours and advance booking limits

## Environment Setup
- .NET 8.0 SDK required
- Docker Desktop for containerization
- SQL Server (via Docker)
- Swagger UI available at root URL in development
