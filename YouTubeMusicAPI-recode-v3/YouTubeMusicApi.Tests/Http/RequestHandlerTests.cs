using YouTubeMusicAPI.Authentication;
using YouTubeMusicAPI.Http;

namespace YouTubeMusicApi.Tests.Http;

[TestFixture]
public class RequestHandlerTests
{
    [Test]
    public void Should_get()
    {
        // Arrange
        HttpClient client = new();
        IAuthenticator authenticator = new AnonymousAuthenticator();
        RequestHandler requestHandler = new(client, authenticator);

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
    public void Should_post()
    {
        // Arrange
        HttpClient client = new();
        IAuthenticator authenticator = new AnonymousAuthenticator();
        RequestHandler requestHandler = new(client, authenticator);

        // Act
        string? response = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            response = await requestHandler.PostAsync("https://jsonplaceholder.typicode.com/posts", new
            {
                Title = "foo",
                Body = "bar",
                UserId = 1
            });
        });

        // Assert
        Assert.That(response, Is.Not.Null.Or.Empty);
        Assert.That(response, Does.StartWith("{"));
        Assert.That(response, Contains.Substring("\"title\": \"foo\""));
        Assert.That(response, Contains.Substring("\"body\": \"bar\""));
        Assert.That(response, Contains.Substring("\"userId\": 1"));
        Assert.That(response, Does.EndWith("}"));

        // Output
        TestContext.Out.WriteLine(response);
    }

}