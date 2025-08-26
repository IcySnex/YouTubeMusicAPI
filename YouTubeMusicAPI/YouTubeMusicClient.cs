using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Services;
using YouTubeMusicAPI.Services.Sub;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI;

/// <summary>
/// Client for interacting with the YouTube Music API.
/// </summary>
public class YouTubeMusicClient
{
    internal readonly RequestHandler RequestHandler;
    internal readonly ILogger? Logger;

    /// <summary>
    /// Creates a new instance of the <see cref="YouTubeMusicClient"/> class.
    /// </summary>
    /// <param name="config">The configuration for this YouTube Music client</param>
    public YouTubeMusicClient(
        YouTubeMusicConfig? config = null)
    {
        Config = config ?? new();

        RequestHandler = new(Config.GeographicalLocation, Config.Authenticator, Config.HttpClient, Config.Logger);
        Logger = Config.Logger;

        RelationsService relations = new(this);
        LyricsService lyrics = new(this);

        Search = new(this);
        Songs = new(this, relations, lyrics);
        Videos = new(this, relations, lyrics);
        Playlists = new(this);
    }


    /// <summary>
    /// The configuration for this client.
    /// </summary>
    public YouTubeMusicConfig Config { get; }


    /// <summary>
    /// The service which handles searches on YouTube Music.
    /// </summary>
    public SearchService Search { get; }

    /// <summary>
    /// The service which handles getting information about songs from YouTube Music.
    /// </summary>
    public SongService Songs { get; }

    /// <summary>
    /// The service which handles getting information about videos from YouTube Music.
    /// </summary>
    public VideoService Videos { get; }

    /// <summary>
    /// The service which handles getting information about playlists from YouTube Music.
    /// </summary>
    public PlaylistsService Playlists { get; }


    /// <summary>
    /// Gets the currently authenticated user on YouTube Music.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <returns>An <see cref="AuthenticatedUser"/> if the user is authenticated, otherwise <see langword="null"/>.</returns>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<AuthenticatedUser?> GetAuthenticatedUserAsync(
        CancellationToken cancellationToken = default)
    {
        if (!RequestHandler.IsAuthenticated)
        {
            Config.Logger?.LogInformation("[YouTubeMusicClient-GetAuthenticatedUserAsync] Skipping request: Not authenticated.");
            return null;
        }

        // Send
        string response = await RequestHandler.PostAsync(Endpoints.AccountMenu, null, ClientType.WebMusic, cancellationToken);

        // Parse
        Logger?.LogInformation("[YouTubeMusicClient-GetAuthenticatedUserAsync] Parsing response...");
        using IDisposable _ = response.ParseJson(out JElement root);

        JElement menuRenderer = root
            .Get("actions")
            .GetAt(0)
            .Get("openPopupAction")
            .Get("popup")
            .Get("multiPageMenuRenderer");
        if (!menuRenderer.Contains("header"))
        {
            Logger?.LogInformation("[YouTubeMusicClient-GetAuthenticatedUserAsync] User not authenticated: No 'header' in 'multiPageMenuRenderer'.");
            return null;
        }

        AuthenticatedUser user = AuthenticatedUser.Parse(menuRenderer);
        return user;
    }
}