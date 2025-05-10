using Microsoft.Extensions.Logging;
using YouTubeMusicAPI;
using YouTubeMusicAPI.Authentication;

namespace YouTubeMusicApi.Tests;

public class Example
{
    [Test]
    public async Task TestAsync()
    {
        // Getting Started
        ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger<Example>();

        string visitorData = await AuthenticationDataGenerator.VisitorDataAsync();
        string rolloutToken = await AuthenticationDataGenerator.RolloutTokenAsync();
        IAuthenticator authenticator = new AnonymousAuthenticator(visitorData, rolloutToken);

        YouTubeMusicConfig config = new()
        {
            GeographicalLocation = "DE",
            Logger = logger,
            Authenticator = authenticator,
        };
        YouTubeMusicClient client = new(config);

    }
}