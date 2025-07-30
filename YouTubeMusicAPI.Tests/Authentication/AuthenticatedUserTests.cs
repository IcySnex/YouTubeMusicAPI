using System.Net;
using YouTubeMusicAPI.Authentication;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;

namespace YouTubeMusicAPI.Tests.Authentication;

[TestFixture]
internal sealed class AuthenticatedUserTests
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