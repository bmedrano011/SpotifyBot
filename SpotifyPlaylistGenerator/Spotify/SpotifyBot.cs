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
        await BuildSongIdList();
        // await BuildPlaylist();

    }
    #endregion

    #region Private Variables
    private SpotifyClient _spotify;
    private ILogger<Worker> _logger;
    private Dictionary<string, string>? Artists = new Dictionary<string, string>();
    private List<string>? SongIds { get; set; } = new List<string>();
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

            try
            {
                await foreach (var item in _spotify.Paginate(searchResults.Artists, (s) => s.Artists))
                {
                    if (!Artists.ContainsKey(item.Id))
                    {

                        Artists.Add(item.Id, item.Name);
                    }
                }
            }
            catch (APIException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Response?.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }

    private async Task BuildSongIdList()
    {
        if (Artists is not null)
        {
            foreach (var art in Artists)
            {
                var searchResults = await _spotify.Artists.GetAlbums(art.Key);
                try
                {
                    await foreach (var item in _spotify.Paginate(searchResults))
                    {
                        if (DateWithinRange(item.ReleaseDate))
                        {
                            var songId = await GetSongId(item.Id, item.Name);
                            if (!SongIds.Contains(songId) && !String.IsNullOrEmpty(songId))
                            {
                                SongIds.Add(songId);
                            }
                        }
                    }
                }
                catch (APIException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.Response?.StatusCode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

    }

    private async Task<string> GetSongId(string albumId, string songName)
    {
        string songId = String.Empty;
        var searchResults = await _spotify.Albums.GetTracks(albumId);
        try
        {
            await foreach (var item in _spotify.Paginate(searchResults))
            {
                if (item.Name == songName) songId = item.Id;
            }
        }
        catch (APIException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.Response?.StatusCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return songId;
    }
    private bool DateWithinRange(string date)
    {
        DateTime songDate = DateTime.Parse(date);
        var dateLimit = DateTime.Today.AddDays(-14);

        return (songDate >= dateLimit);
    }

    private async Task BuildPlaylist()
    {

    }
    #endregion










}