using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class Perfil
    {
        [Key]
        public int ID { get; set; }
        public string nombre { get; set; }
        public virtual ICollection<Usuario> ?listaUsuarios { get; set; }
    }
}
