using StreamberryAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleStreamberry.Utils
{
    public class FilmeServiceHelper
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly string apiUrl = "https://localhost:7250";

        public static async Task<List<StreamingModel>> ObterStreamings()
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{apiUrl}/api/Filme/ObterStreamings");

            if (response.IsSuccessStatusCode)
            {
                var streamings = await response.Content.ReadFromJsonAsync<List<StreamingModel>>();
                return streamings;
            }
            else
            {
                Console.WriteLine("Erro ao obter a lista de streamings. Código de status HTTP: " + response.StatusCode);
                return null;
            }
        }

        public static async Task<List<GeneroModel>> ObterGeneros()
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{apiUrl}/api/Filme/ObterGeneros");

            if (response.IsSuccessStatusCode)
            {
                var generos = await response.Content.ReadFromJsonAsync<List<GeneroModel>>();
                return generos;
            }
            else
            {
                Console.WriteLine("Erro ao obter a lista de gêneros. Código de status HTTP: " + response.StatusCode);
                return null;
            }
        }
    }
}
