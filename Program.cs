using PERPETUUM.Repositories;
using PERPETUUM.Services;
using Serilog;

// JWT
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Cloudinary (opcional: app arranca sin CLOUDINARY_URL; solo falla subida de fotos)
using dotenv.net;

//seed
using MySqlConnector;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// =========================
// CORS
// =========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =========================
// SERILOG
// =========================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs.log",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
    .CreateLogger();

builder.Host.UseSerilog();

// =========================
// CONTROLLERS
// =========================
builder.Services.AddControllers();

// =========================
// JWT
// =========================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(opt =>
   {
       opt.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidateLifetime = true,
           ValidateIssuerSigningKey = true,
           ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
           ValidAudience = builder.Configuration["JWT:ValidAudience"],
           IssuerSigningKey = new SymmetricSecurityKey(
               System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])
           ),
           ClockSkew = TimeSpan.Zero
       };
   });

// =========================
// CLOUDINARY (CORRECTO)
// =========================
DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

builder.Services.AddSingleton(_ =>
{
    var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
    return new PERPETUUM.Services.CloudinaryWrapper(cloudinaryUrl);
});

// =========================
// DEPENDENCIAS DEL PROYECTO
// =========================

// Deceased
builder.Services.AddScoped<IDeceasedRepository, DeceasedRepository>();
builder.Services.AddScoped<IDeceasedService, DeceasedService>();

// FuneralHome
builder.Services.AddScoped<IFuneralHomeRepository, FuneralHomeRepository>();
builder.Services.AddScoped<IFuneralHomeService, FuneralHomeService>();

// MemorialGuardian
builder.Services.AddScoped<IMemorialGuardianRepository, MemorialGuardianRepository>();
builder.Services.AddScoped<IMemorialGuardianService, MemorialGuardianService>();

// Memory
builder.Services.AddScoped<IMemoryRepository, MemoryRepository>();
builder.Services.AddScoped<IMemoryService, MemoryService>();

// Staff
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();

// User
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Auth
builder.Services.AddScoped<IAuthService, AuthService>();

// =========================
// SWAGGER
// =========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Introduce tu token JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[]{}
        }
    });
});

// =========================
// BUILD APP
// =========================
var app = builder.Build();

// CORS
app.UseCors("PermitirTodo");

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

//seed
var connString = builder.Configuration.GetConnectionString("PerpetuumDB")
               + ";AllowUserVariables=true";
using var conn = new MySqlConnection(connString);
await conn.OpenAsync();

var tablesExist = await conn.ExecuteScalarAsync<int>(
    "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'PerpetuumDB' AND table_name = 'FuneralHome'"
);

if (tablesExist == 0)
{
    var sql = await File.ReadAllTextAsync("perpetuum.sql");
    var command = conn.CreateCommand();
    command.CommandText = sql;
    await command.ExecuteNonQueryAsync();
}

app.Run();




