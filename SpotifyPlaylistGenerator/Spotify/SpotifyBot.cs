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
        await BuildArtistList();
        await AddArtistTracksToPlayList();
        watch.Stop();

        Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms, {watch.ElapsedMilliseconds / 1000} s, {watch.ElapsedMilliseconds / 1000 / 60000} mins");
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
    private async Task BuildArtistList()
    {
        foreach (var genre in Genres)
        {
            Console.WriteLine("Getting Genre {0}", genre);
            try
            {
                var searchRequest = new SearchRequest(SearchRequest.Types.All, genre);
                var searchResults = await _spotify.Search.Item(searchRequest);

                decimal index = 1;
                await foreach (var item in _spotify.Paginate(searchResults.Artists, (s) => s.Artists))
                {
                    if (item.Popularity > 35 && !Artists.ContainsKey(item.Id))
                    {
                        Console.WriteLine("Getting Artist {0}", item.Name);
                        Artists.Add(item.Id, item.Name);
                    }
                    decimal? percentage = (index / searchResults.Artists.Total) * 100;
                    Console.WriteLine("BuildArtistList Progress: {0}", percentage);
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

    private async Task AddArtistTracksToPlayList()
    {
        if (Artists is not null)
        {
            decimal index = 1;
            foreach (var art in Artists)
            {
                Console.WriteLine("BuildSongIdList for Artist {0}", art.Value);
                //TODO: To many requests happening here
                try
                {
                    var searchResults = await _spotify.Artists.GetAlbums(art.Key);
                    // System.Threading.Thread.Sleep(30000);
                    await foreach (var album in _spotify.Paginate(searchResults))
                    {
                        Console.WriteLine("Artist Album: {0} - {1}", art.Value, album.Name);
                        if (AlbumDateWithinRange(album.ReleaseDate))
                        {
                            Console.WriteLine("Album: {0} within Date Range (14 days) {1}", album.Name, album.ReleaseDate);
                            List<string> albumTrackURIs = new List<string>(await GetTrackURIs(album.Id));
                            await AddSongsToPlaylist(albumTrackURIs);
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
                Console.WriteLine("Artist Progress: " + (index / Artists.Count()) * 100);
                index++;
            }

        }

    }

    private async Task<List<string>> GetTrackURIs(string albumId)
    {
        // System.Threading.Thread.Sleep(10000);
        List<string> songURIs = new List<string>();

        try
        {
            var searchResults = await _spotify.Albums.GetTracks(albumId);
            await foreach (var albumTrack in _spotify.Paginate(searchResults))
            {
                songURIs.Add(albumTrack.Uri);
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

        return songURIs;
    }

    private bool AlbumDateWithinRange(string date)
    {
        DateTime songDate = DateTime.Parse(date);
        var dateLimit = DateTime.Today.AddDays(-14);

        return (songDate >= dateLimit);
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

    private async Task AddSongsToPlaylist(List<string> songUris)
    {
        try
        {
            if (songUris?.Count() == 0) { return; }
            Console.WriteLine("Adding Tracks");

            PlaylistAddItemsRequest request = new PlaylistAddItemsRequest(songUris);
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