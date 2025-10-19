using Acornima;
using Acornima.Ast;
using Jint;
using System.Collections.Specialized;
using System.Web;
using System.Xml.Linq;
using YouTubeMusicAPI.Internal.JavaScript;
using static YouTubeMusicAPI.Internal.Player;

namespace YouTubeMusicAPI.Internal;

internal class Player(
    JsExtractor.Result javasScript,
    int signatureTimestamp,
    string? poToken)
{
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
        // Player
        //string url = "https://www.youtube.com" + "/iframe_api";
        //string js = await requestHelper.GetAndValidateAsync(url, null, cancellationToken);

        //string playerId = "bcd893b3";//js.GetStringBetween(@"player\/", @"\/") ?? throw new Exception("Failed to get player id");
        //string playerUrl = "https://www.youtube.com" + $"/s/player/{playerId}/player_ias.vflset/en_US/base.js";

        //File.WriteAllText("C:\\Users\\Kevin\\Desktop\\player.js", await requestHelper.GetAndValidateAsync(playerUrl, null, cancellationToken));

        string playerJs = File.ReadAllText("C:\\Users\\Kevin\\Desktop\\player.js");//await requestHelper.GetAndValidateAsync(playerUrl, null, cancellationToken);

        JsAnalyzer analyzer = new(playerJs,
            [
                new("sigFunction", JsMatchers.Sig, true),
                new("nSigFunction", JsMatchers.NSig, true),
                new("sigTimestampVar", JsMatchers.SigTimestamp, false)
            ]);

        JsExtractor extractor = new(analyzer, skipEmitFor: ["sigTimestampVar"]);
        JsExtractor.Result result = extractor.BuildScript();

        if (!result.Exported.Contains("sigFunction"))
            throw new Exception("Failed to extract signature function");

        if (!result.Exported.Contains("nSigFunction"))
            throw new Exception("Failed to extract n signature function");

        int sigTimestamp = result.ExportedRawValues.TryGetValue("sigTimestampVar", out object? sigTimestampVar)
            ? Convert.ToInt32(sigTimestampVar)
            : throw new Exception("Failed to extract signature timestamp");

        return new(result, sigTimestamp, poToken);
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
        Engine jsEngine = new Engine()
            .SetValue("sig", sig)
            .SetValue("nsig", nsig);

        if (sig is not null)
        {
            string decipheredSig = jsEngine.Evaluate("").AsString();
            urlQuery[sp] = decipheredSig;
        }
        if (nsig is not null)
        {
            string decipheredNSig = jsEngine.Evaluate("").AsString();
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
            "WEB_REMIX" => "1.20250331.01.00",
            _ => throw new Exception("This client is not supported"),
        };

        // Result
        string result = urlQuery.ToString() ?? throw new Exception("Failed to build url with deciphered signature");
        return result;
    }
}