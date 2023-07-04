using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StreamberryAPI.Models
{
    public class FilmeModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string? Titulo { get; set; }
        public int MesLancamento { get; set; }
        public int AnoLancamento { get; set; }
        // Relacionamento com Gênero
        public int GeneroId { get; set; }
        [JsonProperty("genero")]
        public GeneroModel? Genero { get; set; }
        // Relacionamento com Avaliacoes
        public List<AvaliacaoModel>? Avaliacoes { get; set; }
        // Relacionamento com Comentarios
        public List<ComentarioModel>? Comentarios { get; set; }
        // Relacionamento com Streaming
        public List<StreamingModel>? Streamings { get; set; }
        public List<FilmeStreamingModel>? FilmeStreamings { get; set; }

        public decimal CalcularMediaAvaliacoes()
        {
            if (Avaliacoes == null || Avaliacoes.Count == 0)
            {
                return 0;
            }

            decimal soma = 0;
            foreach (var avaliacao in Avaliacoes)
            {
                soma += avaliacao.Classificacao;
            }

            return soma / Avaliacoes.Count;
        }
    }
}
