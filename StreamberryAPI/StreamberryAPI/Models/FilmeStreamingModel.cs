using System.ComponentModel.DataAnnotations;

namespace StreamberryAPI.Models
{
    public class FilmeStreamingModel
    {
        [Key]
        public int Id { get; set; }
        public int FilmeId { get; set; }
        public FilmeModel? Filme { get; set; }
        public int StreamingId { get; set; }
        public StreamingModel? Streaming { get; set; }
    }
}
