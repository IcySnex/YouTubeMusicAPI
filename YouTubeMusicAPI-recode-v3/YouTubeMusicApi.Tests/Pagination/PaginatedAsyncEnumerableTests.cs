using System.Text.Json;
using YouTubeMusicAPI.Pagination;

namespace YouTubeMusicApi.Tests.Pagination;

public class PaginatedAsyncEnumerableTests
{
    static readonly int PageSize = 10;

    static readonly int[] Items = [.. Enumerable.Range(1, 100)];

    static async Task<Page<int>> FetchPageAsync(
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        TestContext.Out.WriteLine("Fetching page: {0}", continuationToken ?? "N/A");
        await Task.Delay(100, cancellationToken);

        int offset = continuationToken is null ? 0 : int.Parse(continuationToken);
        int[] result = Items[offset..(offset + PageSize)];

        string? nextContinuationToken = offset + PageSize >= Items.Length ? null : (offset + PageSize).ToString();
        Page<int> page = new(result, nextContinuationToken);

        return page;
    }


    [Test]
    public void Should_enumerate_all_items()
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync);

        // Act
        List<int> result = [];
        Assert.DoesNotThrowAsync(async () =>
        {
            await foreach (int item in enumerable)
                result.Add(item);
        });

        // Assert
        Assert.That(result, Is.EquivalentTo(Items));

        // Output
        string json = JsonSerializer.Serialize(result);
        TestContext.Out.WriteLine("Items: {0}", json);
    }


    [Test]
    [TestCase(0, null)]
    [TestCase(0, 25)]
    [TestCase(25, 25)]
    [TestCase(50, null)]
    public void Should_fetch_range_of_items(
        int offset,
        int? limit)
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync);

        // Act
        IReadOnlyList<int>? result = null;
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await enumerable.FetchItemsAsync(offset, limit);
        });

        // Assert
        Assert.That(result, Is.EquivalentTo(Items[offset..(offset + limit ?? Items.Length)]));

        // Output
        string json = JsonSerializer.Serialize(result);
        TestContext.Out.WriteLine("Items: {0}", json);
    }


    [Test]
    public void Should_fetch_next_page()
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync);

        // Act
        IReadOnlyList<int>? result = null;
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await enumerable.FetchNextPageAsync();
        });

        // Assert
        Assert.That(result, Is.EquivalentTo(Items[..PageSize]));

        // Output
        string json = JsonSerializer.Serialize(result);
        TestContext.Out.WriteLine("Items: {0}", json);
    }

    [Test]
    public void Should_fetch_next_next_page()
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync);

        // Act
        IReadOnlyList<int>? result = null;
        Assert.DoesNotThrowAsync(async () =>
        {
            await enumerable.FetchNextPageAsync();

            result = await enumerable.FetchNextPageAsync();
        });

        // Assert
        Assert.That(result, Is.EquivalentTo(Items[PageSize..(2 * PageSize)]));

        // Output
        string json = JsonSerializer.Serialize(result);
        TestContext.Out.WriteLine("Items: {0}", json);
    }


    [Test]
    public void Should_fetch_previous_page()
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync);

        // Act
        IReadOnlyList<int>? result = null;
        Assert.DoesNotThrowAsync(async () =>
        {
            await enumerable.FetchNextPageAsync();
            await enumerable.FetchNextPageAsync();

            result = await enumerable.FetchPreviousPageAsync();
        });

        // Assert
        Assert.That(result, Is.EquivalentTo(Items[..PageSize]));

        // Output
        string json = JsonSerializer.Serialize(result);
        TestContext.Out.WriteLine("Items: {0}", json);
    }

    [Test]
    public void Should_fetch_previous_previous_page()
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync);

        // Act
        IReadOnlyList<int>? result = null;
        Assert.DoesNotThrowAsync(async () =>
        {
            await enumerable.FetchNextPageAsync();
            await enumerable.FetchNextPageAsync();
            await enumerable.FetchNextPageAsync();

            result = await enumerable.FetchPreviousPageAsync();
        });

        // Assert
        Assert.That(result, Is.EquivalentTo(Items[PageSize..(PageSize + PageSize)]));

        // Output
        string json = JsonSerializer.Serialize(result);
        TestContext.Out.WriteLine("Items: {0}", json);
    }
}