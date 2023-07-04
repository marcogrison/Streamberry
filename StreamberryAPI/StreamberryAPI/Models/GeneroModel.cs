namespace StreamberryAPI.Models
{
    public class GeneroModel
    {
        public int ID { get; set; }
        public string? Nome { get; set; }

        // Relacionamento com Filmes
        public List<FilmeModel>? Filmes { get; set; }
    }
}
