using SpotifyPlaylistGenerator;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddHttpClient<SpotifyClientBuilder>(c => c.BaseAddress = new Uri("https://api.spotify.com/v1/"));


    })
    .Build();

host.Run();
