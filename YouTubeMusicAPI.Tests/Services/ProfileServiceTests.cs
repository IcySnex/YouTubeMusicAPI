using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Services.Profiles;

namespace YouTubeMusicAPI.Tests.Services;

public class ProfileServiceTests
{
    YouTubeMusicClient client;

    [SetUp]
    public async Task Setup() =>
        client = await TestData.CreateClientAsync();


    [Test]
    public void Should_search()
    {
        // Act
        IReadOnlyList<ProfileSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<ProfileSearchResult> response = client.Profiles.SearchAsync(TestData.SearchQuery, TestData.SearchScope, TestData.SearchIgnoreSpelling);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }


    [Test]
    public void Should_get()
    {
        // Act
        ProfileInfo? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Profiles.GetAsync(TestData.ProfileBrowseId);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }
}
