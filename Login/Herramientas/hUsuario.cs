using Login.Models;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;

using System;
using System.IO;
using Microsoft.Extensions.Hosting.Internal;

namespace Login.Herramientas
{
    public class hUsuario
    {
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private string _host = "smtp.gmail.com";
        private int _puerto = 587;

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private string _nombre = "Nuevo usuario";
        private string _remitente = "pruebacorreo172003@gmail.com";
        private string _clave = "xvodomprdlhuwplg";

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public string encriptarSHA256(string password)
        {
            string hash = string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] valor = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                foreach (byte b in valor)
                {
                    hash += $"{b:x2}";
                }
            }
            return hash;
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public string generarToken()
        {
            string token = Guid.NewGuid().ToString("N");
            return token;
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public bool enviarCorreo(correoDTO correo)
        {
            try
            {
                var email = new MimeMessage();

                email.From.Add(new MailboxAddress(_nombre, _remitente));
                email.To.Add(MailboxAddress.Parse(correo.para));
                email.Subject = correo.asunto;
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = correo.contenido
                };

                var smtp = new SmtpClient();
                smtp.Connect(_host, _puerto, SecureSocketOptions.StartTls);

                smtp.Authenticate(_remitente, _clave);
                smtp.Send(email);
                smtp.Disconnect(true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public correoDTO crearPlantilla(IWebHostEnvironment hostingEnvironment, HttpContext HttpContext, Usuario usuario, string destino, string plantilla, string ruta)
        {
            string folder = "Plantillas";

            // Utiliza el hostingEnvironment para resolver la ruta física
            string path = Path.Combine(hostingEnvironment.WebRootPath, folder, plantilla);

            string scheme = HttpContext.Request.Scheme;
            string host = HttpContext.Request.Host.ToString();
            string url = $"{scheme}://{host}/{ruta}";


            StreamReader reader = new StreamReader(path);
            string htmlBody = string.Format(reader.ReadToEnd(), url, usuario.nombre);


            correoDTO correoDTO = new correoDTO()
            {
                para = destino,
                asunto = "Confirmación de cuenta",
                contenido = htmlBody
            };

            return correoDTO;
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public Usuario usuarioNuevo(usuarioDTO nuevoUsuario)
        {
            var usuarioNuevo = new Usuario()
            {
                perfilID = nuevoUsuario.perfilId,
                nombre = nuevoUsuario.nombre,
                apellidoPaterno = nuevoUsuario.apellidoPaterno,
                apellidoMaterno = nuevoUsuario.apellidoMaterno,
                correo = nuevoUsuario.correo,
                password = encriptarSHA256(nuevoUsuario.password),
                restablecer = false,
                validado = false,
                token = generarToken(),

            };

            return usuarioNuevo;
        }

    }
}
