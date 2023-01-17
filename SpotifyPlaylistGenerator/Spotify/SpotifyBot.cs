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
    private Dictionary<string, string>? Artists = new Dictionary<string, string>();
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
            Console.WriteLine("Genre: {0}", genre);
            try
            {
                Console.WriteLine("Before Search");
                var searchRequest = new SearchRequest(SearchRequest.Types.All, genre);
                Console.WriteLine("searchRequest: {0}", searchRequest);
                var searchResults = await _spotify.Search.Item(searchRequest);
                Console.WriteLine("searchRequest.ArtistsCount: {0}", searchResults.Artists.Total);

                await foreach (var item in _spotify.Paginate(searchResults.Artists, (s) => s.Artists))
                {
                    Console.WriteLine(item.Name);
                    // you can use "break" here!
                }

                // var index = 0;
                // // var allResults = await _spotify.PaginateAll(searchResults.Artists, (s) => s.Artists);
                // foreach (var item in searchResults.Artists.Items)
                // {
                //     Console.WriteLine(index);
                //     Console.WriteLine("Item: {0}", item.Name);

                //     if (!Artists.ContainsKey(item.Id))
                //     {
                //         Console.WriteLine("Add Item: {0}", item.Id);
                //         Console.WriteLine("Item: {0} - {1}", item.Id, item.Name);
                //         Artists.Add(item.Id, item.Name);
                //     }
                //     index++;
                // }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
    #endregion










}