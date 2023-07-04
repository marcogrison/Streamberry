using ConsoleStreamberry.Utils;
using Newtonsoft.Json;
using StreamberryAPI.Domain;
using StreamberryAPI.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

public class Program
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly string apiUrl = "https://localhost:7250"; // Alterar para a porta de seu pc

    static void Main()
    {
        bool sair = false;

        while (!sair)
        {
            Console.Clear();
            Console.WriteLine("Desafio Desenvolvedor 2023 - Streamberry");
            Console.WriteLine("=========================================");
            Console.WriteLine("1. Adicionar Filme");
            Console.WriteLine("2. Atualizar Filme");
            Console.WriteLine("3. Listar Filmes");
            Console.WriteLine("4. Deletar Filme");
            Console.WriteLine("5. Pesquisar Filme");
            Console.WriteLine("6. Avaliar Filme e Comentar");
            Console.WriteLine("7. Adicionar Streaming");
            Console.WriteLine("8. Visualizar Comentários");
            Console.WriteLine("9. Avaliações médias filmes por gênero e época de lançamento(década)");
            Console.WriteLine("10. Quantidade streamings que um filme está");
            Console.WriteLine("11. Quantidade de Filmes por Ano");
            Console.WriteLine("0. Sair");
            Console.WriteLine("=========================================");
            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    AdicionarFilme().Wait();
                    break;
                case "2":
                    AtualizarFilme().Wait();
                    break;
                case "3":
                    ListarFilmes().Wait();
                    break;
                case "4":
                    DeletarFilme().Wait();
                    break;
                case "5":
                    PesquisarFilme().Wait();
                    break;
                case "6":
                    AdicionarAvaliacaoComentario().Wait();
                    break;
                case "7":
                    AdicionarStreaming().Wait();
                    break;
                case "8":
                    VisualizarComentarios().Wait();
                    break;
                case "9":
                    ListarAvaliacoesMediasPorGenero().Wait();
                    break;
                case "10":
                    QuantidadeFilmePorStreaming().Wait();
                    break;
                case "11":
                    QuantidadeFilmesPorAno().Wait();
                    break;
                case "0":
                    sair = true;
                    break;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }

            Console.WriteLine("Pressione qualquer tecla para continuar...");
            Console.ReadKey();
        }
    }
    static async Task QuantidadeFilmesPorAno()
    {
        HttpResponseMessage responseFilmes = await httpClient.GetAsync($"{apiUrl}/api/Filme");
        var filmes = await responseFilmes.Content.ReadFromJsonAsync<IEnumerable<FilmeModel>>();

        // Agrupar os filmes por ano de lançamento
        var filmesGroupedByAno = filmes
       .GroupBy(f => f.AnoLancamento)
       .OrderBy(g => g.Key);

        // Exibir a quantidade de filmes e os filmes lançados em cada ano
        foreach (var grupoAno in filmesGroupedByAno)
        {
            var ano = grupoAno.Key;
            int quantidadeFilmes = grupoAno.Count();

            Console.WriteLine($"Ano: {ano} - Quantidade de filmes: {quantidadeFilmes}");

            foreach (var filme in grupoAno)
            {
                Console.WriteLine($"  - {filme.Titulo}");
            }

            Console.WriteLine();
        }
    }

    static async Task QuantidadeFilmePorStreaming()
    {
        HttpResponseMessage responseFilmes = await httpClient.GetAsync($"{apiUrl}/api/Filme");
        var filmes = await responseFilmes.Content.ReadFromJsonAsync<IEnumerable<FilmeModel>>();

        HttpResponseMessage responseFilmeStreamings = await httpClient.GetAsync($"{apiUrl}/api/Filme/FilmeStreaming");

        if (responseFilmeStreamings.IsSuccessStatusCode)
        {
            var filmeStreamings = await responseFilmeStreamings.Content.ReadFromJsonAsync<IEnumerable<FilmeStreamingModel>>();

            // Agrupar as informações dos filmes por filme
            var filmesGroupedByFilme = filmeStreamings
                .GroupBy(x => x.FilmeId);

            // Exibir a quantidade de streamings por filme
            foreach (var grupoFilme in filmesGroupedByFilme)
            {
                var filmeId = grupoFilme.Key;
                int quantidadeStreamings = grupoFilme.Count();

                // Obter o nome do filme
                var filme = filmes.FirstOrDefault(f => f.ID == filmeId);
                var nomeFilme = filme?.Titulo ?? "Filme desconhecido";

                Console.WriteLine($"{nomeFilme}: {quantidadeStreamings} streamings");
            }
        }

    }
    static async Task ListarAvaliacoesMediasPorGenero()
    {
        Console.Clear();
        Console.WriteLine("===== Avaliações Médias por Gênero e Época de Lançamento =====");

        HttpResponseMessage responseMedias = await httpClient.GetAsync($"{apiUrl}/api/Filme/AvaliacoesMediasPorGenero");

        if (responseMedias.IsSuccessStatusCode)
        {
            var mediasAvaliacoesPorGenero = await responseMedias.Content.ReadFromJsonAsync<IEnumerable<MediaAvaliacaoGeneroModel>>();

            foreach (var mediaAvaliacao in mediasAvaliacoesPorGenero)
            {
                Console.WriteLine($"Gênero: {mediaAvaliacao.Genero}, Época de Lançamento: {mediaAvaliacao.EpocaLancamento}, Média de Avaliação: {mediaAvaliacao.MediaAvaliacao.ToString("0.00")}");
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------------------------------------------");
            }
        }
        else
        {
            Console.WriteLine("Erro ao obter as avaliações médias. Código de status HTTP: " + responseMedias.StatusCode);
        }
    }

    static async Task VisualizarComentarios()
    {
        // Obter os comentários por filme
        HttpResponseMessage responseComentarios = await httpClient.GetAsync($"{apiUrl}/api/Filme/GetComentariosPorFilme");

        if (responseComentarios.IsSuccessStatusCode)
        {
            List<ComentarioModel> comentariosFilmes = await responseComentarios.Content.ReadFromJsonAsync<List<ComentarioModel>>();

            Console.WriteLine("===== Comentários =====");

            foreach (var comentarioFilme in comentariosFilmes)
            {
                Console.WriteLine($"Filme: {comentarioFilme.FilmeNome}");
                Console.WriteLine($"Comentário: {comentarioFilme.Comentario}");
                Console.WriteLine("----------------------------------------");
            }
        }
        else
        {
            Console.WriteLine("Erro ao obter os comentários. Código de status HTTP: " + responseComentarios.StatusCode);
        }
    }

    static async Task AdicionarFilme()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Clear();
        Console.WriteLine("===== Adicionar Filme =====");
        Console.InputEncoding = Encoding.GetEncoding("ISO-8859-1");

        // Obter os dados do filme
        Console.Write("Título: ");
        string titulo = Console.ReadLine();

        Console.Write("Mês de Lançamento: ");
        int mesLancamento = int.Parse(Console.ReadLine());

        Console.Write("Ano de Lançamento: ");
        int anoLancamento = int.Parse(Console.ReadLine());

        Console.Write("Gênero: ");
        string generoNome = Console.ReadLine();

        // Obter a lista de streamings disponíveis
        List<StreamingModel> streamings = await FilmeServiceHelper.ObterStreamings();

        Console.WriteLine("Streamings disponíveis:");
        foreach (var streaming in streamings)
        {
            Console.WriteLine($"- {streaming.Nome}");
        }

        Console.Write("Streamings (separados por vírgula): ");
        string streamingNomesInput = Console.ReadLine();

        // Obter os nomes dos streamings como uma lista
        var streamingNomesList = streamingNomesInput.Split(',')
                                                   .Select(s => s.Trim())
                                                   .ToList();

        // Obter os streamings completos com base nos nomes fornecidos
        var streamingsCompletos = streamings.Where(s => streamingNomesList.Contains(s.Nome))
                                            .ToList();

        // Criar o objeto FilmeModel com os dados fornecidos
        var filme = new FilmeModel
        {
            Titulo = titulo,
            MesLancamento = mesLancamento,
            AnoLancamento = anoLancamento,
            Genero = new GeneroModel { Nome = generoNome }
        };

        try
        {
            // Criar a lista de objetos FilmeStreamingModel com o filme e os streamings
            var filmesComStreamings = streamingsCompletos.Select(streaming =>
                new FilmeStreamingModel
                {
                    Filme = filme,
                    Streaming = streaming
                }).ToList();

            // Converter a lista de objetos para JSON
            string jsonFilmesComStreamings = JsonConvert.SerializeObject(filmesComStreamings, Formatting.None);
            HttpContent contentFilmesComStreamings = new StringContent(jsonFilmesComStreamings, Encoding.UTF8, "application/json");

            // Enviar a requisição POST para adicionar os filmes
            HttpResponseMessage response = await httpClient.PostAsync($"{apiUrl}/api/Filme/Adicionar", contentFilmesComStreamings);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Filmes adicionados com sucesso!");
            }
            else
            {
                Console.WriteLine("Erro ao adicionar os filmes. Código de status HTTP: " + response.StatusCode);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Erro ao adicionar os filmes: " + e.Message);
        }
    }

    static async Task AdicionarStreaming()
    {
        Console.WriteLine("===== Adicionar Streaming =====");
        Console.Write("Nome do Streaming: ");
        string streamingNome = Console.ReadLine();

        // Chamar a API para adicionar o streaming
        var streaming = new StreamingModel { Nome = streamingNome };
        string jsonStreaming = JsonConvert.SerializeObject(streaming, Formatting.None);
        HttpContent content = new StringContent(jsonStreaming, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient.PostAsync($"{apiUrl}/api/Filme", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Streaming adicionado com sucesso!");
        }
        else
        {
            Console.WriteLine("Erro ao adicionar o streaming. Código de status HTTP: " + response.StatusCode);
        }
    }

    static async Task AtualizarFilme()
    {
        Console.Clear();
        Console.WriteLine("===== Atualizar Filme =====");

        // Obter o ID do filme a ser atualizado
        Console.Write("Nome do Filme: ");
        string nome = Console.ReadLine();

        // Verificar se o filme existe
        HttpResponseMessage filmeResponse = await httpClient.GetAsync($"{apiUrl}/api/Filme/{nome}/GetPorNome");

        if (!filmeResponse.IsSuccessStatusCode)
        {
            Console.WriteLine("Filme não encontrado. Verifique o nome informado.");
            return;
        }

        // Obter os novos dados do filme
        Console.Write("Novo Título: ");
        string novoTitulo = Console.ReadLine();

        Console.Write("Novo Mês de Lançamento: ");
        int novoMesLancamento = int.Parse(Console.ReadLine());

        Console.Write("Novo Ano de Lançamento: ");
        int novoAnoLancamento = int.Parse(Console.ReadLine());

        // Obter a lista de gêneros disponíveis
        List<GeneroModel> generos = await FilmeServiceHelper.ObterGeneros();

        // Exibir a lista de gêneros e permitir que o usuário escolha um
        if (generos != null && generos.Any())
        {
            Console.WriteLine("Gêneros disponíveis:");

            for (int i = 0; i < generos.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {generos[i].Nome}");
            }

            Console.Write("Digite o número correspondente ao Gênero desejado: ");
            int escolhaGenero = int.Parse(Console.ReadLine());

            if (escolhaGenero >= 1 && escolhaGenero <= generos.Count)
            {
                GeneroModel generoEscolhido = generos[escolhaGenero - 1];

                // Obter a lista de streamings disponíveis
                List<StreamingModel> streamings = await FilmeServiceHelper.ObterStreamings();

                // Exibir a lista de streamings e permitir que o usuário escolha quantos desejar
                if (streamings != null && streamings.Any())
                {
                    Console.WriteLine("Streamings disponíveis:");

                    for (int i = 0; i < streamings.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {streamings[i].Nome}");
                    }

                    Console.Write("Digite os números correspondentes aos Streamings desejados (separados por vírgula): ");
                    string escolhaStreamingsInput = Console.ReadLine();

                    List<int> escolhaStreamings = escolhaStreamingsInput.Split(',')
                                                                       .Select(int.Parse)
                                                                       .ToList();

                    List<StreamingModel> streamingsSelecionados = new List<StreamingModel>();

                    foreach (int escolha in escolhaStreamings)
                    {
                        if (escolha >= 1 && escolha <= streamings.Count)
                        {
                            StreamingModel streamingSelecionado = streamings[escolha - 1];
                            streamingsSelecionados.Add(streamingSelecionado);
                        }
                    }

                    // Criar o objeto FilmeModel com os novos dados fornecidos
                    var genero = new GeneroModel
                    {
                        ID = generoEscolhido.ID, // Define o ID do gênero escolhido
                        Nome = generoEscolhido.Nome
                    };

                    var filme = new FilmeModel
                    {
                        Titulo = novoTitulo,
                        MesLancamento = novoMesLancamento,
                        AnoLancamento = novoAnoLancamento,
                        Genero = genero,
                        FilmeStreamings = streamingsSelecionados.Select(streaming => new FilmeStreamingModel
                        {
                            Streaming = streaming
                        }).ToList()
                    };

                    HttpResponseMessage response = await httpClient.PutAsJsonAsync($"{apiUrl}/api/Filme/{Uri.EscapeDataString(nome)}", filme);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Filme atualizado com sucesso!");
                    }
                    else
                    {
                        Console.WriteLine("Erro ao atualizar o filme. Código de status HTTP: " + response.StatusCode);
                    }
                }
                else
                {
                    Console.WriteLine("Não há streamings disponíveis.");
                }
            }
            else
            {
                Console.WriteLine("Escolha de gênero inválida.");
            }
        }
        else
        {
            Console.WriteLine("Não há gêneros disponíveis.");
        }
    }

    static async Task ListarFilmes()
    {
        Console.Clear();
        Console.WriteLine("===== Listar Filmes =====");

        // Obter os filmes
        HttpResponseMessage responseFilmes = await httpClient.GetAsync($"{apiUrl}/api/Filme");

        if (responseFilmes.IsSuccessStatusCode)
        {
            var filmes = await responseFilmes.Content.ReadFromJsonAsync<IEnumerable<FilmeModel>>();

            // Obter os registros de FilmeStreaming
            HttpResponseMessage responseFilmeStreamings = await httpClient.GetAsync($"{apiUrl}/api/Filme/FilmeStreaming");

            if (responseFilmeStreamings.IsSuccessStatusCode)
            {
                var filmeStreamings = await responseFilmeStreamings.Content.ReadFromJsonAsync<IEnumerable<FilmeStreamingModel>>();

                // Agrupar as informações dos filmes por streaming
                var filmesGroupedByStreaming = filmes
                    .Join(filmeStreamings,
                        filme => filme.ID,
                        filmeStreaming => filmeStreaming.FilmeId,
                        (filme, filmeStreaming) => new { Filme = filme, FilmeStreaming = filmeStreaming })
                    .GroupBy(x => x.FilmeStreaming?.Streaming?.Nome);

                Console.WriteLine("Filmes:");

                foreach (var grupoStreaming in filmesGroupedByStreaming)
                {
                    var streaming = grupoStreaming.Key;
                    Console.WriteLine($"Streaming: {streaming}");

                    var filmesGroupedByGenero = grupoStreaming
                        .Select(x => x.Filme)
                        .GroupBy(filme => filme.Genero?.Nome);

                    foreach (var grupoGenero in filmesGroupedByGenero)
                    {
                        var genero = grupoGenero.Key;
                        Console.WriteLine($"- Gênero: {genero}");

                        foreach (var filme in grupoGenero)
                        {
                            Console.WriteLine($"  - Título: {filme.Titulo} - Mês/Ano de lançamento: {filme.MesLancamento}/{filme.AnoLancamento}");

                            decimal mediaAvaliacoes = filme.CalcularMediaAvaliacoes();
                            Console.WriteLine($"    - Média das Avaliações: {mediaAvaliacoes}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Erro ao obter os registros de FilmeStreaming. Código de status HTTP: " + responseFilmeStreamings.StatusCode);
            }
        }
        else
        {
            Console.WriteLine("Erro ao obter a lista de filmes. Código de status HTTP: " + responseFilmes.StatusCode);
        }
    }

    static async Task DeletarFilme()
    {
        Console.Clear();
        Console.WriteLine("===== Deletar Filme =====");

        // Obter o ID do filme a ser deletado
        Console.Write("Nome do Filme: ");
        string nome = Console.ReadLine();

        HttpResponseMessage response = await httpClient.DeleteAsync($"{apiUrl}/api/Filme/{nome}");

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Filme deletado com sucesso!");
        }
        else
        {
            Console.WriteLine("Erro ao deletar o filme. Código de status HTTP: " + response.StatusCode);
        }
    }

    static async Task PesquisarFilme()
    {
        Console.Clear();
        Console.WriteLine("===== Pesquisar Filme =====");

        // Obter o título do filme a ser pesquisado
        Console.Write("Título do Filme: ");
        string titulo = Console.ReadLine();

        HttpResponseMessage filmeResponse = await httpClient.GetAsync($"{apiUrl}/api/Filme/{titulo}/GetPorNome");

        if (filmeResponse.IsSuccessStatusCode)
        {
            var filme = await filmeResponse.Content.ReadFromJsonAsync<FilmeModel>();

            if (filme != null)
            {
                Console.WriteLine($"ID: {filme.ID}");
                Console.WriteLine($"Título: {filme.Titulo}");
                Console.WriteLine($"Gênero: {filme.Genero?.Nome}");

                Console.WriteLine("Avaliações:");

                Console.WriteLine($"Nota média: {filme.CalcularMediaAvaliacoes().ToString("0.00")}");

                Console.WriteLine("Comentários:");
                foreach (var comentario in filme.Comentarios)
                {
                    Console.WriteLine($"  - Comentário: {comentario.Comentario}");
                    // Exibir outras informações do comentário, se necessário
                }
            }

            else
                Console.WriteLine("Filme não encontrado.");
        }
        else
        {
            Console.WriteLine("Erro ao pesquisar o filme. Código de status HTTP: " + filmeResponse.StatusCode);
        }
    }

    static async Task AdicionarAvaliacaoComentario()
    {
        Console.Clear();
        Console.WriteLine("===== Adicionar Avaliação =====");

        // Obter o ID do filme para adicionar a avaliação
        Console.Write("Nome do Filme: ");
        string nome = Console.ReadLine();

        // Verificar se o filme existe
        HttpResponseMessage filmeResponse = await httpClient.GetAsync($"{apiUrl}/api/Filme/{nome}/GetPorNome");

        if (!filmeResponse.IsSuccessStatusCode)
        {
            Console.WriteLine("Filme não encontrado. Verifique o nome informado.");
            return;
        }

        // Ler e desserializar a resposta JSON para o modelo FilmeModel
        FilmeModel filme = await filmeResponse.Content.ReadFromJsonAsync<FilmeModel>();

        // Obter os dados da avaliação
        Console.Write("Classificação (1 a 5): ");
        decimal classificacao = decimal.Parse(Console.ReadLine());

        Console.Write("Comentário: ");
        string comentarioTexto = Console.ReadLine();

        // Criar o objeto AvaliacaoModel com os dados fornecidos
        var avaliacao = new AvaliacaoModel
        {
            Classificacao = classificacao,
            FilmeID = filme.ID
        };

        // Salvar a avaliação e obter o ID da avaliação criada
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{apiUrl}/api/Filme/{filme.ID}/Avaliacao", avaliacao);


        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            int avaliacaoId = Convert.ToInt32(responseContent);
            if (avaliacaoId > 0)
            {
                // Criar o objeto ComentarioModel com os dados fornecidos
                var comentario = new ComentarioModel
                {
                    FilmeID = filme.ID,
                    Comentario = comentarioTexto
                };

                // Salvar o comentário
                HttpResponseMessage comentarioResponse = await httpClient.PostAsJsonAsync($"{apiUrl}/api/Filme/Comentario", comentario);

                if (comentarioResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Avaliação e comentário adicionados com sucesso!");
                }
                else
                {
                    Console.WriteLine("Erro ao adicionar o comentário. Código de status HTTP: " + comentarioResponse.StatusCode);
                }

            }
            // Verificar se a resposta contém conteúdo

            else
            {
                Console.WriteLine("Erro ao adicionar a avaliação. A resposta não contém dados válidos.");
            }
        }
        else
        {
            Console.WriteLine("Erro ao adicionar a avaliação. Código de status HTTP: " + response.StatusCode);
        }
    }

}
