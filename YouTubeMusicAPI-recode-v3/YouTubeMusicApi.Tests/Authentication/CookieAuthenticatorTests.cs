using System.Net;
using YouTubeMusicAPI.Authentication;

namespace YouTubeMusicApi.Tests.Authentication;

[TestFixture]
public class CookieAuthenticatorTests
{
    [Test]
    public void Should_maintain_parameters()
    {
        // Arrange
        Cookie[] cookies = TestData.RandomCookies();
        string visitorData = TestData.RandomString();
        string poToken = TestData.RandomString();
        string apiKey = TestData.RandomString();
        string userAgent = TestData.RandomString();

        CookieAuthenticator authenticator = new(cookies, visitorData, poToken, apiKey, userAgent);

        CookieCollection storedCookies = authenticator.Container.GetAllCookies();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(authenticator.VisitorData, Is.EqualTo(visitorData));
            Assert.That(authenticator.ProofOfOriginToken, Is.EqualTo(poToken));
            Assert.That(authenticator.ApiKey, Is.EqualTo(apiKey));
            Assert.That(authenticator.UserAgent, Is.EqualTo(userAgent));

            foreach (Cookie cookie in cookies)
                Assert.That(storedCookies.Any(c => c.Name == cookie.Name && c.Value == cookie.Value), Is.True);
        });
    }

    [Test]
    public void Should_add_socs_when_applying()
    {
        // Arrange
        CookieAuthenticator authenticator = new(TestData.RandomCookies());
        HttpRequestMessage request = new(HttpMethod.Post, "https://music.youtube.com/youtubei/v1/player");

        // Act
        authenticator.Apply(request);

        // Assert
        string? cookieHeader = request.Headers.GetValues("Cookie").FirstOrDefault();
        Assert.That(cookieHeader, Does.Contain("SOCS=CAI"));
    }

    [Test]
    public void Should_add_cookies_when_applying()
    {
        // Arrange
        Cookie[] cookies = TestData.RandomCookies();

        CookieAuthenticator authenticator = new(cookies, TestData.RandomString());
        HttpRequestMessage request = new(HttpMethod.Post, "https://music.youtube.com/youtubei/v1/player");

        // Act
        authenticator.Apply(request);

        // Assert
        Assert.Multiple(() =>
        {
            string? cookieHeader = request.Headers.GetValues("Cookie").FirstOrDefault();
            foreach (Cookie cookie in cookies)
                Assert.That(cookieHeader, Does.Contain($"{cookie.Name}={cookie.Value}"));
        });
    }

    [Test]
    public void Should_add_auth_header_when_applying()
    {
        // Arrange
        CookieAuthenticator authenticator = new(TestData.RandomCookies());
        HttpRequestMessage request = new(HttpMethod.Post, "https://music.youtube.com/youtubei/v1/player");

        // Act
        authenticator.Apply(request);

        // Assert
        Assert.Multiple(() =>
        {
            string? authHeader = request.Headers.Authorization?.ToString();
            Assert.That(authHeader, Is.Not.Null.Or.Empty);

            Assert.That(authHeader, Does.Contain("SAPISIDHASH"));

            string? timestampPart = authHeader?.Split(' ')[1];
            Assert.That(timestampPart, Is.Not.Null.Or.Empty);

            Assert.That(timestampPart, Does.Match(@"^[0-9]+_[A-F0-9]+$"));
        });
    }
}