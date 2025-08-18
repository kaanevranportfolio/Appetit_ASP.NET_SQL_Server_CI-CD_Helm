using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestaurantMenuAPI.Data;
using RestaurantMenuAPI.Models;
using RestaurantMenuAPI.Services;
using Serilog;
using System.Text;
using DotNetEnv;

// Load .env file if it exists
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/restaurant-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

//
// Connection string logic explanation:
// Two modes of operation:
// 1. Container/Production mode: Uses environment variables (for Docker/Cloud/K8s)
// 2. Development mode: Uses appsettings.Development.json (for local development)
//
string? connectionString;

// Check if we're running in a container (Docker/K8s) or production environment
if (!builder.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    // Container/Production mode - build connection string from environment variables
    string dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "sqlserver";
    string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "AppetitDb";
    string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
    string? dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    
    if (string.IsNullOrEmpty(dbPassword))
    {
        throw new InvalidOperationException(
            "Database password not found. Please provide it through the DB_PASSWORD environment variable " +
            "when running in container or production mode.");
    }
    
    connectionString = $"Server={dbHost};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=true;";
}
else
{
    // Development mode - use connection string from appsettings.Development.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string not found in appsettings.Development.json. " +
            "Please configure your local development database connection string.");
    }
}

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication Configuration
string secretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey") ?? 
                   Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                   "TestSecretKey123456789012345678901234567890"; // Fallback for tests

string issuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer") ?? 
                Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
                "TestRestaurantAPI"; // Fallback for tests

string audience = Environment.GetEnvironmentVariable("JwtSettings__Audience") ?? 
                  Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
                  "TestRestaurantUsers"; // Fallback for tests

if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey must be at least 32 characters long. Please set JwtSettings__SecretKey or JWT_SECRET_KEY environment variable.");
}

var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Service Registration
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Restaurant Menu & Reservation API",
        Description = "A comprehensive API for managing restaurant menus, reservations, and table scheduling",
        Contact = new OpenApiContact
        {
            Name = "Restaurant API Support",
            Email = "support@restaurant-api.com"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API v1");
        options.RoutePrefix = string.Empty; // Makes Swagger UI available at the root URL
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

// Database initialization and seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        await Program.SeedRolesAsync(roleManager);

        // Seed admin user
        await Program.SeedAdminUserAsync(userManager);

        // Seed sample menu items
        await Program.SeedMenuItemsAsync(context);

        Log.Information("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while initializing the database");
    }
}

app.Run();

public partial class Program {
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Guest", "Staff", "Admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@restaurant.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    public static async Task SeedMenuItemsAsync(ApplicationDbContext context)
    {
        if (!context.MenuItems.Any())
        {
            // Ensure categories exist
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Id = 1, Name = "Appetizers" },
                    new Category { Id = 2, Name = "Main Courses" },
                    new Category { Id = 3, Name = "Desserts" },
                    new Category { Id = 4, Name = "Beverages" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            var menuItems = new List<MenuItem>
            {
                // Appetizers
                new MenuItem { Name = "Caesar Salad", Description = "Fresh romaine lettuce with Caesar dressing, croutons, and parmesan cheese", Price = 12.99m, CategoryId = 1, IsAvailable = true },
                new MenuItem { Name = "Bruschetta", Description = "Grilled bread topped with fresh tomatoes, basil, and mozzarella", Price = 8.99m, CategoryId = 1, IsAvailable = true },
                new MenuItem { Name = "Buffalo Wings", Description = "Spicy chicken wings served with blue cheese dip", Price = 14.99m, CategoryId = 1, IsAvailable = true, AvailableQuantity = 50 },

                // Main Courses
                new MenuItem { Name = "Grilled Salmon", Description = "Fresh Atlantic salmon grilled to perfection with lemon butter sauce", Price = 24.99m, CategoryId = 2, IsAvailable = true, AvailableQuantity = 20 },
                new MenuItem { Name = "Ribeye Steak", Description = "12oz prime ribeye steak cooked to your preference", Price = 32.99m, CategoryId = 2, IsAvailable = true, AvailableQuantity = 15 },
                new MenuItem { Name = "Chicken Alfredo", Description = "Grilled chicken breast over fettuccine pasta with creamy alfredo sauce", Price = 18.99m, CategoryId = 2, IsAvailable = true },
                new MenuItem { Name = "Vegetarian Pizza", Description = "Wood-fired pizza with fresh vegetables and mozzarella cheese", Price = 16.99m, CategoryId = 2, IsAvailable = true },

                // Desserts
                new MenuItem { Name = "Chocolate Lava Cake", Description = "Warm chocolate cake with molten center, served with vanilla ice cream", Price = 8.99m, CategoryId = 3, IsAvailable = true },
                new MenuItem { Name = "Tiramisu", Description = "Classic Italian dessert with coffee-soaked ladyfingers and mascarpone", Price = 7.99m, CategoryId = 3, IsAvailable = true },
                new MenuItem { Name = "Cheesecake", Description = "New York style cheesecake with berry compote", Price = 6.99m, CategoryId = 3, IsAvailable = true },

                // Beverages
                new MenuItem { Name = "House Wine (Glass)", Description = "Red or white wine from our selection", Price = 8.99m, CategoryId = 4, IsAvailable = true },
                new MenuItem { Name = "Craft Beer", Description = "Local craft beer on tap", Price = 5.99m, CategoryId = 4, IsAvailable = true },
                new MenuItem { Name = "Fresh Juice", Description = "Freshly squeezed orange, apple, or cranberry juice", Price = 4.99m, CategoryId = 4, IsAvailable = true },
                new MenuItem { Name = "Espresso", Description = "Rich Italian espresso", Price = 3.99m, CategoryId = 4, IsAvailable = true }
            };

            context.MenuItems.AddRange(menuItems);
            await context.SaveChangesAsync();
        }
    }
}