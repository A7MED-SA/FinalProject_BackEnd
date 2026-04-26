using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using backend_project.Services.Notifications;
using backend_project.Services.TeacherRequests;
using backend_project.Hubs;
using backend_project.Data;
using backend_project.Models;
using backend_project.Configuration;
using backend_project.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Settings
var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings not configured");
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Configure Email Settings
builder.Services.Configure<backend_project.Configuration.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // SignIn settings
    options.SignIn.RequireConfirmedEmail = false; // Set to true in production
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(origin =>
            {
                // يسمح بأي Origin داخل نفس الشبكة
                return origin.StartsWith("http://localhost")
                    || origin.StartsWith("http://192.168.")
                    || origin.StartsWith("http://10.")
                    || origin.StartsWith("http://172.");
            });
    });
});

builder.Services.AddAuthorization();

// Register Services
builder.Services.AddScoped<backend_project.Services.IEmailService, backend_project.Services.EmailService>();
builder.Services.AddScoped<backend_project.Services.IFileService, backend_project.Services.FileService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.ITokenService, backend_project.Services.TokenService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.ISessionService, backend_project.Services.SessionService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.IVerificationService, backend_project.Services.VerificationService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.IActivityLogService, backend_project.Services.ActivityLogService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.IPermissionService, backend_project.Services.PermissionService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.IOAuthService, backend_project.Services.OAuthService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.IAuthenticationService, backend_project.Services.AuthenticationService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.IProfileService, backend_project.Services.ProfileService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITeacherRequestService, TeacherRequestService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.ICategoryService, backend_project.Services.Implementations.CategoryService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.ICourseService, backend_project.Services.Implementations.CourseService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.ISectionService, backend_project.Services.Implementations.SectionService>();

// Course Edit Approval & Public Course Services
builder.Services.AddScoped<backend_project.Services.Interfaces.ICourseEditApprovalService, backend_project.Services.CourseEditApprovalService>();
builder.Services.AddScoped<backend_project.Services.Interfaces.IPublicCourseService, backend_project.Services.PublicCourseService>();

// Background Services
builder.Services.AddHostedService<backend_project.Services.Background.EditRequestCleanupService>();

// Register ALL FluentValidation Validators
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Register HttpClient for OAuth service
builder.Services.AddHttpClient();
builder.Services.AddSignalR();

// Services

// SignalR Hubs

// Register Media Services (MinIO, Media, Admin, Video Processing)
builder.Services.AddMediaServices(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var response = backend_project.DTOs.ApiResponse<object>.FailureResponse("Validation failed", errors);
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

// Configure Google OAuth (if credentials are provided)
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
if (!string.IsNullOrEmpty(googleClientId))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        });
}

// Configure Microsoft OAuth (if credentials are provided)
var microsoftClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
if (!string.IsNullOrEmpty(microsoftClientId))
{
    builder.Services.AddAuthentication()
        .AddMicrosoftAccount(options =>
        {
            options.ClientId = microsoftClientId;
            options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
        });
}

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi  
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await backend_project.Data.DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

// Add Global Exception Handler
app.UseMiddleware<backend_project.Middlewares.ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();