using Acornima;
using Acornima.Ast;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using YouTubeMusicAPI.Internal.JavaScript;
using YouTubeMusicAPI.Models.Search;

namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Contains extensions methods
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// Converts search category to YouTube Music request payload params
    /// </summary>
    /// <param name="value">The YouTube Music item kind to convert</param>
    /// <returns>A YouTube Music request payload params</returns>
    public static string? ToParams(
        this SearchCategory? value) =>
        value switch
        {
            SearchCategory.Songs => "EgWKAQIIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            SearchCategory.Videos => "EgWKAQIQAWoQEAMQBBAJEAoQBRAREBAQFQ%3D%3D",
            SearchCategory.Albums => "EgWKAQIYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            SearchCategory.CommunityPlaylists => "EgeKAQQoAEABahAQAxAKEAkQBBAFEBEQEBAV",
            SearchCategory.Artists => "EgWKAQIgAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            SearchCategory.Podcasts => "EgWKAQJQAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            SearchCategory.Episodes => "EgWKAQJIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            SearchCategory.Profiles => "EgWKAQJYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            _ => null
        };

    /// <summary>
    /// Converts a string to YouTube Music search category
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>A SearchCategory</returns>
    public static SearchCategory? ToSearchCategory(
        this string? value) =>
        value switch
        {
            "Songs" => SearchCategory.Songs,
            "Videos" => SearchCategory.Videos,
            "Albums" => SearchCategory.Albums,
            "Community playlists" or "Playlists" => SearchCategory.CommunityPlaylists,
            "Artists" => SearchCategory.Artists,
            "Podcasts" => SearchCategory.Podcasts,
            "Episodes" => SearchCategory.Episodes,
            "Profiles" => SearchCategory.Profiles,
            _ => null
        };

    /// <summary>
    /// Converts a type to YouTube Music search category
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>A SearchCategory</returns>
    public static SearchCategory? ToSearchCategory(
        this Type value) =>
        value switch
        {
            _ when value == typeof(SongSearchResult) => SearchCategory.Songs,
            _ when value == typeof(VideoSearchResult) => SearchCategory.Videos,
            _ when value == typeof(CommunityPlaylistSearchResult) => SearchCategory.CommunityPlaylists,
            _ when value == typeof(ArtistSearchResult) => SearchCategory.Artists,
            _ when value == typeof(PodcastSearchResult) => SearchCategory.Podcasts,
            _ when value == typeof(EpisodeSearchResult) => SearchCategory.Episodes,
            _ when value == typeof(ProfileSearchResult) => SearchCategory.Profiles,
            _ => null
        };


    /// <summary>
    /// Parses an exact string into a TimeSpan
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <returns>A new TimeSpan</returns>
    /// <exception cref="ArgumentException">Occurrs when value has an invalid format</exception>
    public static TimeSpan ToTimeSpan(
        this string value)
    {
        if (TimeSpan.TryParseExact(value, @"m\:ss", null, out TimeSpan timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(value, @"mm\:ss", null, out timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(value, @"h\:mm\:ss", null, out timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(value, @"hh\:mm\:ss", null, out timeSpan))
            return timeSpan;

        throw new ArgumentException("value has an invalid format");
    }

    /// <summary>
    /// Parses a long string into a TimeSpan
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <returns>A new TimeSpan</returns>
    public static TimeSpan ToTimeSpanLong(
        this string value)
    {
        Regex hourPattern = new(@"(\d+)\s*\+?\s*hour", RegexOptions.IgnoreCase);
        Regex minutePattern = new(@"(\d+)\s*\+?\s*minute", RegexOptions.IgnoreCase);
        Regex secondPattern = new(@"(\d+)\s*\+?\s*second", RegexOptions.IgnoreCase);

        Match hourMatch = hourPattern.Match(value);
        Match minuteMatch = minutePattern.Match(value);
        Match secondMatch = secondPattern.Match(value);

        return new(
            hourMatch.Success ? int.Parse(hourMatch.Groups[1].Value) : 0,
            minuteMatch.Success ? int.Parse(minuteMatch.Groups[1].Value) : 0,
            secondMatch.Success ? int.Parse(secondMatch.Groups[1].Value) : 0);
    }


    /// <summary>
    /// Gets the string between two strings
    /// </summary>
    /// <param name="value">The source string</param>
    /// <param name="start">The string for the start index</param>
    /// <param name="end">The string for the end index</param>
    /// <returns>The string</returns>
    public static string? GetStringBetween(
        this string value,
        string start,
        string end)
    {
        int startIndex = value.IndexOf(start);
        if (startIndex == -1)
            return null;

        int endIndex = value.IndexOf(end, startIndex + start.Length);
        if (endIndex == -1)
            return null;

        return value.Substring(startIndex + start.Length, endIndex - startIndex - start.Length);
    }

    /// <summary>
    /// Gets the fucntion code from a node
    /// </summary>
    /// <param name="value">The node to parse</param>
    /// <param name="fullJs">The full js containing the node</param>
    /// <returns>A boolean indicating weither the function was parsed correctly</returns>
    public static string GetFunctionCode(
        this Node value,
        string fullJs) =>
        fullJs.Substring(value.Start, value.End - value.Start);


    public static string? MemberToString(
        this Node value,
        string source)
    {
        List<string> segments = [];
        Node currentNode = value;

        while (currentNode is MemberExpression curMember)
        {
            Node prop = curMember.Property;
            if (prop is null)
                return null;

            if (curMember.Computed)
            {
                string? propSource = prop.GetFunctionCode(source);
                if (string.IsNullOrEmpty(propSource))
                    return null;

                segments.Insert(0, $"[{propSource.Trim()}]");
            }
            else
            {
                if (prop is not Identifier id)
                    return null;

                segments.Insert(0, $".{id.Name}");
            }

            currentNode = curMember.Object;
        }

        string? baseName = currentNode switch
        {
            Identifier id => id.Name,
            ThisExpression _ => "this",
            _ => null
        };

        return baseName != null ? baseName + string.Concat(segments) : null;
    }

    public static string? MemberBaseName(
        this MemberExpression value,
        string source)
    {
        Node? target = value.Object;

        while (target is MemberExpression nested)
        {
            string? parentName = MemberToString(nested, source);
            if (parentName is not null)
                return parentName;

            target = nested.Object;
        }

        return target switch
        {
            Identifier id => id.Name,
            ThisExpression _ => "this",
            _ => null
        };
    }

    public static string ExtractNodeSource(
        this Node value,
        string source) =>
        source.Substring(value.Start, value.End - value.Start);

    public static string? CreateWrapperFunction(
        this Node node,
        string name,
        JsAnalyzer analyzer)
    {
        string GenerateSignatureWrapper(
            string functionName,
            string targetFunction) =>
            $"  function {functionName}(input) {{\n    const helper = {targetFunction}('https://www.youtube.com/videoplayback', 'signature', String(input));\n    return helper.get('signature');\n  }}";

        string GenerateNClassWrapper(
            string functionName,
            string targetClass) =>
            $"  function {functionName}(input) {{\n    let url = input;\n    if (typeof input !== 'string' || (!input.startsWith('http://') && !input.startsWith('https://'))) {{\n      url = 'https://www.youtube.com/videoplayback?n=' + encodeURIComponent(String(input));\n    }}\n    const helper = new {targetClass}(url, true);\n    return helper.get('n');\n  }}";
        
        string GenerateWrapper(
            string functionName,
            string targetFunction,
            string args) =>
            $"  function {functionName}(input) {{\n    return {targetFunction}({args});\n  }}";

        string ParseFunctionArguments(
            JsAnalyzer analyzer,
            IEnumerable<Node> args)
        {
            List<string> parameters = [];
            foreach (Node arg in args)
                switch (arg)
                {
                    case Identifier id when analyzer.DeclaredVariables.ContainsKey(id.Name):
                        parameters.Add(id.Name);
                        break;

                    case Literal literal when literal.Value is string || literal.Value is double || literal.Value is int:
                        parameters.Add(JsonConvert.SerializeObject(literal.Value));
                        break;

                    default:
                        if (!parameters.Contains("input"))
                            parameters.Add("input");
                        break;
                }

            return string.Join(", ", parameters);
        }

        switch (node)
        {
            case CallExpression callExpr when callExpr.Callee is Identifier callExprCalleeId && analyzer.DeclaredVariables.ContainsKey(callExprCalleeId.Name):
                string callExprArgs = ParseFunctionArguments(analyzer, callExpr.Arguments);
                return GenerateWrapper(name, callExprCalleeId.Name, callExprArgs);

            case VariableDeclarator variableDecl when variableDecl.Init is FunctionExpression functionExpr && variableDecl.Id is Identifier variableDeclId:
                if (LooksLikeSignatureHelper(functionExpr))
                    return GenerateSignatureWrapper(name, variableDeclId.Name);

                string variableDeclArgs = ParseFunctionArguments(analyzer, functionExpr.Params);
                return GenerateWrapper(name, variableDeclId.Name, variableDeclArgs);

            // holy shit. why did you hard code that nFunction name chadacious ????? then why even bother letting the extractor choose the name??
            case ExpressionStatement exprStmt when name == "nFunction" && exprStmt.Expression is AssignmentExpression assignExpr && assignExpr.Operator == Operator.Assignment && assignExpr.Right is FunctionExpression:
                string? targetName = MemberToString(assignExpr.Left, analyzer.Source);
                if (targetName is not null && targetName.StartsWith("g."))
                    return GenerateNClassWrapper(name, targetName);

                break;
        }

        return null;
    }

    public static bool IsTruthyBooleanNode(
        this Node? node)
    {
        return (node is Literal literal && literal.Value is bool b && b) ||
               (node is UnaryExpression unary && unary.Operator == Operator.LogicalNot && unary.Argument is Literal innerLiteral && Convert.ToInt32(innerLiteral.Value) == 0);
    }

    public static bool LooksLikeSignatureHelper(
        this FunctionExpression node)
    {
        if (node.Params.Count != 3 || node.Body is null)
            return false;

        Identifier? helperName = node.Params[0] as Identifier;
        Identifier? signatureParam = node.Params[1] as Identifier;
        if (helperName is null || signatureParam is null)
            return false;

        bool hasUrlHelperConstructor = false;
        bool hasAlrSet = false;
        bool hasSignatureWrite = false;
        bool returnsHelper = false;

        AstWalker.Walk(node.Body, (innerNode, parent, ancestors) =>
        {
            switch (innerNode)
            {
                case NewExpression newExpr when newExpr.Callee is MemberExpression memberCallee && newExpr.Arguments.Count >= 2 && newExpr.Arguments[0] is Identifier arg0 && arg0.Name == helperName.Name && IsTruthyBooleanNode(newExpr.Arguments[1]):
                    string? calleeName = MemberToString(memberCallee, "");
                    hasUrlHelperConstructor |= calleeName != null && calleeName.StartsWith("g.");

                    break;

                case CallExpression callExpr when callExpr.Callee is MemberExpression memberCall && memberCall.Object is Identifier objId && objId.Name == helperName.Name:
                    if (callExpr.Arguments.Count >= 2 &&
                        callExpr.Arguments[0] is Literal arg0Lit &&
                        arg0Lit.Value?.ToString() == "alr" &&
                        callExpr.Arguments[1] is Literal arg1Lit &&
                        arg1Lit.Value?.ToString() == "yes")
                        hasAlrSet = true;
                    else if (callExpr.Arguments.Count >= 2 &&
                        callExpr.Arguments[0] is Identifier arg0Id &&
                        arg0Id.Name == signatureParam.Name)
                        hasSignatureWrite = true;

                    break;

                case ReturnStatement retStmt when retStmt.Argument is Identifier retId && retId.Name == helperName.Name:
                    returnsHelper = true;

                    break;
            }

            return AstWalker.NORMAL;
        });

        return hasUrlHelperConstructor && hasAlrSet && hasSignatureWrite && returnsHelper;
    }

    public static string? GetUrlHelperClassName(
        this FunctionExpression node)
    {
        if (node.Body is null)
            return null;

        if (node.Params[0] is not Identifier helperName)
            return null;

        string? className = null;
        AstWalker.Walk(node.Body, (innerNode, parent, ancestors) =>
        {
            if (innerNode is NewExpression newExpr &&
                newExpr.Callee is MemberExpression memberCallee &&
                newExpr.Arguments.Count >= 2 &&
                newExpr.Arguments[0] is Identifier arg0 &&
                arg0.Name == helperName.Name &&
                IsTruthyBooleanNode(newExpr.Arguments[1]))
            {
                string? calleeName = MemberToString(memberCallee, "");
                if (calleeName?.StartsWith("g.") == true)
                {
                    className = calleeName;
                    return AstWalker.STOP;
                }
            }

            return AstWalker.NORMAL;
        });

        return className;
    }

}