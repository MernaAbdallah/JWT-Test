using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using JWT.Models;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Configure API Versioning
// -----------------------------

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = ApiVersionReader.Combine(
        // new QueryStringApiVersionReader("api-version"), // Version as a query parameter 
        new HeaderApiVersionReader("api-version")      // Version in header
    );
});


// -----------------------------
// Configure Database Context
// -----------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// -----------------------------
// Configure JWT Authentication
// -----------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        //differnce in time between server amd and client 
        ClockSkew = TimeSpan.Zero,
    };
});

// -----------------------------
// Add Controllers
// -----------------------------
builder.Services.AddControllers();

// -----------------------------
// Configure API Version Explorer
// -----------------------------
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Format for versioned API groups 1.0 >> 1
    //options.SubstituteApiVersionInUrl = true; // Use the version in the URL
});

// -----------------------------
// Configure Swagger
// -----------------------------
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter your JWT Access Token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

    // Include versioned endpoints in Swagger
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "webAPI V1", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "webAPI V2", Version = "v2" });

});

// -----------------------------
// Build Application
// -----------------------------
var app = builder.Build();

// -----------------------------
// Configure Middleware
// -----------------------------
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "webAPI V1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "webAPI V2");
});

app.UseAuthentication();
app.UseAuthorization();

// -----------------------------
// Map Controllers
// -----------------------------
app.MapControllers();

// -----------------------------
// Run the Application
// -----------------------------
app.Run();
