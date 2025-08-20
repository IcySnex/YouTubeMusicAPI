using YouTubeMusicAPI.Models;

namespace YouTubeMusicAPI.Tests.Client;

[TestFixture]
internal sealed class YouTubeMusicClientTests
{
    YouTubeMusicClient client;

    [SetUp]
    public async Task Setup() =>
        client = await TestData.CreateClientAsync();


    [Test]
    public void Should_get_authenticated_user()
    {
        // Act
        AuthenticatedUser? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.GetAuthenticatedUserAsync();
        });

        // Assert
        TestData.WriteResult(result);
    }
}