

/*program.cs se encarga de: punto de entrada de la api + iniciar + conectar + inyectar + arrancar*/
using PERPETUUM.Repositories; //para añadir a adscoped
using PERPETUUM.Services; //para añadir a addscoped using System;
using MySqlConnector;
using Serilog;
//JWT::: dependencias para 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

/*genera automáticamente el static void Main(string[] args) por detrás. El args se pasa a CreateBuilder(args) para que la aplicación pueda recibir parámetros de línea de comandos.*/

//configuro inicio de aplicación
var builder = WebApplication.CreateBuilder(args);



// SERILOG:::
// Definimos que queremos escribir en consola y en un archivo
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Para ver todo en la consola negra
    .WriteTo.File("Logs.log", 
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error) // sólo escribe si es ERROR o CRITICAL
    .CreateLogger();

// Le decimos al Host que use Serilog en lugar del logger por defecto
builder.Host.UseSerilog(); 


//añadir servicios de controlador
builder.Services.AddControllers();

//JWT::: le decimos a la palicacion que parametros validaremos en el jwt. El builder.configuration los cogera del appsettings.json //TODO: si cambia ubicacion / puertos de fornt, back cambiar acorde appsettings
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
           IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
       };
   });




//DEPENDENCIAS DEL PROYECTO:::
//deceased 
builder.Services.AddScoped<IDeceasedRepository, DeceasedRepository>();
builder.Services.AddScoped<IDeceasedService, DeceasedService>();

//FuneralHome 
builder.Services.AddScoped<IFuneralHomeRepository, FuneralHomeRepository>();
builder.Services.AddScoped<IFuneralHomeService, FuneralHomeService>();

//MemorialGuardian 

//Memory
builder.Services.AddScoped<IMemoryRepository, MemoryRepository>();
builder.Services.AddScoped<IMemoryService, MemoryService>();

//Staff 
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();

//User
builder.Services.AddScoped<IUserRepository, UserRepository>();

//Auth
builder.Services.AddScoped<IAuthService, AuthService>();



//SWAGGER:::añado swagger siembre
//JWT::: configuración en swagger. para que añada boton de autorizar y nos devuelva el token. Se guarda en memoria
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt => //le dice a swagger que usamos jwt con estandard beared y que se deben enviar en al cabecera. Swagger muestra boton authorize
{
   opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
   opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
   {
       In = ParameterLocation.Header,
       Description = "Please enter token",
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
                   Type=ReferenceType.SecurityScheme,
                   Id="Bearer"
               }
           },
           new string[]{}
       }
   });
});


// construye app con la configuración creada.
var app = builder.Build();  

//SWAGGER:::
if (app.Environment.IsDevelopment()) { 
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
    }


//añadir rutas a los controllers
app.MapControllers();

//JWT:::
app.UseAuthentication();
app.UseAuthorization();

//pone en marcha nuestra app a partir de nuestra config y empieza a escuchar peticiones
app.Run();      

