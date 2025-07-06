using YouTubeMusicAPI;
using YouTubeMusicAPI.Authentication;

namespace YouTubeMusicAPI.Tests.Client;

public class YouTubeMusicConfigTests
{
    [Test]
    public void Should_populate_properties_automatically()
    {
        // Arrange
        YouTubeMusicConfig config = new();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(config.GeographicalLocation, Is.Not.Null.Or.Empty);
            Assert.That(config.Authenticator, Is.Not.Null);
            Assert.That(config.HttpClient, Is.Not.Null);
        });
    }

    [Test]
    public void Should_maintain_parameters()
    {
        // Arrange
        string geographicalLocation = "DE";
        IAuthenticator authenticator = new AnonymousAuthenticator();
        HttpClient httpClient = new();

        YouTubeMusicConfig config = new()
        {
            GeographicalLocation = geographicalLocation,
            Authenticator = authenticator,
            HttpClient = httpClient
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(config.GeographicalLocation, Is.EqualTo(geographicalLocation));
            Assert.That(config.Authenticator, Is.EqualTo(authenticator));
            Assert.That(config.HttpClient, Is.EqualTo(httpClient));
        });
    }
}