using SpotifyPlaylistGenerator;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddHttpClient();


    })
    .Build();

host.Run();
