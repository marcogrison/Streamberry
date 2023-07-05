using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StreamberryAPI.Models
{
    public class FilmeStreamingModel
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("Filme")]
        public int FilmeId { get; set; }
        public FilmeModel? Filme { get; set; }
        [ForeignKey("Streaming")]
        public int StreamingId { get; set; }
        public StreamingModel? Streaming { get; set; }
    }
}
