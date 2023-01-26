using Moq;
using SpotifyPlaylistGenerator;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web.Http;
using SpotifyAPI.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace SpotifyLibraryTest;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void Test1()
    {
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task Delete_Tracks_From_Playlist_Success()
    {
        var mock = new Mock<ILogger<Worker>>();
        ILogger<Worker> logger = mock.Object;
        //or use this short equivalent 
        logger = Mock.Of<ILogger<Worker>>();

        var api = new Mock<IAPIConnector>();
        var config = SpotifyClientConfig.CreateDefault("FakeToken").WithAPIConnector(api.Object);
        var spotify = new SpotifyClient(config);

        SpotifyBot spotifyBot = new SpotifyBot(logger, spotify);


        PrivateObject obj = new PrivateObject(spotifyBot);


        var playlistClient = new Mock<IPlaylistsClient>();
        playlistClient.Setup(u =>
         u.RemoveItems(It.IsAny<string>(), It.IsAny<PlaylistRemoveItemsRequest>(), It.IsAny<CancellationToken>()));

        Assert.IsTrue(true);
    }
    // [TestMethod]
    // public void Extract_Artists_Names()
    // {
    //     var mock = new Mock<ILogger<Worker>>();
    //     ILogger<Worker> logger = mock.Object;
    //     //or use this short equivalent 
    //     logger = Mock.Of<ILogger<Worker>>();

    //     var api = new Mock<IAPIConnector>();
    //     var config = SpotifyClientConfig.CreateDefault("FakeToken").WithAPIConnector(api.Object);
    //     var spotify = new SpotifyClient(config);

    //     SpotifyBot spotifyBot = new SpotifyBot(logger, spotify);


    // }

}

public class PrivateObject
{
    private readonly object o;

    public PrivateObject(object o)
    {
        this.o = o;
    }

    public async Task<object> Invoke(string methodName, params object[] args)
    {
        var methodInfo = o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (methodInfo == null)
        {
            throw new Exception($"Method'{methodName}' not found is class '{o.GetType()}'");
        }
        return methodInfo.Invoke(o, args);
    }
}