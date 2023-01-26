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
        await GetGenreArtists();

        watch.Stop();

        Console.WriteLine($"Execution Time: {Decimal.Parse(watch.ElapsedMilliseconds.ToString())} ms, {Decimal.Parse((watch.ElapsedMilliseconds / 1000).ToString())} s, {Decimal.Parse((watch.ElapsedMilliseconds / 1000 / 60).ToString())} mins");
    }
    #endregion

    #region Private Variables
    private SpotifyClient _spotify;
    private ILogger<Worker> _logger;
    private List<string> ArtistsNames = new List<string>();
    private List<string> SongURIS = new List<string>();
    private List<string> AlbumNames = new List<string>();
    private List<string> Genres = new List<string>(){
            "reggaeton",
            "trap latino",
            "urbano latino",
            "latin hip hop",
            "reggaeton flow",
        };
    private List<string> ExclusionArtists = new List<string>(){
            "Dj Cumbio",
            "Reggaeton Latino",
            "Reggaetone",
            "Reggaetoni",
            "J Balvin",
            "Malekhe",
            "Luis Orlando Ortiz Ibañez",
            "Reggaeton en VIVO",
            "Trap Land",
            "Lofi Hip Hop Nation",
            "Angelique",
            "Lo-fi Hip Hop Beats",
            "Lofi Hip-Hop Music",
            "Hip Hop 90's",
            "Latin Music",
            "Música de Elevador",
            "No Box Sounds",
            "Smooth Jazz Beats",
            "Dinner Jazz Orchestra",
            "Latin Jazz Vibes",
            "Lofi Sleep Chill & Study",
            "Lo Fi Hip Hop",
            "Lofi Chillhop Gaming Streaming Work Music",
            "Lofis",
            "Lofi Night Drives",
            "loftown",
        };

    private List<string> ExclusionAlbums = new List<string>(){
        "Re Kgopela Tshireletso"
    };

    private string _playlistID = "25FqRZoYPOXC3qFIMUXppP";
    #endregion

    #region Private Methods
    private async Task GetGenreArtists()
    {
        decimal genreIndex = 1;
        foreach (var genre in Genres)
        {
            Console.WriteLine("Getting Genre {0}", genre);
            try
            {
                var searchRequest = new SearchRequest(SearchRequest.Types.All, genre);
                var searchResults = await _spotify.Search.Item(searchRequest);

                decimal index = 1;
                await foreach (var artist in _spotify.Paginate(searchResults.Artists, (s) => s.Artists))
                {
                    if (artist.Name.ToLower().Contains("lofi") ||
                    artist.Name.ToLower().Contains("lo-fi") ||
                    artist.Name.ToLower().Contains("lo fi"))
                    {
                        Console.WriteLine($"GetGenreArtists: {artist.Name} - Skipping LOFI");
                        continue;

                    }
                    if (ExclusionArtists.Find(ea => ea == artist.Name) is not null)
                    {
                        Console.WriteLine($"GetGenreArtists: {artist.Name} - Excluded");
                        continue;
                    }

                    if (ArtistsNames.Find(a => a == artist.Name) is not null)
                    {
                        Console.WriteLine($"GetGenreArtists: {artist.Name} - Already processed");
                        continue;
                    }

                    if (artist.Popularity < 15)
                    {
                        Console.WriteLine($"GetGenreArtists: {artist.Name} - Skipped w/ Popularity:  {artist.Popularity}");
                        continue;
                    };

                    ArtistsNames.Add(artist.Name);
                    await GetArtistAlbums(artist);

                    decimal? genrePercentage = (genreIndex / Genres.Count()) * 100;
                    decimal genrePercentageFormatted = decimal.Round(Decimal.Parse(genrePercentage.ToString()), 2, MidpointRounding.AwayFromZero);
                    decimal? percentage = (index / searchResults.Artists.Total) * 100;
                    decimal percentageFormatted = decimal.Round(Decimal.Parse(percentage.ToString()), 2, MidpointRounding.AwayFromZero);
                    index++;
                    if (percentageFormatted % 3 == 0)
                    {
                        Console.WriteLine($"GetGenreArtists Completed:  {percentageFormatted}%");
                        Console.WriteLine($"GetGenreArtists Genre Completed:  {genrePercentageFormatted}%");
                    }
                }
                genreIndex++;
            }
            catch (APITooManyRequestsException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Response?.StatusCode);
                Console.WriteLine($"GetGenreArtists: APITooManyRequestsException waiting {e.RetryAfter} seconds");
                System.Threading.Thread.Sleep(e.RetryAfter);
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
    private async Task GetArtistAlbums(SpotifyAPI.Web.FullArtist artist)
    {
        try
        {
            var searchResults = await _spotify.Artists.GetAlbums(artist.Id);

            decimal index = 1;
            await foreach (var album in _spotify.Paginate(searchResults))
            {
                decimal? percentage = (index / searchResults.Total) * 100;
                decimal percentageFormatted = decimal.Round(Decimal.Parse(percentage.ToString()), 2, MidpointRounding.AwayFromZero);
                index++;
                if (percentageFormatted % 12 == 0)
                {
                    Console.WriteLine($"GetArtistAlbums: {artist.Name} - Completed:  {percentageFormatted}%");
                }
                if (ExclusionAlbums.Find(ea => ea == album.Name) is not null)
                {
                    Console.WriteLine($"GetArtistAlbums: {album.Name} - Excluded");
                    continue;
                }
                if (AlbumNames.Find(a => a == album.Name) is not null)
                {
                    Console.WriteLine($"GetArtistAlbums: {album.Name} - Already processed");
                    continue;
                }
                if (AlbumDateWithinRange(album.ReleaseDate) && album.AlbumType != "compilation")
                {
                    Console.WriteLine("Album {0} within Date Range (21 days) {1} ", album.Name, album.ReleaseDate);
                    AlbumNames.Add(album.Name);
                    await GetAlbumTracks(album);
                }

            }
        }
        catch (APITooManyRequestsException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.Response?.StatusCode);
            Console.WriteLine($"GetArtistAlbums: APITooManyRequestsException waiting {e.RetryAfter} seconds");
            System.Threading.Thread.Sleep(e.RetryAfter);
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
    private async Task GetAlbumTracks(SimpleAlbum album)
    {
        try
        {
            List<string> albumTrackUris = new List<string>();
            var searchResults = await _spotify.Albums.GetTracks(album.Id);

            decimal index = 1;
            await foreach (var track in _spotify.Paginate(searchResults))
            {
                decimal? percentage = (index / searchResults.Total) * 100;
                decimal percentageFormatted = decimal.Round(Decimal.Parse(percentage.ToString()), 2, MidpointRounding.AwayFromZero);
                index++;
                if (percentageFormatted % 5 == 0)
                {
                    Console.WriteLine($"GetAlbumTracks: {album.Name} - Completed:  {percentageFormatted}%");
                }

                if (SongURIS.Find(uri => uri == track.Uri) is not null)
                {
                    Console.WriteLine($"GetAlbumTracks: {track.Uri} - Already Processed");
                    continue;
                }

                SongURIS.Add(track.Uri);
                albumTrackUris.Add(track.Uri);
            }

            if (albumTrackUris.Count() > 0)
            {
                await AddSongsToPlaylist(albumTrackUris);
            }

        }
        catch (APITooManyRequestsException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.Response?.StatusCode);
            Console.WriteLine($"GetAlbumTracks: APITooManyRequestsException waiting {e.RetryAfter} seconds");
            System.Threading.Thread.Sleep(e.RetryAfter);
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

            decimal? index = 1;
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

                decimal? percentage = (index / playlistIds.Count()) * 100;
                decimal percentageFormatted = decimal.Round(Decimal.Parse(percentage.ToString()), 2, MidpointRounding.AwayFromZero);
                index++;
                if (percentageFormatted % 10 == 0)
                {
                    Console.WriteLine($"DeletePlaylistTracks - Completed:  {percentageFormatted}%");
                    System.Threading.Thread.Sleep(10000);
                }
            }
        }
        catch (APITooManyRequestsException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.Response?.StatusCode);
            Console.WriteLine($"DeletePlaylistTracks: APITooManyRequestsException waiting {e.RetryAfter} seconds");
            System.Threading.Thread.Sleep(e.RetryAfter);
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
    private async Task AddSongsToPlaylist(List<string> trackURIs)
    {
        if (trackURIs.Count == 0) return;
        try
        {
            Console.WriteLine($"Adding {trackURIs.Count()} Tracks to playlist. Total Playlist Tracks {SongURIS.Count()} ");
            PlaylistAddItemsRequest request = new PlaylistAddItemsRequest(trackURIs);
            var update = await _spotify.Playlists.AddItems(_playlistID, request);
        }
        catch (APITooManyRequestsException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.Response?.StatusCode);
            Console.WriteLine($"AddSongsToPlaylist: APITooManyRequestsException waiting {e.RetryAfter} seconds");
            System.Threading.Thread.Sleep(e.RetryAfter);
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