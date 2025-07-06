using YouTubeMusicAPI.Authentication;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Tests.Http;

[TestFixture]
internal sealed class RequestHandlerTests
{
    [Test]
    public void Should_get()
    {
        // Arrange
        HttpClient httpClient = new();
        IAuthenticator authenticator = new AnonymousAuthenticator();
        RequestHandler requestHandler = new(TestData.GeographicalLocation, authenticator, httpClient);

        // Act
        string? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await requestHandler.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
        });

        // Assert
        Assert.That(result, Is.Not.Null.Or.Empty);
        Assert.That(result, Does.StartWith("{"));
        Assert.That(result, Contains.Substring("\"id\": 1"));
        Assert.That(result, Does.EndWith("}"));

        TestData.WriteResult(result);
    }

    [Test]
    [TestCase(ClientType.None)]
    [TestCase(ClientType.WebMusic)]
    [TestCase(ClientType.IOS)]
    [TestCase(ClientType.Tv)]
    public void Should_post_with_client(
        ClientType clientType)
    {
        // Arrange
        HttpClient httpClient = new();
        IAuthenticator authenticator = new AnonymousAuthenticator();
        RequestHandler requestHandler = new(TestData.GeographicalLocation, authenticator, httpClient);

        // Act
        string? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await requestHandler.PostAsync("https://jsonplaceholder.typicode.com/posts",
            [
                new("title", "foo"),
            ], clientType);
        });

        // Assert
        Assert.That(result, Is.Not.Null.Or.Empty);
        Assert.That(result, Does.StartWith("{"));
        Assert.That(result, Contains.Substring("\"title\": \"foo\""));
        if (clientType.Create() is YouTubeMusicAPI.Http.Client client)
        {
            Assert.That(result, Contains.Substring($"\"clientName\": \"{client.ClientName}\""));
            Assert.That(result, Contains.Substring($"\"clientVersion\": \"{client.ClientVersion}\""));
        }
        Assert.That(result, Does.EndWith("}"));

        TestData.WriteResult(result);
    }

}