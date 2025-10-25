using Jint;
using System.Collections.Specialized;
using System.Web;
using YouTubeMusicAPI.Internal.JavaScript;

namespace YouTubeMusicAPI.Internal;

internal class Player(
    JsExtractor.Result javasScript,
    int signatureTimestamp,
    string? poToken)
{
    readonly Engine jsEngine = new Engine().Execute(javasScript.Output);



    /// <summary>
    /// The JavaScript extraction result.
    /// </summary>
    public JsExtractor.Result JavaScript { get; } = javasScript;

    /// <summary>
    /// The timestamp of the JavaScript signature.
    /// </summary>
    public int SignatureTimestamp { get; } = signatureTimestamp;

    /// <summary>
    /// The Proof of Origin Token (required for SABR Requests)
    /// </summary>
    public string? PoToken { get; set; } = poToken;


    /// <summary>
    /// Creates a new player
    /// </summary>
    /// <param name="requestHelper">The HTTP request helper</param>
    /// <param name="poToken">The Proof of Origin Token (required for SABR Requests)</param>
    /// <param name="cancellationToken">The token to cancel this action</param>
    /// <returns>The player</returns>
    public static async Task<Player> CreateAsync(
        RequestHelper requestHelper,
        string? poToken,
        CancellationToken cancellationToken = default)
    {
        string url = "https://www.youtube.com" + "/iframe_api";
        string js = await requestHelper.GetAndValidateAsync(url, null, cancellationToken);

        string playerId = js.GetStringBetween(@"player\/", @"\/") ?? throw new Exception("Failed to get player id");
        string playerUrl = "https://www.youtube.com" + $"/s/player/{playerId}/player_ias.vflset/en_US/base.js";

        string playerJs = await requestHelper.GetAndValidateAsync(playerUrl, null, cancellationToken);

        JsAnalyzer analyzer = new(playerJs,
            [
                new("decipherSignature", JsMatchers.Signature, true),
                new("decipherNSignature", JsMatchers.NSignature, true),
                new("signatureTimestamp", JsMatchers.SignatureTimestamp, false)
            ]);

        JsExtractor extractor = new(analyzer, skipEmitFor: ["signatureTimestamp"]);
        JsExtractor.Result result = extractor.BuildScript();

        if (!result.Exported.Contains("decipherSignature"))
            throw new Exception("Failed to extract signature deciphering function");

        if (!result.Exported.Contains("decipherNSignature"))
            throw new Exception("Failed to extract n-signature deciphering function");

        int signatureTimestamp = result.ExportedRawValues.TryGetValue("signatureTimestamp", out object? rawSignatureTimestamp)
            ? Convert.ToInt32(rawSignatureTimestamp)
            : throw new Exception("Failed to extract signature timestamp");

        return new(result, signatureTimestamp, poToken);
    }


    /// <summary>
    /// Deciphers the signature cipher
    /// </summary>
    /// <param name="url">The url</param>
    /// <param name="signatureCipher">The signature cipher</param>
    /// <param name="cipher">The cipher</param>
    /// <returns>The deciphered url</returns>
    public string Decipher(
        string? url,
        string? signatureCipher,
        string? cipher)
    {
        string actualUrl = url ?? signatureCipher ?? cipher ?? throw new Exception("No url, signature cipher or cipher provided");

        // Parse
        NameValueCollection query = HttpUtility.ParseQueryString(actualUrl);
        string extractedUrl = HttpUtility.UrlDecode(query["url"] ?? actualUrl);
        string? sig = HttpUtility.UrlDecode(query["s"]);
        string sp = query["sp"] ?? "signature";

        NameValueCollection urlQuery = HttpUtility.ParseQueryString(extractedUrl);
        string? nsig = urlQuery["n"];
        string? sabr = urlQuery["sabr"];
        string? client = urlQuery["c"];

        if ((signatureCipher is not null || cipher is not null) && sig is null)
            throw new Exception("Signature cipher does not contain s parameter.");

        // Signatures
        if (sig is not null)
        {
            string decipheredSig = jsEngine.Evaluate($"exportedVars.decipherSignature('{sig}');").AsString();
            urlQuery[sp] = decipheredSig;
        }
        if (nsig is not null)
        {
            string decipheredNSig = jsEngine.Evaluate($"exportedVars.decipherNSignature('{nsig}');").AsString();
            urlQuery["n"] = decipheredNSig;
        }

        // SABR Requests
        if (sabr != "1")
        {
            if (PoToken is null)
                throw new Exception("Proof of Origin Token missing. SABR Request must include a PoToken.");
            urlQuery["pot"] = PoToken;
        }

        // Client Version
        urlQuery["cver"] = client switch
        {
            "WEB_REMIX" => "1.20251022.00.01",
            _ => throw new Exception("This client is not supported"),
        };

        // Result
        string result = urlQuery.ToString() ?? throw new Exception("Failed to build url with deciphered signature");
        return result;
    }
}