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
    /// Extracts the global variable from the player
    /// </summary>
    /// <param name="playerJs">The player javascript source</param>
    /// <param name="ast">The parsed player abstract syntax tree</param>
    /// <returns>The global variable</returns>
    static (string Source, string Name)? ExtractGlobalVariable(
        string playerJs,
        Script ast)
    {
        string[] patterns = ["-_w8_", "Untrusted URL{", "1969", "1970", "playerfallback"];

        foreach (Node node in ast.DescendantNodesAndSelf())
        {
            if (node is not VariableDeclaration variable ||
                variable.Declarations.FirstOrDefault()?.Id is not Identifier identifier ||
                variable.Declarations.FirstOrDefault()?.Init is null)
                continue;

            string code = playerJs.Substring(node.Start, node.End - node.Start);
            if (patterns.Any(marker => code.Contains(marker)))
                return (code, identifier.Name);
        }

        return null;
    }

    /// <summary>
    /// Extracts the signature deciphering algorithm from the player
    /// </summary>
    /// <param name="playerJs">The player javascript source</param>
    /// <param name="globalVariable">The player global variable, if available</param>
    /// <returns>The signature deciphering algorithm</returns>
    static string ExtractSigDecipherAlgorithm(
        string playerJs,
        (string Source, string Name)? globalVariable)
    {
        Match match = Regex.Match(playerJs, @"function\(([A-Za-z_0-9]+)\)\{([A-Za-z_0-9]+=[A-Za-z_0-9]+\.split\((?:[^)]+)\)(.+?)\.join\((?:[^)]+)\))\}");
        if (!match.Success && globalVariable?.Name is not null)
        {
            string globalVariableName = Regex.Escape(globalVariable.Value.Name);
            match = Regex.Match(playerJs, $@"function\(([A-Za-z_0-9]+)\)\{{([A-Za-z_0-9]+=[A-Za-z_0-9]+\[{globalVariableName}\[\d+\]\]\([^)]*\)([\s\S]+?)\[{globalVariableName}\[\d+\]\]\([^)]*\))\}}");
        }
        if (!match.Success)
            throw new Exception("Failed to extract signature deciphering algorithm");


        string varName = match.Groups[1].Value;
        string? objName = match.Groups[3].Value.Split(['.', '['], StringSplitOptions.RemoveEmptyEntries) is { Length: > 0 } parts ? parts[0].Replace(";", "").Trim() : null;
        string? functions = GetStringBetween(playerJs, $"var {objName}={{", "};");

        if (string.IsNullOrEmpty(varName) ||
            string.IsNullOrEmpty(objName) ||
            string.IsNullOrEmpty(functions))
            throw new Exception("Failed to extract signature deciphering algorithm");

        return $"{globalVariable?.Source ?? ""} function descramble_sig({varName}) {{ let {objName}={{{functions}}}; {match.Groups[2].Value} }} descramble_sig(sig);";
    }

    /// <summary>
    /// Extracts the n-signature deciphering algorithm from the player
    /// </summary>
    /// <param name="playerJs">The player javascript source</param>
    /// <param name="ast">The parsed player abstract syntax tree</param>
    /// <param name="globalVariable">The player global variable, if available</param>
    /// <returns>The n-signature deciphering algorithm</returns>
    static string ExtractNSigDecipherAlgorithm(
        string playerJs,
        Script ast,
        (string Source, string Name)? globalVariable)
    {
        string[] patterns = globalVariable is null
            ? ["-_w8_", "1969", "enhanced_except"]
            : [$"new Date({globalVariable.Value.Name}", ".push(String.fromCharCode(", ".reverse().forEach(function"];

        foreach (Node node in ast.DescendantNodesAndSelf())
        {
            if (!node.TryGetFunctionInfo(playerJs, out string? funcName, out string? funcCode))
                continue;

            foreach (string pattern in patterns)
            {
                if (!funcCode!.Contains(pattern))
                    continue;

                funcCode = Regex.Replace(funcCode, @"if\s*\(\s*typeof\s+[a-zA-Z0-9_$]+\s*===\s*[a-zA-Z0-9_[$]+\[\d+\]\s*\)\s*return\s*[a-zA-Z0-9_$];", "", RegexOptions.Multiline);
                return $"{funcCode.Trim()} {funcName}(nsig);";
            }
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
        Script ast = new Parser().ParseScript(playerJs);

        (string Source, string Name)? gloablVariable = ExtractGlobalVariable(playerJs, ast);
        string sigDecipherAlgorithm = ExtractSigDecipherAlgorithm(playerJs, gloablVariable);
        string nSigDecipherAlgorithm = ExtractNSigDecipherAlgorithm(playerJs, ast, gloablVariable);
        int sigTimestamp = ExtractSigTimestamp(playerJs);

        return new(sigDecipherAlgorithm, nSigDecipherAlgorithm, sigTimestamp, poToken);
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
            string decipheredSig = jsEngine.Evaluate(sigDecipherAlgorithm).AsString();
            urlQuery[sp] = decipheredSig;
        }
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
            "WEB_REMIX" => "1.20250331.01.00",
            _ => throw new Exception("This client is not supported"),
        };

        // Result
        string result = urlQuery.ToString() ?? throw new Exception("Failed to build url with deciphered signature");
        return result;
    }
}