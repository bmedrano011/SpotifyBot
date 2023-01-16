

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
    //https://medium.com/dotvvm/using-net-core-worker-services-in-a-dotvvm-web-application-fd23463b975f

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var config = SpotifyClientConfig.CreateDefault();
            var request = new ClientCredentialsRequest(_config["ClientID"], _config["ClientSecret"]);
            var response = await new OAuthClient(config).RequestToken(request);

            // string token = await GenerateAccessToken();

            // var spotify = new SpotifyClient(config.WithToken(response.AccessToken));
            Console.WriteLine("Spotify API");
            AccessToken token = GetToken().Result;
            //var tokenExtracted = String.Format("Access Token: {0}", token.access_token);

            var spotify = new SpotifyClient(config.WithToken(token.access_token));
            var userProfile = await spotify.UserProfile.Current();

        }
    }

    private async Task<AccessToken> GetToken()
    {
        Console.WriteLine("Getting Token");
        string clientId = "30162b7b6e2648159d45966fd6ed2074";
        string clientSecret = "7ce889a3ce794695a4ec1277b1fd4079";
        string credentials = String.Format("{0}:{1}", clientId, clientSecret);

        using (var client = new HttpClient())
        {
            //Define Headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));

            //Prepare Request Body
            List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>();
            requestData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            // requestData.Add(new KeyValuePair<string, string>("Spotify", "access_token"));

            FormUrlEncodedContent requestBody = new FormUrlEncodedContent(requestData);

            //Request Token
            var request = await client.PostAsync("https://accounts.spotify.com/api/token", requestBody);
            var response = await request.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AccessToken>(response);
        }
    }
}

