using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Login.Models
{
    public class Usuario
    {
        [Key]
        public int ID { get; set; }
        public int perfilID { get; set; }
        [ForeignKey(nameof(perfilID))]
        public String nombre { get; set; }
        public string apellidoPaterno { get; set; }
        public string apellidoMaterno { get; set; }
        public String correo { get; set; }
        public String password { get; set; }
        public bool restablecer { get; set; }
        public bool validado { get; set; }
        public String token { get; set; }

        public virtual Perfil? perfil {get; set;}
    }

    public class usuarioDTO
    {
        public int ID { get; set; }

        [Display(Name = "Perfil")]
        public int perfilId { get; set; }

        [Display(Name = "Nombre")]
        public String nombre { get; set; }

        [Display(Name = "Apellido Paterno")]
        public string apellidoPaterno { get; set; }

        [Display(Name = "Apellido Materno")]
        public string apellidoMaterno { get; set; }

        [Display(Name = "Correo")]
        [EmailAddress(ErrorMessage = "Ingrese una direccion de correo valida") ]
        public String correo { get; set; }

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public String password { get; set; }

        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        public string confirmarPassword { get; set; }


        /*public bool restablecer { get; set; }
        public bool validado { get; set; }
        public String token { get; set; }*/
    }
}
