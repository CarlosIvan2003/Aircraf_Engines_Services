using Login.Herramientas;
using Login.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Login.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class APILoginController : ControllerBase
    {
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private readonly Context _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        hUsuario hUsuario = new hUsuario();

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public APILoginController(Context context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        [HttpGet]
        [Route("GETS")]
        public async Task<ActionResult<IEnumerable<Usuario>>> listaAlumnos()
        {
            IEnumerable<Usuario> alumno = await _context.Usuarios.ToListAsync();
            return Ok(alumno);
        }

        [HttpPost]
        [Route("SESION")]
        public async Task<ActionResult> Login(LoginDTO login)
        {
            var password = hUsuario.encriptarSHA256(login.Password);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.correo == login.Correo && u.password == password);

            if (usuario != null)
            {
                if (usuario.validado == false)
                {
                    return Ok(new { mensaje = "Su correo esta en espera de validacion" });
                }
                if (usuario.restablecer)
                {
                    return Ok(new { mensaje = "Ha solicitado la recuperacion de su cuenta, favor de revisar la bandeja de entrada de su correo" });
                }
                return Ok(new { mensaje = $"{usuario.perfilID}" });
            }
            return Ok(new { mensaje = "Las credenciales no coinsiden" });
        }


        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        [HttpPost]
        [Route("POST")]
        public async Task<ActionResult> crear([FromBody] usuarioDTO usuario)
        {
            var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(c => c.correo == usuario.correo);
            if (usuarioExistente == null)
            {
                var usuarioNuevo = hUsuario.usuarioNuevo(usuario);
                var destinatario = "carlosivan12.ci2@gmail.com";
                var plantilla = "Confirmar.html";
                var ruta = $"Login/confirmar?token={usuarioNuevo.token}";
                var correo = hUsuario.crearPlantilla(_hostingEnvironment, HttpContext, usuarioNuevo, destinatario, plantilla, ruta);
                bool enviado = hUsuario.enviarCorreo(correo);

                if (enviado)
                {
                    _context.Usuarios.Add(usuarioNuevo);
                    await _context.SaveChangesAsync();
                    return Ok(new { mensaje = "Su cuenta esta en espera de aceptacion, favor de esperar el correo de validacion" });
                }
                return Ok(new { mensaje = "Usuario No Registrado" });
            }
            return Ok(new { mensaje = "Ya existe un usuario con ese correo" });
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        [HttpPost]
        [Route("RESTABLECER")]
        public async Task<ActionResult> restablecer(LoginDTO login)
        {
            var recuperarUsuario = await _context.Usuarios.FirstOrDefaultAsync(c => c.correo == login.Correo);
            if (recuperarUsuario != null && recuperarUsuario.validado == true)
            {
                if (recuperarUsuario.restablecer == false)
                {
                    var destinatario = recuperarUsuario.correo;
                    var plantilla = "Recuperar.html";
                    var ruta = $"Login/Recuperar?token={recuperarUsuario.token}";
                    var correo = hUsuario.crearPlantilla(_hostingEnvironment, HttpContext, recuperarUsuario, destinatario, plantilla, ruta);
                    bool enviado = hUsuario.enviarCorreo(correo);

                    recuperarUsuario.restablecer = true;

                    if (enviado)
                    {
                        await _context.SaveChangesAsync();
                        return Ok(new { mensaje = "Restablecer cuenta, favor de revisar la bandeja de entrada de su correo para seguir con el proceso" });
                    }
                    return Ok(new { mensaje = "No se pudo restablecer la cuenta, intentelo de nuevo" });
                }
                return Ok(new { mensaje = "Ya ha solicitado el restablecimiento de su cuenta, favor de revisar la bandeja de entrada de su correo" });
            }
            return Ok(new { mensaje = "No hay ningun ususario asociado a esa cuenta" });
        }
    }
}
