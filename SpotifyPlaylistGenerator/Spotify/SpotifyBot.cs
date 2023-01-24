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
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        await DeletePlaylistTracks();
        await AddTrackToPlaylist();
        watch.Stop();

        Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms, {watch.ElapsedMilliseconds / 1000} s, {watch.ElapsedMilliseconds / 1000 / 60} mins");
    }
    #endregion

    #region Private Variables
    private SpotifyClient _spotify;
    private ILogger<Worker> _logger;
    private Dictionary<string, string>? Artists = new Dictionary<string, string>();
    private List<string> Genres = new List<string>(){
            "reggaeton",
            "trap latino",
            "urbano latino",
            "latin hip hop",
            "reggaeton flow",
        };
    private string _playlistID = "25FqRZoYPOXC3qFIMUXppP";
    #endregion

    #region Private Methods
    private async Task AddTrackToPlaylist()
    {
        foreach (var genre in Genres)
        {
            Console.WriteLine("Getting Genre {0}", genre);
            try
            {
                var searchRequest = new SearchRequest(SearchRequest.Types.All, genre);
                var searchResults = await _spotify.Search.Item(searchRequest);

                decimal index = 1;
                await foreach (var track in _spotify.Paginate(searchResults.Tracks, (s) => s.Tracks))
                {
                    if (AlbumDateWithinRange(track.Album.ReleaseDate) && track.Popularity > 10)
                    {
                        Console.WriteLine("Song: {0} in Album {1} within Date Range (14 days) {2} ", track.Name, track.Album.Name, track.Album.ReleaseDate);
                        await AddSongToPlaylist(track);
                    }

                    decimal? percentage = (index / searchResults.Tracks.Total) * 100;
                    decimal percentageFormatted = decimal.Round(Decimal.Parse(percentage.ToString()), 2, MidpointRounding.AwayFromZero);
                    index++;
                    if (percentageFormatted % 5 == 0)
                    {
                        Console.WriteLine("AddTrackToPlaylist Progress: {0}", percentageFormatted);
                    }
                }
            }
            catch (APITooManyRequestsException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Response?.StatusCode);
                Console.WriteLine("APITooManyRequestsException waiting 30 seconds");
                System.Threading.Thread.Sleep(30000);
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
    private bool AlbumDateWithinRange(string date)
    {
        DateTime songDate;
        bool isWithinRange = false;
        if (DateTime.TryParse(date, out songDate))
        {
            var dateLimit = DateTime.Today.AddDays(-21);
            isWithinRange = (songDate >= dateLimit);
        }
        return isWithinRange;
    }

    private async Task DeletePlaylistTracks()
    {
        try
        {
            var playlist = await _spotify.Playlists.Get(_playlistID);
            var hasNext = playlist.Tracks.Next is not null ? true : false;

            if (playlist.Tracks.Total == 0) return;

            while (hasNext || (playlist.Tracks.Total < 100 && playlist.Tracks.Total > 0))
            {
                Console.WriteLine($"Playlist: {playlist.Name} - {playlist.Tracks.Total} songs");
                var playlistIds = new List<PlaylistRemoveItemsRequest.Item>();

                foreach (PlaylistTrack<IPlayableItem> item in playlist.Tracks.Items)
                {
                    if (item.Track is FullTrack track)
                    {
                        var playlistItem = new PlaylistRemoveItemsRequest.Item();
                        playlistItem.Uri = track.Uri;
                        playlistIds.Add(playlistItem);
                    }
                }

                PlaylistRemoveItemsRequest request = new PlaylistRemoveItemsRequest();
                request.Tracks = playlistIds;

                var delete = await _spotify.Playlists.RemoveItems(_playlistID, request);
                Console.WriteLine($"Playlist: {playlist.Name} - Songs Removed {delete.SnapshotId}");

                playlist = await _spotify.Playlists.Get(_playlistID);
                hasNext = playlist.Tracks.Next is not null ? true : false;
            }
        }
        catch (APITooManyRequestsException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.Response?.StatusCode);
            Console.WriteLine("APITooManyRequestsException waiting 30 seconds");
            System.Threading.Thread.Sleep(30000);
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

    private async Task AddSongToPlaylist(SpotifyAPI.Web.FullTrack track)
    {
        try
        {
            Console.WriteLine($"Adding Track {track.Name}");

            PlaylistAddItemsRequest request = new PlaylistAddItemsRequest(new List<string> { track.Uri });
            var update = await _spotify.Playlists.AddItems(_playlistID, request);
        }
        catch (APITooManyRequestsException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.Response?.StatusCode);
            Console.WriteLine("APITooManyRequestsException waiting 30 seconds");
            System.Threading.Thread.Sleep(30000);
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
    #endregion










}