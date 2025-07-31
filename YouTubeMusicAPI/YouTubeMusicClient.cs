using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Services;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI;

/// <summary>
/// Client for interacting with the YouTube Music API.
/// </summary>
public class YouTubeMusicClient
{
    readonly RequestHandler requestHandler;

    /// <summary>
    /// Creates a new instance of the <see cref="YouTubeMusicClient"/> class.
    /// </summary>
    /// <param name="config">The configuration for this YouTube Music client</param>
    public YouTubeMusicClient(
        YouTubeMusicConfig? config = null)
    {
        Config = config ?? new();

        requestHandler = new(Config.GeographicalLocation, Config.Authenticator, Config.HttpClient, Config.Logger);
        Search = new(requestHandler, Config.Logger);
        Info = new(requestHandler, Config.Logger);
    }


    /// <summary>
    /// The configuration for this YouTube Music client.
    /// </summary>
    public YouTubeMusicConfig Config { get; }


    /// <summary>
    /// The service used to search on YouTube Music.
    /// </summary>
    public SearchService Search { get; }

    /// <summary>
    /// The service used to get information from YouTube Music.
    /// </summary>
    public InfoService Info { get; }


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
        if (!requestHandler.IsAuthenticated)
        {
            Config.Logger?.LogInformation("[YouTubeMusicClient-GetAuthenticatedUserAsync] Could not get authenticated user: RequestHandler not authenticated.");
            return null;
        }

        // Send
        string response = await requestHandler.PostAsync(Endpoints.AccountMenu, null, ClientType.WebMusic, cancellationToken);

        // Parse
        using JsonDocument json = JsonDocument.Parse(response);
        JsonElement rootElement = json.RootElement;

        JsonElement menuRenderer = rootElement
            .GetProperty("actions")
            .GetElementAt(0)
            .GetProperty("openPopupAction")
            .GetProperty("popup")
            .GetProperty("multiPageMenuRenderer");

        if (!menuRenderer.TryGetProperty("header", out _))
        {
            Config.Logger?.LogInformation("[YouTubeMusicClient-GetAuthenticatedUserAsync] Could not get authenticated user: User not logged in.");
            return null;
        }

        AuthenticatedUser user = AuthenticatedUser.Parse(menuRenderer);
        return user;
    }
}