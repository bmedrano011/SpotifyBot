namespace SpotifyPlaylistGenerator;
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private IConfiguration _config;

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    //https://github.com/JohnnyCrazy/SpotifyAPI-NET/tree/master/SpotifyAPI.Web.Auth
    //https://johnnycrazy.github.io/SpotifyAPI-NET/docs/getting_started
    //https://developer.spotify.com/console/get-current-user/
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var config = SpotifyClientConfig.CreateDefault();
            var request = new ClientCredentialsRequest(_config["ClientID"], _config["ClientSecret"]);
            var response = await new OAuthClient(config).RequestToken(request);

            var spotify = new SpotifyClient(config.WithToken(response.AccessToken));

            var user = await spotify.UserProfile.Current();

            Console.WriteLine(user.DisplayName);
        }
    }
}
