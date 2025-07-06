using YouTubeMusicAPI.Authentication;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicApi.Tests.Http;

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
        string? response = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            response = await requestHandler.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
        });

        // Assert
        Assert.That(response, Is.Not.Null.Or.Empty);
        Assert.That(response, Does.StartWith("{"));
        Assert.That(response, Contains.Substring("\"id\": 1"));
        Assert.That(response, Does.EndWith("}"));

        // Output
        TestContext.Out.WriteLine(response);
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
        string? response = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            response = await requestHandler.PostAsync("https://jsonplaceholder.typicode.com/posts",
            [
                new("title", "foo"),
            ], clientType);
        });

        // Assert
        Assert.That(response, Is.Not.Null.Or.Empty);
        Assert.That(response, Does.StartWith("{"));
        Assert.That(response, Contains.Substring("\"title\": \"foo\""));
        if (clientType.Create() is YouTubeMusicAPI.Http.Client client)
        {
            Assert.That(response, Contains.Substring($"\"clientName\": \"{client.ClientName}\""));
            Assert.That(response, Contains.Substring($"\"clientVersion\": \"{client.ClientVersion}\""));
        }
        Assert.That(response, Does.EndWith("}"));

        // Output
        TestContext.Out.WriteLine(response);
    }

}