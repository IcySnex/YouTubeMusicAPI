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
    JsAlgorithms jsAlgorithms,
    string? poToken)
{
    public class JsAlgorithms(
        string sigDecipher,
        string nSigDecipher,
        int sigTimestamp)
    {
        public string SigDecipher { get; } = sigDecipher;

        public string NSigDecipher { get; } = nSigDecipher;

        public int SigTimestamp { get; } = sigTimestamp;
    }


    static JsAlgorithms ExtractJsAlgorithms(
        string playerJs)
    {
        JsAnalyzer ex = new(playerJs, [
            new("sigFunction", JsMatchers.Sig, true),
            new("nSigFunction", JsMatchers.NSig, true),
            new("sigTimestampVar", JsMatchers.SigTimestamp, false)
            ]);
        return null;

        Script ast = new Parser().ParseScript(playerJs);

        string? signature = null;
        string? nSignature = null;
        int? sigTimestamp = null;
        foreach (Node node in ast.DescendantNodes())
        {
            //if (TryParseSignature(node, playerJs, out string? signatureResult))
            //    signature = signatureResult;

            //else if (TryParseNSignature(node, playerJs, out string? nSignatureResult))
            //    nSignature = nSignatureResult;

            //else if (TryParseSigTimestamp(node, playerJs, out int? sigTimestampResult))
            //    sigTimestamp = sigTimestampResult;
        }

        if (signature is null)
            throw new Exception("Failed to extract signature");

        if (nSignature is null)
            throw new Exception("Failed to extract n signature");

        if (sigTimestamp is null)
            throw new("Failed to extract signature timestamp");

        return new(signature, nSignature, sigTimestamp.Value);
    }


    /// <summary>
    /// The JavaScript deciphering algorithms for this player
    /// </summary>
    public JsAlgorithms Algorithms { get; } = jsAlgorithms;

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
        JsAlgorithms algorithms = ExtractJsAlgorithms(playerJs);

        return new(algorithms, poToken);
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
            string decipheredSig = jsEngine.Evaluate(Algorithms.SigDecipher).AsString();
            urlQuery[sp] = decipheredSig;
        }
        if (nsig is not null)
        {
            string decipheredNSig = jsEngine.Evaluate(Algorithms.NSigDecipher).AsString();
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