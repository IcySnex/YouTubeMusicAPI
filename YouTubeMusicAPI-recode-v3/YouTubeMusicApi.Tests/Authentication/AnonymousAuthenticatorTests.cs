using YouTubeMusicAPI.Authentication;

namespace YouTubeMusicApi.Tests.Authentication;

[TestFixture]
public class AnonymousAuthenticatorTests
{
    [Test]
    public void Should_maintain_parameters()
    {
        // Arrange
        string visitorData = TestData.RandomString();
        string poToken = TestData.RandomString();
        string apiKey = TestData.RandomString();
        string userAgent = TestData.RandomString();

        AnonymousAuthenticator authenticator = new(visitorData, poToken, apiKey, userAgent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(authenticator.VisitorData, Is.EqualTo(visitorData));
            Assert.That(authenticator.ProoOfOriginToken, Is.EqualTo(poToken));
            Assert.That(authenticator.ApiKey, Is.EqualTo(apiKey));
            Assert.That(authenticator.UserAgent, Is.EqualTo(userAgent));
        });
    }

    [Test]
    public void Should_add_socs_when_applying()
    {
        // Arrange
        AnonymousAuthenticator authenticator = new();
        HttpRequestMessage request = new(HttpMethod.Post, "https://music.youtube.com/youtubei/v1/player");

        // Act
        authenticator.Apply(request);

        // Assert
        string cookieHeader = request.Headers.GetValues("Cookie").FirstOrDefault() ?? "";
        Assert.That(cookieHeader, Does.Contain("SOCS=CAI"), "SOCS=CAI cookie is missing");
    }
}