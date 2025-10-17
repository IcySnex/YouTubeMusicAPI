using Acornima;
using Acornima.Ast;
using Jint;
using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;
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



    static bool IsSigMatcher(
        VariableDeclarator node)
    {
        if (node.Id is not Identifier id ||
            node.Init is not FunctionExpression fn ||
            fn.Params.Count != 3 ||
            fn.Body is not BlockStatement body)
            return false;

        foreach (Statement stmt in body.Body)
        {
            if (stmt is not ExpressionStatement exprStmt)
                continue;

            if (exprStmt.Expression is not LogicalExpression logical ||
                logical.Operator != Operator.LogicalAndAssignment ||
                logical.Left is not Identifier ||
                logical.Right is not SequenceExpression seq ||
                seq.Expressions.Count <= 0)
                continue;

            if (seq.Expressions[0] is not AssignmentExpression assign ||
                assign.Operator != Operator.Assignment ||
                assign.Left is not Identifier ||
                assign.Right is not CallExpression call ||
                call.Callee is not Identifier ||
                !call.Arguments.Any(arg =>
                    arg is CallExpression callExp &&
                    callExp.Callee is Identifier callee &&
                    callee.Name == "decodeURIComponent"))
                continue;

            return true;
        }

        return false;
    }

    static bool IsNSigMatcher(
        VariableDeclarator node)
    {
        return node.Id is Identifier &&
               node.Init is ArrayExpression arr &&
               arr.Elements.FirstOrDefault() is Identifier;
    }

    static bool IsSigTimestampMatcher(
        Node node,
        string playerJs)
    {
        if (node is not VariableDeclaration varDecl)
            return false;

        string code = varDecl.GetFunctionCode(playerJs);
        if (code.Contains("signatureTimestamp"))
        {

        }

        return false;
    }

    static JsAlgorithms ExtractJsAlgorithms(
        string playerJs)
    {
        Script ast = new Parser().ParseScript(playerJs);

        string? sigDecipher = null;
        string? nSigDecipher = null;
        int? sigTimestamp = null;

        var shit = ast.DescendantNodesAndSelf();
        foreach (Node node in shit)
        {
            //if (IsSigMatcher(varDecl))
            //    sigDecipher = varDecl.GetFunctionCode(playerJs);
            //else if (IsNSigMatcher(varDecl))
            //    nSigDecipher = varDecl.GetFunctionCode(playerJs);
            if (IsSigTimestampMatcher(node, playerJs))
            {

            }
        }

        if (sigDecipher is null)
            throw new Exception("Failed to extract signature decipher function");
        if (nSigDecipher is null)
            throw new Exception("Failed to extract n decipher function");
        if (sigTimestamp is null)
            throw new("Failed to extract signature timestamp");

        return new(sigDecipher, nSigDecipher, sigTimestamp.Value);
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

        //string playerId = js.GetStringBetween(@"player\/", @"\/") ?? throw new Exception("Failed to get player id");
        //string playerUrl = "https://www.youtube.com" + $"/s/player/{playerId}/player_ias.vflset/en_US/base.js";

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