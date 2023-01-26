using SpotifyAPI.Web.Auth;

namespace SpotifyPlaylistGenerator;
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private IConfiguration _config;
    private static readonly EmbedIOAuthServer _server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
    private const string CredentialsPath = "credentials.json";

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string clientId = _config["ClientID"];
        if (string.IsNullOrEmpty(clientId))
        {
            throw new NullReferenceException(
              "Please set SPOTIFY_CLIENT_ID via environment variables before starting the program"
            );
        }

        if (File.Exists(CredentialsPath))
        {
            await Start(clientId);
        }
        else
        {
            await StartAuthentication(clientId);
        }
    }

    private async Task<AccessToken> GetToken()
    {
        string clientId = _config["ClientID"];
        string clientSecret = _config["ClientSecret"];
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

            FormUrlEncodedContent requestBody = new FormUrlEncodedContent(requestData);

            //Request Token
            var request = await client.PostAsync("https://accounts.spotify.com/api/token", requestBody);
            var response = await request.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AccessToken>(response);
        }
    }

    private async Task Start(string clientId)
    {
        var json = await File.ReadAllTextAsync(CredentialsPath);
        var token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);

        var authenticator = new PKCEAuthenticator(clientId!, token!);
        authenticator.TokenRefreshed += (sender, token) => File.WriteAllText(CredentialsPath, JsonConvert.SerializeObject(token));

        var config = SpotifyClientConfig.CreateDefault()
          .WithAuthenticator(authenticator);

        var spotify = new SpotifyClient(config);

        var me = await spotify.UserProfile.Current();
        Console.WriteLine($"Welcome {me.DisplayName} ({me.Id}), you're authenticated!");

        SpotifyBot spotifyBot = new SpotifyBot(_logger, spotify);
        await spotifyBot.Run();

        _server.Dispose();
        Environment.Exit(0);
    }

    private async Task StartAuthentication(string clientId)
    {
        var (verifier, challenge) = PKCEUtil.GenerateCodes();

        await _server.Start();
        _server.AuthorizationCodeReceived += async (sender, response) =>
        {
            await _server.Stop();
            PKCETokenResponse token = await new OAuthClient().RequestToken(
            new PKCETokenRequest(clientId!, response.Code, _server.BaseUri, verifier)
          );

            await File.WriteAllTextAsync(CredentialsPath, JsonConvert.SerializeObject(token));
            await Start(clientId);
        };

        var request = new LoginRequest(_server.BaseUri, clientId!, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = new List<string> {
                UserReadEmail,
                UserReadPrivate,
                PlaylistReadPrivate,
                PlaylistReadCollaborative,
                PlaylistModifyPrivate,
                PlaylistModifyPublic,
            }
        };

        Uri uri = request.ToUri();
        try
        {
            BrowserUtil.Open(uri);
        }
        catch (Exception)
        {
            Console.WriteLine("Unable to open URL, manually open: {0}", uri);
        }
    }
}



