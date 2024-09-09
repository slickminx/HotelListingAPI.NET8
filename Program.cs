using HotelListing.Configurations;
using HotelListing.Contracts;
using HotelListing.Data;
using HotelListing.Data.HotelListing.Data;
using HotelListing.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

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
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", b => b.AllowAnyHeader()
                                       .AllowAnyOrigin()
                                       .AllowAnyMethod());
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
