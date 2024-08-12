﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using YouTubeMusicAPI.Internal;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Client;

/// <summary>
/// Client for searching on YouTube Music
/// </summary>
public class YouTubeMusicClient
{
    readonly ILogger? logger;
    readonly YouTubeMusicBase baseClient;

    readonly string geographicalLocation;

    /// <summary>
    /// Creates a new search client
    /// </summary>
    /// <param name="geographicalLocation">The region for the payload</param>
    public YouTubeMusicClient(
        string geographicalLocation = "US")
    {
        this.geographicalLocation = geographicalLocation;

        this.baseClient = new();

        logger?.LogInformation($"[YouTubeMusicClient-.ctor] YouTubeMusicClient has been initialized.");
    }

    /// <summary>
    /// Creates a new search client with extendended logging functions
    /// </summary>
    /// <param name="logger">The optional logger used for logging</param>
    /// <param name="geographicalLocation">The region for the payload</param>
    public YouTubeMusicClient(
        ILogger logger,
        string geographicalLocation = "US")
    {
        this.geographicalLocation = geographicalLocation;

        this.logger = logger;
        this.baseClient = new(logger);

        logger?.LogInformation($"[YouTubeMusicClient-.ctor] YouTubeMusicClient with extendended logging functions has been initialized.");
    }


    /// <summary>
    /// Gets the shelf kind based on the type of the shelf item
    /// </summary>
    /// <typeparam name="T">The requested shelf type</typeparam>
    /// <returns>The shelf kind</returns>
    ShelfKind GetShelfKind<T>() where T : IShelfItem =>
        typeof(T) switch
        {
            Type type when type == typeof(Song) => ShelfKind.Songs,
            Type type when type == typeof(Video) => ShelfKind.Videos,
            Type type when type == typeof(Album) => ShelfKind.Albums,
            Type type when type == typeof(CommunityPlaylist) => ShelfKind.CommunityPlaylists,
            Type type when type == typeof(Artist) => ShelfKind.Artists,
            Type type when type == typeof(Podcast) => ShelfKind.Podcasts,
            Type type when type == typeof(Episode) => ShelfKind.Episodes,
            Type type when type == typeof(Profile) => ShelfKind.Profiles,
            _ => ShelfKind.Unknown
        };


