using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using MongoDB.Driver;

using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.Filters;
using BikeShowroomAPI.Models.MongoDB;
using BikeShowroomAPI.Services;
using BikeShowroomAPI.Services.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers(options =>
{
    // Automatically record an audit event for every successful write (POST/PUT/DELETE)
    // by forwarding it to the AuditLogService microservice.
    options.Filters.Add<AuditActionFilter>();
})
.AddJsonOptions(options =>
{
    // Prevent "possible object cycle detected" errors when serializing MongoDB
    // entities with circular navigation properties (e.g. Sale <-> SaleItem).
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your JWT token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "bearer",
                BearerFormat = "JWT",
                Name = "Authorization",
                In = ParameterLocation.Header
            },
            new string[] {}
        }
    });
});

// MongoDB Configuration
builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("MongoDB")
        ?? "mongodb://localhost:27017";
    options.DatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "BikeShowroomDB";
});

// Register MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(sp.GetRequiredService<IOptions<MongoDbSettings>>().Value.ConnectionString));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(
        sp.GetRequiredService<IOptions<MongoDbSettings>>().Value.DatabaseName));
builder.Services.AddScoped<MongoDbContext>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSecureSecretKeyHere_ChangeThisInProduction_MinimumLength32Characters!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BikeShowroomAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "BikeShowroomClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register MongoDB services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();
builder.Services.AddScoped<IUserService, UserService>();

// AuditLogService microservice client
builder.Services.AddHttpClient<IAuditService, AuditService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["AuditService:BaseUrl"] ?? "http://localhost:5002"
    );
});

var app = builder.Build();

// Seed initial data (roles, admin user)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding data");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Seed data for MongoDB
static async Task SeedData(IServiceProvider services)
{
    var context = services.GetRequiredService<MongoDbContext>();

    // Ensure indexes are created
    await context.EnsureIndexesAsync();

    // Check whether the default admin user already exists.
    // Do not skip seeding just because other users exist in the database.
    var adminExists = await context.Users.CountDocumentsAsync(u =>
        u.Email == "admin@bikeshowroom.com") > 0;
    if (adminExists)
        return;

    // Create default SuperAdmin user
    var superAdmin = new ApplicationUser
    {
        UserName = "admin@bikeshowroom.com",
        NormalizedUserName = "ADMIN@BIKESHOWROOM.COM",
        Email = "admin@bikeshowroom.com",
        NormalizedEmail = "ADMIN@BIKESHOWROOM.COM",
        EmailConfirmed = true,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        SecurityStamp = Guid.NewGuid().ToString(),
        PhoneNumber = "",
        PhoneNumberConfirmed = false,
        TwoFactorEnabled = false,
        LockoutEnabled = true,
        AccessFailedCount = 0,
        FirstName = "Super",
        LastName = "Admin",
        CompanyId = null,
        BranchId = null,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        Roles = new List<string> { "SuperAdmin" }
    };

    await context.Users.InsertOneAsync(superAdmin);
}