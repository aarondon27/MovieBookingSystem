using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace BMS.Services
{
    public class TmdbService
    {
        private readonly string? _apiKey;
        private readonly HttpClient _httpClient;

        public TmdbService(IConfiguration config)
        {
            _apiKey = config["Tmdb:ApiKey"];
            _httpClient = new HttpClient();
        }
        
        public async Task<JObject> GetNowPlayingAsync()
        {
            string url = $"https://api.themoviedb.org/3/movie/now_playing?api_key={_apiKey}&region=IN";
            var response = await _httpClient.GetStringAsync(url);
            return JObject.Parse(response);
        }

        public async Task<JObject> GetMovieDetailsAsync(int tmdbId)
        {
            string url = $"https://api.themoviedb.org/3/movie/{tmdbId}?api_key={_apiKey}&region=IN";

            var response = await _httpClient.GetStringAsync(url);
            return JObject.Parse(response);
        }

        public async Task<JObject> GetMovieReleaseDatesAsync(int movieId)
        {
            string url = $"https://api.themoviedb.org/3/movie/{movieId}/release_dates?api_key={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JObject.Parse(json);
        }

    }
}