using System.ComponentModel.DataAnnotations.Schema;

namespace StreamberryAPI.Models
{
    public class ComentarioModel
    {
        public int ID { get; set; }
        public int FilmeID { get; set; }
        public string? Comentario { get; set; }
        [NotMapped]
        public string? FilmeNome { get; set; }
        // Relacionamento com Filme
        public FilmeModel? Filme { get; set; }
    }
}
