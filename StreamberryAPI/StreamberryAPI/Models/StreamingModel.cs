namespace StreamberryAPI.Models
{
    public class StreamingModel
    {
        public int ID { get; set; }
        public string? Nome { get; set; }
        public int? FilmeID { get; set; }
        // Relacionamento com Filme
        public FilmeModel? Filme { get; set; }
    }
}
