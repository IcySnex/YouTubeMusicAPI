using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Streaming;

namespace YouTubeMusicAPI.Tests;

/// <summary>
/// Streaming
/// </summary>
internal class Streaming
{
    ILogger logger;
    YouTubeMusicClient client;

    [SetUp]
    public void Setup()
    {
        ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        logger = factory.CreateLogger<Search>();
        client = new(logger, TestData.GeographicalLocation, TestData.VisitorData, TestData.PoToken, TestData.Cookies);
    }


    /// <summary>
    /// Get streaming data
    /// </summary>
    [Test]
    public void StreamingData()
    {
        StreamingData? data = null;
        Assert.DoesNotThrowAsync(async () =>
        {
            data = await client.GetStreamingDataAsync(TestData.SongVideoId);
        });
        Assert.That(data, Is.Not.Null);
        Assert.That(data.StreamInfo, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(data, Formatting.Indented);
        logger.LogInformation("\nStreaming Data:\n{data}", readableResults);
    }

    /// <summary>
    /// Get stream
    /// </summary>
    [Test]
    public void Stream()
    {
        Stream? stream = null;
        Assert.DoesNotThrowAsync(async () =>
        {
            StreamingData data = await client.GetStreamingDataAsync(TestData.SongVideoId);
            Assert.That(data, Is.Not.Null);
            Assert.That(data.StreamInfo, Is.Not.Empty);

            stream = await data.StreamInfo[0].GetStreamAsync();
        });
        Assert.That(stream, Is.Not.Null);

        // Output
        logger.LogInformation("\nStream Lenght:\n{streamLenght}", stream.Length);
    }


    /// <summary>
    /// Download to file
    /// </summary>
    [Test]
    public void Download()
    {
        Assert.DoesNotThrowAsync(async () =>
        {
            StreamingData data = await client.GetStreamingDataAsync(TestData.SongVideoId);
            Assert.That(data, Is.Not.Null);
            Assert.That(data.StreamInfo, Is.Not.Empty);

            Stream stream = await data.StreamInfo[0].GetStreamAsync();
            Assert.That(stream, Is.Not.Null);
            Assert.That(stream.Length, Is.GreaterThan(0));

            using FileStream fileStream = new(TestData.FilePath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);
        });

        // Output
        logger.LogInformation("Download finished");
    }


    /// <summary>
    /// Add song or video to watch history
    /// </summary>
    [Test]
    public void AddToWatchHistory()
    {
        Assert.DoesNotThrowAsync(async () =>
        {
            SongVideoInfo songVideo = await client.GetSongVideoInfoAsync(TestData.SongVideoId);

            await client.AddToWatchHistoryAsync(songVideo);
        });

        // Output
        logger.LogInformation("Added song or video to watch history");
    }

    /// <summary>
    /// Updates the watch time of a song or video
    /// </summary>
    [Test]
    public void UpdateWatchTime()
    {
        Assert.DoesNotThrowAsync(async () =>
        {
            SongVideoInfo songVideo = await client.GetSongVideoInfoAsync(TestData.SongVideoId);

            await client.AddToWatchHistoryAsync(songVideo);
            await client.UpdateWatchTimeAsync(songVideo, TestData.WatchTime);
        });

        // Output
        logger.LogInformation("Added song or video to watch history and updated watch time");
    }
}