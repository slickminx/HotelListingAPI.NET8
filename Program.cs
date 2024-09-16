using HotelListing.Configurations;
using HotelListing.Contracts;
using HotelListing.Data;
using HotelListing.Middleware;
using HotelListing.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using System.IO;
using System.Text;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(options => {
    options.UseSqlite(connectionString);
});

//IdentityUser is the default user of IdentityCore.
////Comes out of the box with everthing that we need as a user type, includes email, phone number, username, password, encryption. 
//Role represents what the user can do
//Use what data store it should use for authentication
//You can also use an external database.
//Use case: If you had one central database, identity server database,
//that serves multiple applications and this api needs to use this datastore for users, not the data  
builder.Services.AddIdentityCore<ApiUser>()
    .AddRoles<IdentityRole>()
    .AddTokenProvider<DataProtectorTokenProvider<ApiUser>>("HotelListingApi")
    .AddEntityFrameworkStores<HotelListingDbContext>().AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options=>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Hotel Listing API", Version = "v1" });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    { 
      Description = @"JWT Authroization header using the Bearer scheme.  Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef..'",
      Name = "Authorization", 
      In = ParameterLocation.Header,
      Type = SecuritySchemeType.ApiKey, 
      Scheme = JwtBearerDefaults.AuthenticationScheme
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
                },
                Scheme = "0auth2",
                Name = JwtBearerDefaults.AuthenticationScheme, 
                In = ParameterLocation.Header
            }, 
            new List<string>()
        }
    });
});

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", b => b.AllowAnyHeader()
                                       .AllowAnyOrigin()
                                       .AllowAnyMethod());
});

//When you request for the version in Postman or whatever,
//requesting the version via header, keeps the url standardized vs requesting the api version through a query string.
//It's just cleaner to request in the header.

builder.Services.AddApiVersioning(options => 
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1,0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"), 
        new HeaderApiVersionReader("X-Version"), 
        new MediaTypeApiVersionReader("ver")
    );
});

builder.Services.AddVersionedApiExplorer(
    options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    
    });

builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddAutoMapper(typeof(MapperConfig));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IHotelsRespository, HotelsRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

builder.Services.AddAuthentication(options => {
    //Below is an object
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;  //"Bearer"
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        //parameters....
        //Encode a string, a secret key that is used to encode a symetrical key,
        //which is issued along with the token that is called the issuing signing key
        //that means if someone trys to spoof the token, the bad actor cannot replicate the secret key encoded, then effectively replicate a token. 
        //and so generate their own token, the api will reject because of our Bearer token.

        ValidateIssuerSigningKey = true,

        //ValidateIssuer, want to make sure that it came from the api
        ValidateIssuer = true,
        //ValidateAudience, want to make sure that token came from someone we recognized
        ValidateAudience = true,
        //ValidateLifetime the token would be rejected if it wasn't set to lifetime?
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    
        //Storing Keys from appsettings.json, you can put it in the user secrets, right click protect and select "manager user secrets", it will open a secrets.json file to store them there.
    
    };
});

builder.Services.AddResponseCaching(options => 
{
    options.MaximumBodySize = 1024; //1MB
    options.UseCaseSensitivePaths = true;
});

//AspNetCore.HealthChecks.SqlLite
//EF Core - Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
builder.Services.AddHealthChecks()
    .AddCheck<CustomHealthCheck>("Custom Health Check", failureStatus: HealthStatus.Degraded, 
    tags: new[] { "custom" }
    )
    .AddSqlite(connectionString, tags: new[] {"database"})
    .AddDbContextCheck<HotelListingDbContext>(tags: new[] { "database" });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}
app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/healthcheck", new HealthCheckOptions
{ 
    Predicate = healthcheck => healthcheck.Tags.Contains("custom"),
    ResultStatusCodes = 
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
         [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
          [HealthStatus.Degraded] = StatusCodes.Status200OK,
    },
    ResponseWriter = WriteResponse
});

app.MapHealthChecks("/databasehealthcheck", new HealthCheckOptions
{
    Predicate = healthcheck => healthcheck.Tags.Contains("database"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
         [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
          [HealthStatus.Degraded] = StatusCodes.Status200OK,
    },
    ResponseWriter = WriteResponse
});

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
         [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
          [HealthStatus.Degraded] = StatusCodes.Status200OK,
    },
    ResponseWriter = WriteResponse
});

static Task WriteResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var options = new JsonWriterOptions { Indented = true };

    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("status", healthReport.Status.ToString());
        jsonWriter.WriteStartObject("results");

        foreach (var healthReportEntry in healthReport.Entries)
        { 
            jsonWriter.WriteStartObject(healthReportEntry.Key);
            jsonWriter.WriteString("status", healthReportEntry.Value.Status.ToString());
            jsonWriter.WriteString("description", healthReportEntry.Value.Description);
            jsonWriter.WriteStartObject("data");

            foreach (var item in healthReportEntry.Value.Data)
            { 
                jsonWriter.WritePropertyName(item.Key);
                System.Text.Json.JsonSerializer.Serialize(jsonWriter, item.Value, item.Value?.GetType() ?? typeof(object));

            }
            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();

        }
        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndObject();
    }

    return context.Response.WriteAsync(Encoding.UTF8.GetString(memoryStream.ToArray()));
}

app.MapHealthChecks("/health");

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseResponseCaching();

app.Use(async (context, next) => 
{
    context.Response.GetTypedHeaders().CacheControl =
    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
    {
        Public = true, 
        //Every 10 seconds get fresh data
        MaxAge = TimeSpan.FromSeconds(10)
    };

    //the cache response may vary in terms of the type of data it can expect,
    //ex. json, compressed, file
    context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
        new string[] { "Accept-Encoding" };

    await next();
});
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


class CustomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var isHealthy = true;

        /* custom checks. Logic...etc...etc...*/

        if (isHealthy)
        { 
            return Task.FromResult(HealthCheckResult.Healthy("All systems are looking good!"));
        }

        return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, "System unhealthy"));
    }
}