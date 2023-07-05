using Microsoft.EntityFrameworkCore;
using StreamberryAPI.Data;
using StreamberryAPI.Models;

namespace StreamberryAPI.Domain
{
    public class FilmeService
    {
        private readonly AppDbContext _dbContext;

        public FilmeService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IEnumerable<FilmeModel> GetAllFilmes()
        {
            return _dbContext.Filme
                .Include(filme => filme.Genero)
                .Include(filme => filme.Avaliacoes)
                .Include(filme => filme.Streamings)
                .Include(filme => filme.Comentarios)
                .ToList();
        }

        public IEnumerable<FilmeStreamingModel> GetFilmeStreamings()
        {
            return _dbContext.FilmeStreaming
                .Include(fs => fs.Filme)
                .Include(fs => fs.Streaming)
                .ToList();
        }

        public FilmeModel GetFilmeById(int id)
        {
            return _dbContext.Filme.FirstOrDefault(filme => filme.ID == id);
        }

        public FilmeModel GetFilmeByNome(string nome)
        {
            return _dbContext.Filme
                .Include(filme => filme.Genero)
                .Include(filme => filme.Avaliacoes)
                .Include(filme => filme.Comentarios)
                .FirstOrDefault(filme => filme.Titulo == nome);
        }

        public void AddFilme(FilmeModel filme, List<StreamingModel> streamings)
        {
            var filmeExistente = _dbContext.Filme.FirstOrDefault(f => f.Titulo == filme.Titulo && f.AnoLancamento == filme.AnoLancamento);

            if (filmeExistente != null)
            {
                filme = filmeExistente;
            }
            else
            {
                var generoExistente = _dbContext.Genero.FirstOrDefault(g => g.Nome == filme.Genero.Nome);

                if (generoExistente != null)
                {
                    filme.GeneroId = generoExistente.ID;
                    filme.Genero = generoExistente;
                }
                else
                {
                    var novoGenero = new GeneroModel
                    {
                        Nome = filme.Genero?.Nome
                    };

                    _dbContext.Genero.Add(novoGenero);
                    _dbContext.SaveChanges();

                    filme.GeneroId = novoGenero.ID;
                    filme.Genero = novoGenero;
                }

                // Adiciona o filme ao contexto
                _dbContext.Filme.Add(filme);
                _dbContext.SaveChanges(); // Salva o filme com o ID corretamente atualizado
            }

            foreach (var streaming in streamings)
            {
                var novoFilmeStreaming = new FilmeStreamingModel
                {
                    FilmeId = filme.ID,
                    StreamingId = streaming.ID,   
                };

                _dbContext.FilmeStreaming.Add(novoFilmeStreaming);
            }

            _dbContext.SaveChanges();
        }



        public void AdicionarStreaming(StreamingModel streaming)
        {
            _dbContext.Streaming.Add(streaming);
            _dbContext.SaveChanges();
        }

        public List<StreamingModel> ObterStreamings()
        {
            return _dbContext.Streaming.ToList();
        }

        public List<GeneroModel> ObterGeneros()
        {
            return _dbContext.Genero.ToList();
        }

        public void UpdateFilme(string nome, FilmeModel filme)
        {
            FilmeModel existingFilme = _dbContext.Filme.Include(f => f.FilmeStreamings).Include(g => g.Genero).FirstOrDefault(f => f.Titulo == nome);

            if (existingFilme != null)
            {
                existingFilme.Titulo = filme.Titulo;
                existingFilme.MesLancamento = filme.MesLancamento;
                existingFilme.AnoLancamento = filme.AnoLancamento;

                // Verificar se o gênero existe na tabela 'genero'
                var genero = _dbContext.Genero.FirstOrDefault(g => g.Nome == filme.Genero.Nome);
                if (genero != null)
                {
                    existingFilme.Genero = genero;  // Atualizar o objeto Genero
                }

                // Remover os streamings trocados dos registros existentes de FilmeStreaming
                var existingStreamings = existingFilme.FilmeStreamings.ToList();
                var streamingsToRemove = existingStreamings.Where(fs => !filme.FilmeStreamings.Any(ns => ns.Streaming != null && ns.Streaming.ID == fs.Streaming?.ID)).ToList();

                foreach (var streamingToRemove in streamingsToRemove)
                {
                    existingFilme.FilmeStreamings.Remove(streamingToRemove);
                }

                // Adicionar novos registros de FilmeStreaming com os streamings escolhidos
                foreach (var streaming in filme.FilmeStreamings)
                {
                    var existingStreaming = existingFilme.FilmeStreamings.FirstOrDefault(fs => fs.StreamingId == streaming.Streaming.ID);

                    if (existingStreaming == null)
                    {
                        var existingStreamingDb = _dbContext.Streaming.FirstOrDefault(s => s.ID == streaming.Streaming.ID);
                        if (existingStreamingDb != null)
                        {
                            FilmeStreamingModel novoFilmeStreaming = new FilmeStreamingModel
                            {
                                FilmeId = existingFilme.ID,
                                StreamingId = existingStreamingDb.ID
                            };

                            existingFilme.FilmeStreamings.Add(novoFilmeStreaming);
                        }
                    }
                }

                _dbContext.SaveChanges();
            }
        }

