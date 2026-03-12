using Acornima.Ast;
using Jint;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Web;
using YouTubeMusicAPI.Internal.JavaScript;

namespace YouTubeMusicAPI.Internal;

internal class Player(
    Prepared<Script> javaScript,
    int signatureTimestamp)
{
    readonly Prepared<Script> javaScript = javaScript;

    readonly ConcurrentBag<Engine> enginePool = [];
    readonly int enginePoolMaxSize = 5;


    Engine GetEngine()
    {
        if (enginePool.TryTake(out Engine engine))
            return engine;

        return new Engine().Execute(javaScript);
    }

    void ReturnEngine(
        Engine engine)
    {
        if (enginePool.Count >= enginePoolMaxSize)
        {
            engine.Dispose();
            return;
        }

        enginePool.Add(engine);
    }


    /// <summary>
    /// The timestamp of the JavaScript signature.
    /// </summary>
    public int SignatureTimestamp { get; } = signatureTimestamp;


    /// <summary>
    /// Creates a new player
    /// </summary>
    /// <param name="requestHelper">The HTTP request helper</param>
    /// <param name="playerId">The ID of the player. Null to get the most recent YouTube player.</param>
    /// <param name="cancellationToken">The token to cancel this action</param>
    /// <returns>The player</returns>
    public static async Task<Player> CreateAsync(
        RequestHelper requestHelper,
        string? playerId,
        CancellationToken cancellationToken = default)
    {
        if (playerId is null)
        {
            string url = "https://www.youtube.com" + "/iframe_api";
            string js = await requestHelper.GetAndValidateAsync(url, null, cancellationToken);

            playerId = js.GetStringBetween(@"player\/", @"\/") ?? throw new("Failed to get player id");
        }

        string playerUrl = "https://www.youtube.com" + $"/s/player/{playerId}/player_ias.vflset/en_US/base.js";
        string playerJs = await requestHelper.GetAndValidateAsync(playerUrl, null, cancellationToken);

        JsAnalyzer analyzer = new(playerJs,
            [
                new("sigFunction", JsMatchers.Signature, true),
                new("nFunction", JsMatchers.NSignature, true),
                new("signatureTimestampVar", JsMatchers.SignatureTimestamp, false)
            ]);

        JsExtractor extractor = new(analyzer, isStrictMode: false, skipEmitFor: ["signatureTimestampVar"]);
        JsExtractor.Result result = extractor.BuildScript();

        if (!result.Exported.Contains("sigFunction"))
            throw new Exception("Failed to extract signature deciphering function");

        if (!result.Exported.Contains("nFunction"))
            throw new Exception("Failed to extract n-signature deciphering function");

        int signatureTimestamp = result.ExportedRawValues.TryGetValue("signatureTimestampVar", out object? rawSignatureTimestamp)
            ? Convert.ToInt32(rawSignatureTimestamp)
            : throw new Exception("Failed to extract signature timestamp");

        Prepared<Script> script = Engine.PrepareScript(result.Output);
        return new(script, signatureTimestamp);
    }


    /// <summary>
    /// Deciphers the signature cipher
    /// </summary>
    /// <param name="url">The url</param>
    /// <param name="signatureCipher">The signature cipher</param>
    /// <param name="cipher">The cipher</param>
    /// <param name="poToken">The Proof of Origin Token (required for SABR Requests)</param>
    /// <returns>The deciphered url</returns>
    public string Decipher(
        string? url,
        string? signatureCipher,
        string? cipher,
        string? poToken = null)
    {
        string actualUrl = url ?? signatureCipher ?? cipher ?? throw new("No url, signature cipher or cipher provided");

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
        if (sig is not null || nsig is not null)
        {
            Engine jsEngine = GetEngine();
            try
            {
                if (sig is not null)
                {
                    string decipheredSig = jsEngine.Evaluate($"exportedVars.sigFunction('{sig}');").AsString();
                    urlQuery[sp] = decipheredSig;
                }
                if (nsig is not null)
                {
                    string decipheredNSig = jsEngine.Evaluate($"exportedVars.nFunction('{nsig}');").AsString();
                    urlQuery["n"] = decipheredNSig;
                }
            }
            finally
            {
                ReturnEngine(jsEngine);
            }
        }

        // SABR Requests
        if (sabr != "1")
        {
            if (poToken is null)
                throw new Exception("Proof of Origin Token missing. SABR Request must include a PoToken.");
            urlQuery["pot"] = poToken;
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