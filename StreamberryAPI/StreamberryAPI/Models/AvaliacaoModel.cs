namespace StreamberryAPI.Models
{
    public class AvaliacaoModel
    {
        public int ID { get; set; }
        public int FilmeID { get; set; }
        public decimal Classificacao { get; set; }

        // Relacionamento com Filme
        public FilmeModel? Filme { get; set; }
    }
}
