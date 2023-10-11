using Login.Herramientas;
using Login.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Login.Controllers
{
    public class LoginController : Controller
    {
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private readonly Context _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public LoginController(Context context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;

        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public IActionResult Login()
        {
            return View();
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public IActionResult Sesion()
        {
            return View();
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        hUsuario hUsuario = new hUsuario();

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public IActionResult Registro()
        {
            ViewData["listaPerfiles"] = new SelectList(_context.Perfiles, "ID", "nombre");
            return View();
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public IActionResult restablecer()
        {
            return View();
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        public IActionResult Recuperar()
        {
            return View();
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(usuarioDTO usuarioDTO)
        {
            var password = hUsuario.encriptarSHA256(usuarioDTO.password);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.correo == usuarioDTO.correo && u.password == password);

            if (usuario != null)
            {
                if (usuario.validado == false)
                {
                    ViewBag.Confirmado = "Su correo esta en espera de validacion ";
                    return View();
                }
                if (usuario.restablecer)
                {
                    ViewBag.Confirmado = "Ha solicitado la recuperacion de su cuenta, favor de revisar la bandeja de entrada de su correo";
                    return View();
                }
                return RedirectToAction("Sesion");
            }
            ViewBag.Validacion = "Las credenciales no coinciden";
            return View();
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(usuarioDTO nuevoUsuario)
        {
            if (nuevoUsuario.password != nuevoUsuario.confirmarPassword || nuevoUsuario.perfilId == 0)
            {
                ViewBag.mensajeperfil = nuevoUsuario.perfilId == 0 ? "Seleccione un perfil para el registro" : "";
                ViewBag.mensajePassword = nuevoUsuario.password != nuevoUsuario.confirmarPassword ? "Las contraseñas tienen que ser iguales" : "";

                ViewData["listaPerfiles"] = new SelectList(_context.Perfiles, "ID", "nombre");
                return View();
            }

            var usuario = _context.Usuarios.FirstOrDefault(c => c.correo == nuevoUsuario.correo);

            if (usuario == null)
            {
                var usuarioNuevo = hUsuario.usuarioNuevo(nuevoUsuario);
                var destinatario = "carlosivan12.ci2@gmail.com";
                var plantilla = "Confirmar.html";
                var ruta = $"Login/confirmar?token={usuarioNuevo.token}";

                var correo = hUsuario.crearPlantilla(_hostingEnvironment, HttpContext, usuarioNuevo, destinatario, plantilla, ruta);
                bool enviado = hUsuario.enviarCorreo(correo);

                if (enviado)
                {
                    ViewBag.Creado = true;
                    ViewBag.Mensaje = $"Su cuenta esta en espera de aceptacion, favor de esperar el correo de validacion";
                    _context.Usuarios.Add(usuarioNuevo);
                    await _context.SaveChangesAsync();
                    return View();
                }
                ViewBag.Creado = true;
                ViewBag.Mensaje = $"No se pudo completar el registro, intentelo de nuevo";
                return View();
            }
            ViewBag.Creado = false;
            ViewBag.Mensaje = "El correo ya se encuentra registrado";
            return View();
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public async Task<IActionResult> confirmar(string token)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.token == token);
            if (usuario != null && usuario.validado == false)
            {
                var destinatario = usuario.correo;
                var plantilla = "Respuesta.html";
                var ruta = "Login/Login";
                var correo = hUsuario.crearPlantilla(_hostingEnvironment, HttpContext, usuario, destinatario, plantilla, ruta);
                bool enviado = hUsuario.enviarCorreo(correo);

                usuario.validado = true;

                if (enviado)
                {
                    await _context.SaveChangesAsync();
                    ViewBag.Validado = "El usuario ha sido validado";
                    return View();
                }
                ViewBag.Validado = "No se pudo validar el usuario, intentelo de nuevo";
                return View();
            }
            ViewBag.Validado = "El correo del usuario ya esta dado de alta";
            return View();
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> restablecer(usuarioDTO usuario)
        {
            var recuperarUsuario = await _context.Usuarios.FirstOrDefaultAsync(c => c.correo == usuario.correo);
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
                        ViewBag.Restablecer = "Restablecer cuenta, favor de revisar la bandeja de entrada de su correo para seguir con el proceso";
                        return View();
                    }
                    ViewBag.Restablecer = "No se pudo restablecer la cuenta, intentelo de nuevo";
                    return View();
                }
                ViewBag.Restablecer = "Ya ha solicitado el restablecimiento de su cuenta, favor de revisar la bandeja de entrada de su correo";
                return View();
            }
            ViewBag.Fallo = "No hay ningun ususario asociado a esa cuenta";
            return View();
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recuperar(string token, usuarioDTO usuario)
        {
            var recuperarUsuario = await _context.Usuarios.FirstOrDefaultAsync(r => r.token == token);
            if (recuperarUsuario != null)
            {
                if (recuperarUsuario.restablecer == false)
                {
                    ViewBag.Restablecido = "La cuenta ya sido restablecida";
                    return View();
                }
                if (usuario.password != usuario.confirmarPassword)
                {
                    ViewBag.Password = "Las contraseñas no coinciden";
                    return View();
                }

                recuperarUsuario.password = hUsuario.encriptarSHA256(usuario.password);
                recuperarUsuario.restablecer = false;

                await _context.SaveChangesAsync();
                ViewBag.Restablecido = "La cuenta ha sido restablecida correctamente";
                return View();
            }
            ViewBag.NoRestablecido = "Usuario no encontrado";
            return View();
        }
    }
}