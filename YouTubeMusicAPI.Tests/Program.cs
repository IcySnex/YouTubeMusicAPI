using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Authentication;

namespace YouTubeMusicAPI.Tests;

public class Program
{
    [Test]
    public async Task MainAsync()
    {
        // Getting Started
        ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger<Program>();

        IAuthenticator authenticator = new AnonymousAuthenticator();

        YouTubeMusicConfig config = new()
        {
            GeographicalLocation = "DE",
            Logger = logger,
            Authenticator = authenticator,
        };
        YouTubeMusicClient client = new(config);

        await Task.Delay(100);
    }
}