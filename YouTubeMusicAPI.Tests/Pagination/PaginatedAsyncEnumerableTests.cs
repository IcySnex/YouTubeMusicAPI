using System.Threading.Tasks;
using YouTubeMusicAPI.Pagination;

namespace YouTubeMusicAPI.Tests.Pagination;

public class PaginatedAsyncEnumerableTests
{
    static readonly int PageSize = 10;

    static readonly int[] Items = [.. Enumerable.Range(1, 100)];

    static readonly Page<int> CashedFirstPage = new([.. Enumerable.Range(1, 10)], "10");

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

        TestData.WriteResult(result);
    }

    [Test]
    public void Should_enumerate_all_items_with_cashed_first_page()
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync, CashedFirstPage);

        // Act
        List<int> result = [];
        Assert.DoesNotThrowAsync(async () =>
        {
            await foreach (int item in enumerable)
                result.Add(item);
        });

        // Assert
        Assert.That(result, Is.EquivalentTo(Items));

        TestData.WriteResult(result);
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

        TestData.WriteResult(result);
    }

    [Test]
    [TestCase(0, null)]
    [TestCase(0, 25)]
    [TestCase(25, 25)]
    [TestCase(50, null)]
    public void Should_fetch_range_of_items_with_cashed_first_page(
        int offset,
        int? limit)
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync, CashedFirstPage);

        // Act
        IReadOnlyList<int>? result = null;
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await enumerable.FetchItemsAsync(offset, limit);
        });

        // Assert
        Assert.That(result, Is.EquivalentTo(Items[offset..(offset + limit ?? Items.Length)]));

        TestData.WriteResult(result);
    }


    [Test]
    public void Should_fetch_next_page()
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync);

        // Act
        bool result = false;
        Assert.DoesNotThrowAsync(async () =>
        {
            result = await enumerable.FetchNextPageAsync();
        });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(enumerable.CurrentPage, Is.EquivalentTo(Items[..PageSize]));
        });

        TestData.WriteResult(enumerable.CurrentPage);
    }

    [Test]
    public void Should_fetch_next_next_page()
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync);

        // Act
        bool result = false;
        Assert.DoesNotThrowAsync(async () =>
        {
            await enumerable.FetchNextPageAsync();

            result = await enumerable.FetchNextPageAsync();
        });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(enumerable.CurrentPage, Is.EquivalentTo(Items[PageSize..(2 * PageSize)]));
        });

        TestData.WriteResult(enumerable.CurrentPage);
    }


    [Test]
    public void Should_fetch_previous_page()
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync);

        // Act
        bool result = false;
        Assert.DoesNotThrowAsync(async () =>
        {
            await enumerable.FetchNextPageAsync();
            await enumerable.FetchNextPageAsync();

            result = await enumerable.FetchPreviousPageAsync();
        });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(enumerable.CurrentPage, Is.EquivalentTo(Items[..PageSize]));
        });

        TestData.WriteResult(enumerable.CurrentPage);
    }

    [Test]
    public void Should_fetch_previous_previous_page()
    {
        // Arrange
        PaginatedAsyncEnumerable<int> enumerable = new(FetchPageAsync);

        // Act
        bool result = false;
        Assert.DoesNotThrowAsync(async () =>
        {
            await enumerable.FetchNextPageAsync();
            await enumerable.FetchNextPageAsync();
            await enumerable.FetchNextPageAsync();

            await enumerable.FetchPreviousPageAsync();
            result = await enumerable.FetchPreviousPageAsync();
        });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(enumerable.CurrentPage, Is.EquivalentTo(Items[..PageSize]));
        });


        TestData.WriteResult(enumerable.CurrentPage);
    }
}