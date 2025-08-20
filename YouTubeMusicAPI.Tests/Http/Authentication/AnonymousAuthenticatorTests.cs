using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Http.Authentication;

namespace YouTubeMusicAPI.Tests.Http.Authentication;

[TestFixture]
internal sealed class AnonymousAuthenticatorTests
{
    [Test]
    public void Should_maintain_parameters()
    {
        // Arrange
        string visitorData = TestData.RandomString();
        string poToken = TestData.RandomString();
        string rolloutToken = TestData.RandomString();

        AnonymousAuthenticator authenticator = new(visitorData, rolloutToken, poToken);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(authenticator.VisitorData, Is.EqualTo(visitorData));
            Assert.That(authenticator.ProofOfOriginToken, Is.EqualTo(poToken));
            Assert.That(authenticator.RolloutToken, Is.EqualTo(rolloutToken));
        });
    }

    [Test]
    public void Should_add_socs_when_applying()
    {
        // Arrange
        AnonymousAuthenticator authenticator = new();
        HttpRequestMessage request = new(HttpMethod.Post, "https://music.youtube.com/youtubei/v1/player");

        // Act
        bool result = authenticator.Apply(request, ClientType.WebMusic);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);

            string cookieHeader = request.Headers.GetValues("Cookie").FirstOrDefault() ?? "";
            Assert.That(cookieHeader, Does.Contain("SOCS=CAI"), "SOCS=CAI cookie is missing");
        });
    }
}