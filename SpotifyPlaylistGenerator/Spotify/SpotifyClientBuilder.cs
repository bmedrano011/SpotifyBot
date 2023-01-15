public class SpotifyClientBuilder
{
    private readonly HttpClient _httpClient;

    public SpotifyClientBuilder(HttpClient httpClient) => _httpClient = httpClient;

}