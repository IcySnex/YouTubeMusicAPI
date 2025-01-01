using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using YouTubeMusicAPI.Internal;

namespace YouTubeMusicAPI.Client;

/// <summary>
/// Client for all low level YouTube Music API calls
/// </summary>
public class YouTubeMusicBase
{
    readonly ILogger? logger;
    readonly RequestHelper requestHelper;

    /// <summary>
    /// Creates a new base client
    /// </summary>
    /// <param name="cookies">Initial cookies used for authentication</param>
    public YouTubeMusicBase(
        IEnumerable<Cookie>? cookies = null)
    {
        this.requestHelper = new(cookies);
    }

    /// <summary>
    /// Creates a new base client with extendended logging functions
    /// </summary>
    /// <param name="logger">The optional logger used for logging</param>
    /// <param name="cookies">Initial cookies used for authentication</param>
    public YouTubeMusicBase(
        ILogger logger,
        IEnumerable<Cookie>? cookies = null)
    {
        this.logger = logger;
        this.requestHelper = new(cookies);

        logger.LogInformation($"[BaseClient-.ctor] BaseClient with with extendended logging functions has been initialized.");
    }


    /// <summary>
    /// Gets the web content of the given YouTube Music API endpoint with parameter queries
    /// </summary>
    /// <param name="apiEndpoint">The specific API endpoint for the request</param>
    /// <param name="parameterItems">The items to add to the query parameters</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    /// <returns>The json object containing the response data</returns>
    public async Task<string> GetWebContentAsync(
        string apiEndpoint,
        (string key, string? value)[] parameterItems,
        CancellationToken cancellationToken = default)
    {
        string url = Endpoints.MusicWebUrl + apiEndpoint;

        // Create parameters
        logger?.LogInformation($"[YouTubeMusicBase-GetWebContentAsync] Creating parameters.");
        NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
        foreach ((string key, string? value) in parameterItems)
            if (value is not null)
                parameters[key] = value;

        // Send request
        string response = await requestHelper.GetAndValidateAsync(url, parameters.ToString(), cancellationToken);

        return response;
    }

    /// <summary>
    /// Sends a new request to the YouTube Music API with a payload
    /// </summary>
    /// <param name="apiEndpoint">The specific API endpoint for the request</param>
    /// <param name="payload">The request payload</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    /// <returns>The json object containing the response data</returns>
    public async Task<JObject> SendRequestAsync(
        string apiEndpoint,
        Dictionary<string, object> payload,
        CancellationToken cancellationToken = default)
    {
        string url = Endpoints.MusicApiUrl + apiEndpoint;

        // Send request
        string response = await requestHelper.PostAndValidateAsync(url, payload, null, cancellationToken);

        // Parsing response
        logger?.LogInformation($"[YouTubeMusicBase-SendRequestAsync] Parsing response.");
        return JObject.Parse(response);
    }


    // FOR SOME REASON YOUTUBE DOESNT WANT TO ACCEPT MY DECYPHERED SIGNATURE
    // 403 FORBIDDEN ???
    //
    ///// <summary>
    ///// Parses a player for decyphering signatures
    ///// </summary>
    ///// <param name="cancellationToken">The token to cancel this action</param>
    ///// <returns>The player signature timestamp and operations list</returns>
    ///// <exception cref="Exception"></exception>
    //public async Task<(string signatureTimestamp, List<Func<string, string>> operations)> ParsePlayerAsync(
    //    CancellationToken cancellationToken = default)
    //{
    //    // iframe
    //    string iframeUrl = Endpoints.YouTubeWebUrl + Endpoints.Iframe;
    //    string iframe = await requestHelper.GetAndValidateAsync(iframeUrl, null, cancellationToken);

    //    // Player
    //    string playerVersion = Regex.Match(iframe, @"player\\?/([0-9a-fA-F]{8})\\?/").Groups[1].Value;
    //    string playerSourceUrl = Endpoints.YouTubeWebUrl + Endpoints.PlayerSource(playerVersion);
    //    string playerSource = await requestHelper.GetAndValidateAsync(playerSourceUrl, null, cancellationToken);

    //    // Signature
    //    string signatureTimestamp = Regex.Match(playerSource, @"(?:signatureTimestamp|sts):(\d{5})").Groups[1].Value;

    //    if (string.IsNullOrWhiteSpace(signatureTimestamp))
    //        throw new Exception("Failed to find signature timestamp in player source");

    //    // Cipher
    //    string cipherCallsite = Regex.Match(playerSource,
    //        """
    //        [$_\w]+=function\([$_\w]+\){([$_\w]+)=\1\.split\(['"]{2}\);.*?return \1\.join\(['"]{2}\)}
    //        """,
    //                RegexOptions.Singleline).Groups[0].Value;
    //    if (string.IsNullOrWhiteSpace(cipherCallsite))
    //        throw new Exception("Failed to find cipher callsite in player source");

    //    string cipherContainerName = Regex.Match(cipherCallsite, @"([$_\w]+)\.[$_\w]+\([$_\w]+,\d+\);").Groups[1].Value;
    //    if (string.IsNullOrWhiteSpace(cipherContainerName))
    //        throw new Exception("Failed to find cipher container name in player source");

    //    string cipherDefinition = Regex.Match(playerSource,
    //        $$"""
    //        var {{Regex.Escape(cipherContainerName)}}={.*?};
    //        """,
    //        RegexOptions.Singleline).Groups[0].Value;
    //    if (string.IsNullOrWhiteSpace(cipherDefinition))
    //        throw new Exception("Failed to find cipher definition in player source");

    //    // Functions
    //    string swapFuncName = Regex.Match(cipherDefinition, @"([$_\w]+):function\([$_\w]+,[$_\w]+\){+[^}]*?%[^}]*?}", RegexOptions.Singleline).Groups[1].Value;
    //    string spliceFuncName = Regex.Match(cipherDefinition, @"([$_\w]+):function\([$_\w]+,[$_\w]+\){+[^}]*?splice[^}]*?}", RegexOptions.Singleline).Groups[1].Value;
    //    string reverseFuncName = Regex.Match(cipherDefinition, @"([$_\w]+):function\([$_\w]+\){+[^}]*?reverse[^}]*?}", RegexOptions.Singleline).Groups[1].Value;

    //    // Operations
    //    List<Func<string, string>> operations = new();
    //    foreach (string statement in cipherCallsite.Split(';'))
    //    {
    //        string calledFuncName = Regex.Match(statement, @"[$_\w]+\.([$_\w]+)\([$_\w]+,\d+\)").Groups[1].Value;
    //        if (string.IsNullOrWhiteSpace(calledFuncName))
    //            continue;

    //        if (string.Equals(calledFuncName, swapFuncName, StringComparison.Ordinal))
    //        {
    //            int index = int.Parse(Regex.Match(statement, @"\([$_\w]+,(\d+)\)").Groups[1].Value);
    //            operations.Add(s => new StringBuilder(s)
    //                {
    //                    [0] = s[index],
    //                    [index] = s[0],
    //                }.ToString());
    //        }
    //        else if (string.Equals(calledFuncName, spliceFuncName, StringComparison.Ordinal))
    //        {
    //            int index = int.Parse(Regex.Match(statement, @"\([$_\w]+,(\d+)\)").Groups[1].Value);
    //            operations.Add(s => s.Substring(index));
    //        }
    //        else if (string.Equals(calledFuncName, reverseFuncName, StringComparison.Ordinal))
    //        {
    //            operations.Add(s =>
    //            {
    //                s.Reverse();
    //                return s;
    //            });
    //        }
    //    }

    //    return (signatureTimestamp, operations);
    //}
}