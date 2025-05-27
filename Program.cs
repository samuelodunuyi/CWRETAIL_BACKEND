// Program.cs
using CWSERVER.Data;
using CWSERVER.Filters;
using CWSERVER.Models.Entities;
using CWSERVER.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var corsAllowedOrigins = builder.Environment.IsDevelopment()
    ? new[] {
        "http://localhost:9898",
        "https://localhost:9898",
        "http://localhost:5173" 
      }
    : builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT Authentication support in Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lowercase
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });

    // Optional: Include XML comments if you have them
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // options.IncludeXmlComments(xmlPath);
});



builder.Services.AddScoped<ActiveUserFilter>();
//builder.Services.AddHttpContextAccessor();
// Add DbContext
//builder.Services.AddDbContext<ApiDbContext>(options =>
//    options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ApiDbContext>(options =>
options.UseSqlServer(
        config.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,             // default is 6
            maxRetryDelay: TimeSpan.FromSeconds(30), // default is 30
            errorNumbersToAdd: null       // keep null unless you want to retry specific SQL error codes
        )
    )
);

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApiDbContext>()
.AddDefaultTokenProviders();

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCorsPolicy", policy =>
    {
        policy.WithOrigins(corsAllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

//authentication stuff
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = config["JwtConfig:Issuer"],
        ValidAudience = config["JwtConfig:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["JwtConfig:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, 
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
            var jwtId = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);

            if (string.IsNullOrEmpty(jwtId))
                context.Fail("Invalid token");

            var isRevoked = await authService.IsTokenRevoked(jwtId);
            if (isRevoked)
                context.Fail("Token revoked");
        },

       
        OnMessageReceived = context =>
        {
            
            if (context.Request.Path.StartsWithSegments("/api/auth/token/refresh"))
            {
            
                context.Options.TokenValidationParameters.ValidateLifetime = false;
            }
            return Task.CompletedTask;
        },

      
        OnChallenge = context =>
        {
          
            context.Options.TokenValidationParameters.ValidateLifetime = true;
            return Task.CompletedTask;
        }
    };
});



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireEmployeeRole", policy => policy.RequireRole("Employee", "StoreRep"));
    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
});

builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("VueCorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        var allowedOrigins = string.Join(",", corsAllowedOrigins);
        context.Response.Headers.Add("Access-Control-Allow-Origin", allowedOrigins);
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.StatusCode = 200;
        return;
    }
    await next();
});

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    
    var roles = new[] { "Admin", "Employee", "Customer", "StoreRep" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

   
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var admin = new User
        {
            Email = adminEmail,
            UserName = adminEmail,
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}

app.Run();