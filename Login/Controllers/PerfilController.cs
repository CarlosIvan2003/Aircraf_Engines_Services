using Login.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Login.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PerfilController : ControllerBase
    {
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private readonly Context _context;

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public PerfilController(Context context)
        {
            _context = context;
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        [HttpPost]
        [Route("POST")]
        public async Task<ActionResult> agregarPerfil ([FromBody] Perfil nuevoPerfil)
        {
            if (nuevoPerfil == null)
            {
                return BadRequest();
            }

            _context.Perfiles.Add(nuevoPerfil);
            await _context.SaveChangesAsync();
            return Ok("Agregado");

        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        [HttpGet]
        [Route("GETS")]
        public async Task<ActionResult<IEnumerable<Perfil>>> listaPerfiles()
        {
            IEnumerable<Perfil> perfiles = await _context.Perfiles.ToListAsync(); 
            return Ok(perfiles);
        }
    }
}