    /// <summary>
    /// Parses a search request response into shelf results
    /// </summary>
    /// <param name="requestResponse">The request response to parse</param>
    /// <returns>An array of shelves containing all items</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    IEnumerable<Shelf> ParseSearchResponse(
        JObject requestResponse)
    {
        List<Shelf> results = [];

        // Get shelves
        logger?.LogInformation($"[YouTubeMusicClient-ParseSearchResponse] Getting shelves from search response.");
        bool isContinued = requestResponse.ContainsKey("continuationContents");

        IEnumerable<JToken>? shelvesData = isContinued
            ? requestResponse
                .SelectToken("continuationContents")
            : requestResponse
                .SelectToken("contents.tabbedSearchResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents")
                ?.Where(token => token["musicShelfRenderer"] is not null)
                ?.Select(token => token.First!);

        if (shelvesData is null || !shelvesData.Any())
        {
            logger?.LogWarning($"[YouTubeMusicClient-ParseSearchResponse] Parsing search failed. Request response does not contain any shelves.");
            return [];
        }

        foreach (JToken? shelfData in shelvesData)
        {
            JToken? shelfDataObject = shelfData.First;
            if (shelfDataObject is null)
                continue;

            // Parse info from shelf data
            string? nextContinuationToken = shelfDataObject.SelectObjectOptional<string>("continuations[0].nextContinuationData.continuation");

            string? category = isContinued
                ? requestResponse
                    .SelectToken("header.musicHeaderRenderer.header.chipCloudRenderer.chips")
                    ?.FirstOrDefault(token => token.SelectObjectOptional<bool>("chipCloudChipRenderer.isSelected"))
                    ?.SelectObjectOptional<string>("chipCloudChipRenderer.uniqueId")
                : shelfDataObject
                    .SelectObjectOptional<string>("title.runs[0].text");
            JToken[] shelfItems = shelfDataObject.SelectObjectOptional<JToken[]>("contents") ?? [];

            ShelfKind kind = category.ToShelfKind();
            Func<JToken, IShelfItem>? getShelfItem = kind switch
            {
                ShelfKind.Songs => ShelfItemParser.GetSong,
                ShelfKind.Videos => ShelfItemParser.GetVideo,
                ShelfKind.Albums => ShelfItemParser.GetAlbums,
                ShelfKind.CommunityPlaylists => ShelfItemParser.GetCommunityPlaylist,
                ShelfKind.Artists => ShelfItemParser.GetArtist,
                ShelfKind.Podcasts => ShelfItemParser.GetPodcast,
                ShelfKind.Episodes => ShelfItemParser.GetEpisode,
                ShelfKind.Profiles => ShelfItemParser.GetProfile,
                _ => null
            };

            List<IShelfItem> items = [];
            if (getShelfItem is not null)
                foreach (JToken shelfItem in shelfItems)
                {
                    // Parse shelf item
                    JToken? itemObject = shelfItem.First?.First;

                    if (itemObject is null)
                        continue;

                    items.Add(getShelfItem(itemObject));
                }

            // Create shelf
            Shelf shelf = new(nextContinuationToken, [.. items], kind);
            results.Add(shelf);
        }

        return results;
    }



    /// <summary>
    /// Searches for a query on YouTube Music
    /// </summary>
    /// <param name="query">The query to search for</param>
    /// <param name="continuationToken">The continuation token to get further elemnts from a pervious search</param>
    /// <param name="kind">The shelf kind of items to search for</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>An array of shelves containing all search results</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<Shelf>> SearchAsync(
        string query,
        string? continuationToken = null,
        ShelfKind? kind = null,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(query))
        {
            logger?.LogError($"[YouTubeMusicClient-SearchAsync] Search failed. Query parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(query), "Search failed. Query parameter is null or whitespace.");
        }

        (string key, object? value)[] payload =
        [
            ("query", query),
            ("params", kind.ToParams()),
            ("continuation", continuationToken)
        ];

        // Send request
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Search, payload, "en", geographicalLocation, cancellationToken);

        // Parse request response
        IEnumerable<Shelf> searchResults = ParseSearchResponse(requestResponse);
        return kind is null || kind == ShelfKind.Unknown ? searchResults : searchResults.Where(searchResult => searchResult.Kind == kind);
    }

    /// <summary>
    /// Searches for a specfic shelf for a query on YouTube Music
    /// </summary>
    /// <param name="query">The query to search for</param>
    /// <param name="limit">The limit of items to return</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>An array of the specific shelf items</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<T>> SearchAsync<T>(
        string query,
        int limit = 20,
        CancellationToken cancellationToken = default) where T : IShelfItem
    {
        ShelfKind kind = GetShelfKind<T>();

        // All items requested (does not support continuation)
        if (kind == ShelfKind.Unknown)
        {
            IEnumerable<Shelf> allShelves = await SearchAsync(query, null, kind, cancellationToken);

            return allShelves
                .SelectMany(shelf => shelf.Items)
                .Take(limit)
                .Cast<T>();
        }

        // Special items requested
        List<T> searchResults = [];

        string? nextContinuationToken = null;
        bool hasMoreResults = true;

        while (searchResults.Count < limit && hasMoreResults)
        {
            IEnumerable<Shelf> currentShelf = await SearchAsync(query, nextContinuationToken, kind, cancellationToken);

            Shelf? requestedShelf = currentShelf.FirstOrDefault(shelf => shelf.Kind == kind);
            if (requestedShelf is null)
            {
                logger?.LogWarning($"[YouTubeMusicClient-SearchAsync] Search results do not cotain requested filtered shelf.");
                break;
            }

            searchResults.AddRange(requestedShelf.Items.Cast<T>());

            nextContinuationToken = requestedShelf.NextContinuationToken;
            hasMoreResults = !string.IsNullOrEmpty(nextContinuationToken);
        }

        return searchResults.Take(limit);
    }


    /// <summary>
    /// Gets the browse id for an album used for getting information
    /// </summary>
    /// <param name="id">The id of the album</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The browse id of the album</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<string> GetAlbumBrowseIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(id))
        {
            logger?.LogError($"[YouTubeMusicClient-GetAlbumBrowseIdAsync] Getting album browse id failed. Id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(id), "Getting album browse id failed. Id parameter is null or whitespace.");
        }

        if (id.StartsWith("MPRE"))
            return id;

        (string key, string? value)[] parameters =
        [
            ("list", id)
        ];

        // Send request
        string requestResponse = await baseClient.GetWebContentAsync(Endpoints.Playlist, parameters, cancellationToken);

        // Parse request response
        Match match = Regex.Match(Regex.Unescape(requestResponse), "\"MPRE.+?\"");
        if (!match.Success)
        {
            logger?.LogError($"[YouTubeMusicClient-GetAlbumBrowseIdAsync] Getting album browse id failed. Found no match.");
            throw new Exception("Getting album browse id failed. Found no match.");
        }

        return match.Value.Trim('"');
    }

    /// <summary>
    /// Gets the browse id for an playlist used for getting information
    /// </summary>
    /// <param name="id">The id of the playlist</param>
    /// <returns>The browse id of the playlist</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    public string GetCommunityPlaylistBrowseId(
        string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            logger?.LogError($"[YouTubeMusicClient-GetCommunityPlaylistBrowseId] Getting community playlist browse id failed. Id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(id), "Getting community playlist browse id failed. Id parameter is null or whitespace.");
        }

        if (id.StartsWith("VL"))
            return id;

        return $"VL{id}";
    }


    /// <summary>
    /// Gets the information about a song or video on YouTube Music
    /// </summary>
    /// <param name="id">The id of the song or video</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The song or video info</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<SongVideoInfo> GetSongVideoInfoAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(id))
        {
            logger?.LogError($"[YouTubeMusicClient-GetSongVideoInfoAsync] Getting info failed. Id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(id), "Getting info failed. Id parameter is null or whitespace.");
        }

        (string key, object? value)[] payload =
        [
            ("videoId", id)
        ];

        // Send request
        JObject playerRequestResponse = await baseClient.SendRequestAsync(Endpoints.Player, payload, "en", geographicalLocation, cancellationToken);
        JObject nextRequestResponse = await baseClient.SendRequestAsync(Endpoints.Next, payload, "en", geographicalLocation, cancellationToken);

        // Parse request response
        SongVideoInfo info = InfoParser.GetSongVideo(playerRequestResponse, nextRequestResponse);
        return info;
    }


    /// <summary>
    /// Gets the information about an album on YouTube Music
    /// </summary>
    /// <param name="browseId">The brwose id of the album</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The album info</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<AlbumInfo> GetAlbumInfoAsync(
        string browseId,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(browseId))
        {
            logger?.LogError($"[YouTubeMusicClient-GetAlbumInfoAsync] Getting info failed. Browse id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(browseId), "Getting info failed. Browse id parameter is null or whitespace.");
        }

        (string key, object? value)[] payload =
        [
            ("browseId", browseId)
        ];

        // Send request
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, "en", geographicalLocation, cancellationToken);

        // Parse request response
        AlbumInfo info = InfoParser.GetAlbum(requestResponse);
        return info;
    }

    /// <summary>
    /// Gets the information about a community playlist on YouTube Music
    /// </summary>
    /// <param name="browseId">The brwose id of the community playlist</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The community playlist info</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<CommunityPlaylistInfo> GetCommunityPlaylistInfoAsync(
        string browseId,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(browseId))
        {
            logger?.LogError($"[YouTubeMusicClient-GetCommunityPlaylistInfoAsync] Getting info failed. Browse id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(browseId), "Getting info failed. Browse id parameter is null or whitespace.");
        }

        (string key, object? value)[] payload =
        [
            ("browseId", browseId)
        ];

        // Send request
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, "en", geographicalLocation, cancellationToken);

        // Parse request response
        CommunityPlaylistInfo info = InfoParser.GetCommunityPlaylist(requestResponse);
        return info;
    }

    /// <summary>
    /// Gets the information about a community playlist on YouTube Music
    /// </summary>
    /// <param name="browseId">The brwose id of the community playlist</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The community playlist info</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<ArtistInfo> GetArtistInfoAsync(
        string browseId,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(browseId))
        {
            logger?.LogError($"[YouTubeMusicClient-GetArtistInfoAsync] Getting info failed. Browse id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(browseId), "Getting info failed. Browse id parameter is null or whitespace.");
        }

        (string key, object? value)[] payload =
        [
            ("browseId", browseId)
        ];

        // Send request
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, "en", geographicalLocation, cancellationToken);

        // Parse request response
        ArtistInfo info = InfoParser.GetArtist(requestResponse);
        return info;
    }
}