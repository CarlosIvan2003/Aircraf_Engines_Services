// constructor de la app se encarga de construir la app
using Login.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Agrega servicios al contenedor de inyeccion de independencias agregando vistas y controladores 
builder.Services.AddControllersWithViews();

// Agrega en el contenedor de servicios una configuracion para utilizar sqlserver obteniendo la cadena de conexion
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyContext"))
);

// construye la aplicacion en base a la configuracion previa 
var app = builder.Build();

// verifica en que entorno se ejecuta la app
if (app.Environment.IsProduction())
{
    // Manejador de exepciones
    app.UseExceptionHandler("/Home/Error");
    // Habilita la politica de seguridad de HTTPS, el uso de HSTP (HTTP Strict Transport Security)
    app.UseHsts();
}
else if (app.Environment.IsDevelopment())
{
    
}

// Se configuran los middlewares que son parte de la solicitud HTTP
app.UseHttpsRedirection();
// El manejo de archivos estaticos como CSS JS 
app.UseStaticFiles();
// Configuracion de enrutameinto 
app.UseRouting();
// Autorizacion
app.UseAuthorization();

// Es un mapeo de rutas que redirecciona a un controlador y una vista, si no se especifica nada en la URL 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

// Ejecuta la app
app.Run();
