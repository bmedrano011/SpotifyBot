namespace SpotifyPlaylistGenerator.Spotify;

public class SpotifyClientBuilder
{
    HttpClient HttpClient { get; }
    private readonly SpotifyClientConfig _spotifyClientConfig;

    public SpotifyClientBuilder(HttpClient client, SpotifyClientConfig spotifyClientConfig)
    {
        HttpClient = client;
        HttpClient.BaseAddress = new Uri("https://api.spotify.com/api");
        _spotifyClientConfig = spotifyClientConfig;
    }

    // public async Task<SpotifyClient> BuildClient()
    // {
    //     var token = await HttpClient.GetTokenAsync("Spotify", "access_token");

    //     return new SpotifyClient(_spotifyClientConfig.WithToken(token));
    // }
    public async Task<HttpResponseMessage> GetToken()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/token");
        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return response;
    }
    // public async Task<SpotifyClient> BuildClient()
    // {
    //     return new SpotifyClient(_spotifyClientConfig.WithToken(token));
    // }
}

