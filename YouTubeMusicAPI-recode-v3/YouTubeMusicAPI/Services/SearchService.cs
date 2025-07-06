using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services;

/// <summary>
/// Service used to search on YouTube Music.
/// </summary>
public sealed class SearchService : YouTubeMusicService
{
    // Query Params
    const string QueryParamsSongs = "EgWKAQIIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D";
    const string QueryParamsVideos = "EgWKAQIQAWoQEAMQBBAJEAoQBRAREBAQFQ%3D%3D";
    const string QueryParamsAlbums = "EgWKAQIYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D";
    const string QueryParamsCommunityPlaylists = "EgeKAQQoAEABahAQAxAKEAkQBBAFEBEQEBAV";
    const string QueryParamsArtists = "EgWKAQIgAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D";
    const string QueryParamsPodcasts = "EgWKAQJQAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D";
    const string QueryParamsEpisodes = "EgWKAQJIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D";
    const string QueryParamsProfiles = "EgWKAQJYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D";


    /// <summary>
    /// Initializes a new instance of the <see cref="SearchService"/> class.
    /// </summary>
    /// <param name="requestHandler"></param>
    /// <param name="logger">The logger used to provide progress and error messages.</param>
    internal SearchService(
        RequestHandler requestHandler,
        ILogger? logger = null) : base(requestHandler, logger) { }

    /// <summary>
    /// Searches for songs on YouTube Music.
    /// </summary>
    /// <param name="query">The search query used to find songs.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="SongSearchResult"/>'s.</returns>
    public PaginatedAsyncEnumerable<SongSearchResult> SongsAsync(
        string query)
    {
        Ensure.NotNullOrEmpty(query, nameof(query));

        async Task<Page<SongSearchResult>> FetchPageAsync(
            string? nextContinuationToken,
            CancellationToken cancellationToken = default)
        {
            // Send request
            KeyValuePair<string, object?>[] payload =
            [
                new("query", query),
                new("params", QueryParamsSongs),
                new("continuation", nextContinuationToken)
            ];

            string response = await requestHandler.PostAsync(Endpoints.Search, payload, ClientType.WebMusic, cancellationToken);

            // Parse response
            JsonElement rootElement = JsonDocument.Parse(response).RootElement;

            bool isContinued = rootElement.TryGetProperty("continuationContents", out JsonElement continuationContents);

            JsonElement shelf = isContinued
                ? continuationContents
                    .GetProperty("musicShelfContinuation")
                : rootElement
                    .GetProperty("contents")
                    .GetProperty("tabbedSearchResultsRenderer")
                    .GetProperty("tabs")
                    .GetElementAt(0)
                    .GetProperty("tabRenderer")
                    .GetProperty("content")
                    .GetProperty("sectionListRenderer")
                    .GetProperty("contents")
                    .EnumerateArray()
                    .First(content =>
                    {
                        if (!content.TryGetProperty("musicShelfRenderer", out JsonElement shelf))
                            return false;

                        string category = shelf
                            .GetProperty("title")
                            .GetProperty("runs")
                            .GetElementAt(0)
                            .GetProperty("text")
                            .GetStringOrEmpty();

                        return category == "Songs";
                    })
                    .GetProperty("musicShelfRenderer");

            string? continuationToken = shelf
                    .GetPropertyOrNull("continuations")
                    ?.GetElementAtOrNull(0)
                    ?.GetPropertyOrNull("nextContinuationData")
                    ?.GetPropertyOrNull("continuation")
                    ?.GetString();

            List<SongSearchResult> result = [];
            foreach (JsonElement item in shelf.GetProperty("contents").EnumerateArray())
            {
                SongSearchResult song = SongSearchResult.Parse(item);
                result.Add(song);
            }

            return new(result, continuationToken);
        }
        return new(FetchPageAsync);
    }
}