using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using SpotifyPlaylistGenerator;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SpotifyAPI.Web.Http;
using SpotifyAPI.Web;

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
    public void Extract_Artists_Names()
    {
        var mock = new Mock<ILogger<Worker>>();
        ILogger<Worker> logger = mock.Object;
        //or use this short equivalent 
        logger = Mock.Of<ILogger<Worker>>();

        var api = new Mock<IAPIConnector>();
        var config = SpotifyClientConfig.CreateDefault("FakeToken").WithAPIConnector(api.Object);
        var spotify = new SpotifyClient(config);

        SpotifyBot spotifyBot = new SpotifyBot(logger, spotify);


    }

}