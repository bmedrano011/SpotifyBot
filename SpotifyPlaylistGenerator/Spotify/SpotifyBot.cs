namespace SpotifyPlaylistGenerator;
public class SpotifyBot
{
    #region Public Variables

    #endregion

    #region Public Constructors
    public SpotifyBot(ILogger<Worker> logger, SpotifyClient spotify)
    {
        _logger = logger;
        _spotify = spotify;
    }

    #endregion

    #region Public Methods
    public async Task Run()
    {
        await BuildArtistList();

    }
    #endregion

    #region Private Variables
    private SpotifyClient _spotify;
    private ILogger<Worker> _logger;
    private Dictionary<string, string>? Artists;
    private List<string> Genres = new List<string>()
        {
            "reggaeton",
            "trap latino",
            "urbano latino",
            "latin hip hop",
            "reggaeton flow",
        };
    #endregion

    #region Private Methods
    private async Task BuildArtistList()
    {
        foreach (var genre in Genres)
        {
            var searchRequest = new SearchRequest(SearchRequest.Types.All, genre);
            var searchResults = await _spotify.Search.Item(searchRequest);
            await foreach (var item in _spotify.Paginate(searchResults.Artists, (s) => s.Artists))
            {
                if (!Artists.ContainsKey(item.Id))
                {
                    Artists.Add(item.Id, item.Name);
                }
            }
        }
    }
    #endregion










}