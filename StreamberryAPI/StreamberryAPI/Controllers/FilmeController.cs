using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StreamberryAPI.Domain;
using StreamberryAPI.Models;

namespace StreamberryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilmeController : ControllerBase
    {
        private readonly FilmeService _filmeService;

        public FilmeController(FilmeService filmeService)
        {
            _filmeService = filmeService;
        }

        // GET: api/Filme
        [HttpGet]
        public IEnumerable<FilmeModel> Get()
        {
            return _filmeService.GetAllFilmes();
        }

        // GET api/Filme/5
        [HttpGet("{id}")]
        public ActionResult<FilmeModel> Get(int id)
        {
            var filme = _filmeService.GetFilmeById(id);
            if (filme == null)
            {
                return NotFound();
            }
            return filme;
        }

        // GET api/Filme/5
        [HttpGet("{nome}/GetPorNome")]
        public ActionResult<FilmeModel> Get(string nome)
        {
            var filme = _filmeService.GetFilmeByNome(nome);
            if (filme == null)
            {
                return NotFound();
            }
            return filme;
        }

        // POST api/Filme
        [HttpPost("Adicionar")]
        public ActionResult<List<FilmeModel>> AdicionarFilme([FromBody] List<FilmeStreamingModel> filmesComStreamings)
        {
            try
            {
                foreach (var filmeStreaming in filmesComStreamings)
                {
                    var filme = filmeStreaming.Filme;
                    var streaming = filmeStreaming.Streaming;

                    _filmeService.AddFilme(filme, new List<StreamingModel> { streaming });
                }

                var idsFilmes = filmesComStreamings.Select(f => f.Filme.ID).ToList();
                var filmes = filmesComStreamings.Select(f => f.Filme).ToList();

                return CreatedAtAction(nameof(Get), new { ids = idsFilmes }, filmes);
            }
            catch (Exception e)
            {
                return BadRequest("Erro ao adicionar os filmes: " + e.Message);
            }
        }

        // PUT api/Filme/5
        [HttpPut("{nome}")]
        public IActionResult Put(string nome, [FromBody] FilmeModel filme)
        {
            _filmeService.UpdateFilme(nome, filme);
            return NoContent();
        }

        // DELETE api/Filme/5
        [HttpDelete("{nome}")]
        public IActionResult Delete(string nome)
        {
            _filmeService.DeleteFilme(nome);
            return NoContent();
        }

        [HttpPost("{filmeId}/Avaliacao")]
        public async Task<IActionResult> AdicionarAvaliacao(int filmeId, AvaliacaoModel avaliacao)
        {
            try
            {
                int avaliacaoId = await _filmeService.AdicionarAvaliacao(filmeId, avaliacao);
                return Ok(avaliacaoId);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Erro ao adicionar a avaliação: {e.Message}");
            }
        }


        [HttpPost]
        public ActionResult<StreamingModel> AdicionarStreaming([FromBody] StreamingModel streaming)
        {
            _filmeService.AdicionarStreaming(streaming);
            return CreatedAtAction(nameof(Get), new { id = streaming.ID }, streaming);
        }

        [HttpGet("ObterStreamings")]
        public ActionResult<List<StreamingModel>> ObterStreamings()
        {
            var streamings = _filmeService.ObterStreamings();
            return Ok(streamings);
        }

        [HttpGet("ObterGeneros")]
        public ActionResult<List<GeneroModel>> ObterGeneros()
        {
            var streamings = _filmeService.ObterGeneros();
            return Ok(streamings);
        }

        [HttpGet("FilmeStreaming")]
        public ActionResult<IEnumerable<FilmeStreamingModel>> GetFilmeStreamings()
        {
            var filmeStreamings = _filmeService.GetFilmeStreamings();
            return Ok(filmeStreamings);
        }

        [HttpPost("Comentario")]
        public async Task<IActionResult> AdicionarComentario(ComentarioModel comentario)
        {
            try
            {
                await _filmeService.AdicionarComentario(comentario);
                return Ok("Comentário adicionado com sucesso!");
            }
            catch (Exception ex)
            {
                // Lidar com erros
                return BadRequest("Erro ao adicionar o comentário: " + ex.Message);
            }
        }

        [HttpGet("GetComentariosPorFilme")]
        public IActionResult VisualizarComentarios()
        {
            try
            {
                List<ComentarioModel> comentarios =  _filmeService.ObterComentarios();
                return Ok(comentarios);
            }
            catch (Exception ex)
            {
                // Lidar com erros
                return BadRequest("Erro ao obter os comentários: " + ex.Message);
            }
        }

        [HttpGet("AvaliacoesMediasPorGenero")]
        public ActionResult<IEnumerable<MediaAvaliacaoGeneroModel>> GetAvaliacoesMediasPorGenero()
        {
            var mediasAvaliacoesPorGenero = _filmeService.ObterAvaliacoesMediasPorGenero();
            return Ok(mediasAvaliacoesPorGenero);
        }
    }
}
