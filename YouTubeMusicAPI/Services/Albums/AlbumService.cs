﻿using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Services.Playlists;
using YouTubeMusicAPI.Services.Relations;
using YouTubeMusicAPI.Services.Search;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Albums;

/// <summary>
/// Service which handles getting information about albums from YouTube Music.
/// </summary>
public sealed partial class AlbumService
{
    [GeneratedRegex("\"MPRE.+?\"")]
    private static partial Regex BrowseIdRegex();


    readonly YouTubeMusicClient client;

    /// <summary>
    /// Creates a new instance of the <see cref="AlbumService"/> class.
    /// </summary>
    /// <param name="client">The shared base client.</param>
    internal AlbumService(
        YouTubeMusicClient client)
    {
        this.client = client;
    }


    /// <summary>
    /// Creates a paginator that searches for albums on YouTube Music.
    /// </summary>
    /// <remarks>
    /// Convenience method that forwards to <see cref="SearchService.ByCategoryAsync{T}(string, SearchScope, bool)"/>.
    /// </remarks>
    /// <param name="query">The query to search for.</param>
    /// <param name="scope">The scope of the search.</param>
    /// <param name="ignoreSpelling">Weither to ignore spelling suggestions.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="AlbumSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<AlbumSearchResult> SearchAsync(
        string query,
        SearchScope scope = SearchScope.Global,
        bool ignoreSpelling = true) =>
        client.Search.ByCategoryAsync<AlbumSearchResult>(query, scope, ignoreSpelling);


    /// <summary>
    /// Gets the browse ID for a album on YouTube Music.
    /// </summary>
    /// <param name="id">The id of the album.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The browse ID of the album.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>id</c> is <see langword="null"/> or empty.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<string> GetBrowseIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));

        if (id.StartsWith("MPRE"))
            return id;

        // Send request
        string url = Endpoints.Playlists
            .SetQueryParameter("list", id);

        string response = await client.RequestHandler.GetAsync(url, null, ClientType.None, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[AlbumService-GetBrowseIdAsync] Parsing response...");
        Match match = BrowseIdRegex().Match(Regex.Unescape(response));

        if (!match.Success)
        {
            client.Logger?.LogError("[AlbumService-GetBrowseIdAsync] Found no Regex match for browse ID: '\\\"MPRE.+?\\\"'.");
            throw new FormatException("Found no Regex match for browse ID: '\\\"MPRE.+?\\\"'.");
        }

        string browseId = match.Value.Trim('"');
        return browseId;
    }


    /// <summary>
    /// Gets detailed information about a album on YouTube Music.
    /// </summary>
    /// <param name="browseId">The browse ID of the album.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="AlbumInfo"/> containing the information.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>browseId</c> is <see langword="null"/> or empty or it is not a valid browse ID.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<AlbumInfo> GetAsync(
        string browseId,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(browseId, nameof(browseId));
        Ensure.StartsWith(browseId, "MPRE", nameof(browseId));

        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("browseId", browseId),
        ];

        string response = await client.RequestHandler.PostAsync(Endpoints.Browse, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[AlbumService-GetAsync] Parsing response...");
        using IDisposable _ = response.ParseJson(out JElement root);

        AlbumInfo album = AlbumInfo.Parse(root);
        return album;
    }


    /// <summary>
    /// Gets the related content for the album on YouTube Music.
    /// </summary>
    /// <remarks>
    /// This method does not perform any network operations. It is pseudo-async to preserve consistency with similar methods in the library, e.g. <see cref="PlaylistService.GetRelationsAsync(PlaylistInfo, CancellationToken)"/>.
    /// </remarks>
    /// <param name="album">The album to get the relations for.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="AlbumRelations"/> containing the related content for the album.</returns>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Preserve consistency with similar methods in the library")]
    public Task<AlbumRelations> GetRelationsAsync(
        AlbumInfo album,
        CancellationToken cancellationToken = default)
    {
        // Getting relations
        client.Logger?.LogInformation("[AlbumService-GetRelationsAsync] Getting relations...");

        AlbumRelations relations = album.Relations;
        return Task.FromResult(relations);
    }
}