        public void DeleteFilme(string nome)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    FilmeModel existingFilme = _dbContext.Filme
                        .Include(f => f.Comentarios)
                        .Include(f => f.Streamings)
                        .FirstOrDefault(f => f.Titulo == nome);

                    if (existingFilme != null)
                    {
                        // Excluir registros relacionados na tabela "filmestreaming"
                        var filmestreamings = _dbContext.FilmeStreaming
                            .Where(fs => fs.FilmeId == existingFilme.ID)
                            .ToList();

                        foreach (var filmestreaming in filmestreamings)
                        {
                            _dbContext.FilmeStreaming.Remove(filmestreaming);
                        }

                        // Verificar se existem comentários associados ao filme
                        if (existingFilme.Comentarios != null && existingFilme.Comentarios.Count > 0)
                        {
                            // Excluir os comentários associados ao filme
                            _dbContext.Comentario.RemoveRange(existingFilme.Comentarios);
                        }

                        _dbContext.Filme.Remove(existingFilme);
                        _dbContext.SaveChanges();

                        // Confirmar a transação
                        transaction.Commit();
                    }
                }
                catch (Exception)
                {
                    // Ocorreu um erro, desfazer a transação
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<int> AdicionarAvaliacao(int filmeId, AvaliacaoModel avaliacao)
        {
            try
            {
                var filme = await _dbContext.Filme.FindAsync(filmeId);

                if (filme == null)
                {
                    throw new Exception("Filme não encontrado. Verifique o ID informado.");
                }

                avaliacao.FilmeID = filmeId;

                _dbContext.Avaliacao.Add(avaliacao);
                await _dbContext.SaveChangesAsync();

                return avaliacao.ID;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao adicionar a avaliação: " + ex.Message);
            }
        }

        public async Task<bool> AdicionarComentario(ComentarioModel comentario)
        {
            // Verificar se o comentário é válido
            if (string.IsNullOrEmpty(comentario.Comentario))
                throw new ArgumentException("O comentário é obrigatório.");

            try
            {
                _dbContext.Comentario.Add(comentario);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Lidar com erros
                Console.WriteLine("Erro ao salvar o comentário: " + ex.Message);
                return false;
            }
        }

        public List<ComentarioModel> ObterComentarios()
        {
            try
            {
                List<ComentarioModel> comentariosFilmes = _dbContext.Comentario
                    .Include(c => c.Filme)
                    .Select(c => new ComentarioModel
                    {
                        Comentario = c.Comentario,
                        FilmeID = c.FilmeID,
                        FilmeNome = c.Filme.Titulo,
                        Filme = c.Filme
                    })
                    .ToList();

                return comentariosFilmes;
            }
            catch (Exception ex)
            {
                // Lidar com erros
                throw new Exception("Erro ao obter os comentários: " + ex.Message);
            }
        }

        public List<MediaAvaliacaoGeneroModel> ObterAvaliacoesMediasPorGenero()
        {
            var filmes = _dbContext.Filme
                .Include(filme => filme.Genero)
                .Include(filme => filme.Avaliacoes)
                .ToList();

            var mediasAvaliacoesPorGenero = filmes
             .GroupBy(filme =>
             {
                 int anoLancamento = filme.AnoLancamento;
                 int epoca = (anoLancamento - 1) / 10 * 10; // Agrupa por décadas
                 return new { filme.Genero?.Nome, EpocaLancamento = $"{epoca}-{epoca + 9}" };
             })
             .Select(grupo =>
             {
                 decimal media = grupo.SelectMany(filme => filme.Avaliacoes).Any() ? grupo.SelectMany(filme => filme.Avaliacoes).Average(avaliacao => avaliacao.Classificacao) : 0;
                 return new MediaAvaliacaoGeneroModel
                 {
                     Genero = grupo.Key.Nome,
                     EpocaLancamento = grupo.Key.EpocaLancamento,
                     MediaAvaliacao = media
                 };
             })
             .ToList();

            return mediasAvaliacoesPorGenero;
        }

    }
}
