using Acornima;
using Acornima.Ast;
using Jint;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;

namespace YouTubeMusicAPI.Internal;

internal class Player
{
    /// <summary>
    /// Gets the string between two strings
    /// </summary>
    /// <param name="source">The source string</param>
    /// <param name="start">The string for the start index</param>
    /// <param name="end">The string for the end index</param>
    /// <returns>The string</returns>
    static string? GetStringBetween(
        string source,
        string start,
        string end)
    {
        int startIndex = source.IndexOf(start);
        if (startIndex == -1)
            return null;

        int endIndex = source.IndexOf(end, startIndex + start.Length);
        if (endIndex == -1)
            return null;

        return source.Substring(startIndex + start.Length, endIndex - startIndex - start.Length);
    }


    /// <summary>
    /// Extracts the signature deciphering algorithm from the player
    /// </summary>
    /// <param name="playerJs">The player javascript source</param>
    /// <returns>The signature deciphering algorithm</returns>
    static string ExtractSigDecipherAlgorithm(
        string playerJs)
    {
        Match match = Regex.Match(playerJs, @"function\((\w+)\)\{(\1=\1\.split\([\'""]{2}\);(.*?)return\s*?\1\.join\([\'""]{2}\))};");
        if (!match.Success)
            throw new Exception("Failed to extract signature deciphering algorithm");

        string varName = match.Groups[1].Value;
        string? objName = match.Groups[3].Value.Split('.').FirstOrDefault();
        string? functions = GetStringBetween(playerJs, $"var {objName}={{", "};");

        if (string.IsNullOrEmpty(varName) ||
            string.IsNullOrEmpty(objName) ||
            string.IsNullOrEmpty(functions))
            throw new Exception("Failed to extract signature deciphering algorithm");

        return $"function descramble_sig({varName}) {{ let {objName}={{{functions}}}; {match.Groups[2].Value} }} descramble_sig(sig);";

    }

    /// <summary>
    /// Extracts the n-signature deciphering algorithm from the player
    /// </summary>
    /// <param name="playerJs">The player javascript source</param>
    /// <returns>The n-signature deciphering algorithm</returns>
    static string ExtractNSigDecipherAlgorithm(
        string playerJs)
    {
        Parser parser = new();
        Script ast = parser.ParseScript(playerJs);

        foreach (Node node in ast.DescendantNodesAndSelf())
        {
            if (node is not ExpressionStatement statement ||
                statement.Expression is not AssignmentExpression assignment ||
                assignment.Left is not Identifier identifier ||
                assignment.Right is not FunctionExpression)
                continue;

            string code = playerJs.Substring(node.Start, node.End - node.Start);
            if (!code.Contains("enhanced_except") &&
                !code.Contains("-_w8_") &&
                !code.Contains("1969"))
                continue;

            string fixedCode = Regex.Replace(code, @"if\s*\(\s*typeof\s*[a-zA-Z0-9_$]*\s*===\s*""*undefined""*\s*\)\s*return\s+[a-zA-Z0-9_$]*;", "");
            return $"{fixedCode} {identifier.Name}(nsig);";
        }

        throw new Exception("Could not find n-signature deciphering algorithm");
    }

    /// <summary>
    /// Extracts the signature timestamp from the player
    /// </summary>
    /// <param name="playerJs">The player javascript source</param>
    /// <returns>The signature timestamp</returns>
    static int ExtractSigTimestamp(
        string playerJs)
    {
        string? sigTimestamp = GetStringBetween(playerJs, "signatureTimestamp:", "}");
        return sigTimestamp is null ? 0 : int.Parse(sigTimestamp);
    }


    readonly string sigDecipherAlgorithm;
    readonly string nSigDecipherAlgorithm;

    Player(
        string sigDecipherAlgorithm,
        string nSigDecipherAlgorithm,
        int sigTimestamp,
        string? poToken)
    {
        this.sigDecipherAlgorithm = sigDecipherAlgorithm;
        this.nSigDecipherAlgorithm = nSigDecipherAlgorithm;

        SigTimestamp = sigTimestamp;
        PoToken = poToken;
    }


    /// <summary>
    /// The signature timestamp for this player
    /// </summary>
    public int SigTimestamp { get; }

    /// <summary>
    /// The Proof of Origin Token (required for SABR Requests)
    /// </summary>
    public string? PoToken { get; set; }


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
        string url = "https://www.youtube.com" + "/iframe_api";
        string js = await requestHelper.GetAndValidateAsync(url, null, cancellationToken);

        string playerId = GetStringBetween(js, @"player\/", @"\/") ?? throw new Exception("Failed to get player id");

        string playerUrl = "https://www.youtube.com" + $"/s/player/{playerId}/player_ias.vflset/en_US/base.js";
        string playerJs = await requestHelper.GetAndValidateAsync(playerUrl, null, cancellationToken);

        string sigDecipherAlgorithm = ExtractSigDecipherAlgorithm(playerJs);
        string nSigDecipherAlgorithm = ExtractNSigDecipherAlgorithm(playerJs);
        int sigTimestamp = ExtractSigTimestamp(playerJs);

        return new(sigDecipherAlgorithm, nSigDecipherAlgorithm, sigTimestamp, poToken);
    }


    /// <summary>
    /// Deciphers the signature cipher
    /// </summary>
    /// <param name="signatureCipher">The signature cipher</param>
    /// <returns>The deciphered url</returns>
    public string Decipher(
        string signatureCipher)
    {
        // Parse
        NameValueCollection query = HttpUtility.ParseQueryString(signatureCipher);
        string url = query["url"] ?? throw new Exception("Signature cipher does not contain url parameter");
        string sig = query["s"] ?? throw new Exception("Signature cipher does not contain s parameter");
        string sp = query["sp"] ?? "signature";

        NameValueCollection urlQuery = HttpUtility.ParseQueryString(url);
        string? nsig = urlQuery["n"];
        string? sabr = urlQuery["sabr"];
        string? client = urlQuery["c"];

        // Signatures
        Engine jsEngine = new Engine()
            .SetValue("sig", sig)
            .SetValue("nsig", nsig);

        string decipheredSig = jsEngine.Evaluate(sigDecipherAlgorithm).AsString();
        urlQuery[sp] = decipheredSig;

        if (nsig is not null)
        {
            string decipheredNsig = jsEngine.Evaluate(nSigDecipherAlgorithm).AsString();
            urlQuery["n"] = decipheredNsig;
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
            "WEB_REMIX" => "1.20211213.00.00",
            _ => throw new Exception("This client is not supported"),
        };

        // Result
        string result = urlQuery.ToString() ?? throw new Exception("Failed to build url with deciphered signature");
        return result;
    }
}