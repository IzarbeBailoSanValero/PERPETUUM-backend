

/*program.cs se encarga de: punto de entrada de la api + iniciar + conectar + inyectar + arrancar*/
using PERPETUUM.Repositories; //para añadir a adscoped
using PERPETUUM.Services; //para añadir a addscoped using System;
using MySqlConnector;
using Serilog;

/*genera automáticamente el static void Main(string[] args) por detrás. El args se pasa a CreateBuilder(args) para que la aplicación pueda recibir parámetros de línea de comandos.*/

//configuro inicio de aplicación
var builder = WebApplication.CreateBuilder(args);



//  Configuración de SERILOG
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



/////////////////inyección de dependencias///////////////////////
//deceased 
builder.Services.AddScoped<IDeceasedRepository, DeceasedRepository>();
builder.Services.AddScoped<IDeceasedService, DeceasedService>();
//FuneralHome 
builder.Services.AddScoped<IFuneralHomeRepository, FuneralHomeRepository>();
builder.Services.AddScoped<IFuneralHomeService, FuneralHomeService>();
//MemorialGuardian 
//Memory 
//Staff 
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();
//User




//añado swagger siembre
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// construye app con la configuración creada.
var app = builder.Build();  

/// Activar Swagger solo en desarrollo 
if (app.Environment.IsDevelopment()) { 
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
    }


//añadir rutas a los controllers
app.MapControllers();

//pone en marcha nuestra app a partir de nuestra config y empieza a escuchar peticiones
app.Run();      

