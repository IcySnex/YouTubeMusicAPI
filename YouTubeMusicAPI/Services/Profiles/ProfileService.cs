using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Services.Search;
using YouTubeMusicAPI.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YouTubeMusicAPI.Services.Profiles;

/// <summary>
/// Service which handles getting information about profiles from YouTube Music.
/// </summary>
public sealed class ProfileService
{
    readonly YouTubeMusicClient client;

    /// <summary>
    /// Creates a new instance of the <see cref="ProfileService"/> class.
    /// </summary>
    /// <param name="client">The shared base client.</param>
    internal ProfileService(
        YouTubeMusicClient client)
    {
        this.client = client;
    }


    /// <summary>
    /// Creates a paginator that searches for profiles on YouTube Music.
    /// </summary>
    /// <remarks>
    /// Convenience method that forwards to <see cref="SearchService.ByCategoryAsync{T}(string, SearchScope, bool)"/>.
    /// </remarks>
    /// <param name="query">The query to search for.</param>
    /// <param name="scope">The scope of the search.</param>
    /// <param name="ignoreSpelling">Weither to ignore spelling suggestions.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="ProfileSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<ProfileSearchResult> SearchAsync(
        string query,
        SearchScope scope = SearchScope.Global,
        bool ignoreSpelling = true) =>
        client.Search.ByCategoryAsync<ProfileSearchResult>(query, scope, ignoreSpelling);


    /// <summary>
    /// Gets detailed information about a profile on YouTube Music.
    /// </summary>
    /// <param name="browseId">The browse ID of the profile.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="ProfileInfo"/> containing the information.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>browseId</c> is <see langword="null"/> or empty or it is not a valid browse ID.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<ProfileInfo> GetAsync(
        string browseId,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(browseId, nameof(browseId));

        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("browseId", browseId),
        ];

        string response = await client.RequestHandler.PostAsync(Endpoints.Browse, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[ProfileService-GetAsync] Parsing response...");
        using IDisposable _ = response.ParseJson(out JElement root);

        bool isProfile = root
            .Get("header")
            .Contains("musicVisualHeaderRenderer");
        if (!isProfile)
        {
            client.Logger?.LogError("[ProfileService-GetAsync] The provided browse ID does not correspond to a profile. Use 'client.Artists.GetAsync(string browseId)' instead.");
            throw new InvalidOperationException("The provided browse ID does not correspond to a profile. Use 'client.Artists.GetAsync(string browseId)' instead.");
        }

        ProfileInfo profile = ProfileInfo.Parse(root);
        return profile;
    }
}