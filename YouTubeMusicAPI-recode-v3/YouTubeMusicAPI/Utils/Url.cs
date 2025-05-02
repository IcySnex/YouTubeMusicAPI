namespace YouTubeMusicAPI.Utils;

internal static class Url
{
    public static string RemoveQueryParameter(
        string url,
        string key)
    {
        Ensure.NotNullOrEmpty(url, nameof(url));
        Ensure.NotNullOrEmpty(key, nameof(key));

        int index = url.IndexOf('?');
        if (index == -1)
            return url;

        string baseUrl = url[..index];
        string queryString = url[(index + 1)..];

        string query = "";
        foreach (string parameter in queryString.Split('&'))
        {
            if (parameter.StartsWith(key + '=', StringComparison.Ordinal))
                continue;

            query += query.Length == 0 ? parameter : '&' + parameter;
        }

        return query.Length == 0 ? baseUrl : baseUrl + '?'+ query;
    }

    public static string SetQueryParameter(
        string url,
        string key,
        string value)
    {
        Ensure.NotNullOrEmpty(url, nameof(url));

        if (!url.Contains('?'))
            return url + '?' + key + '=' + value;

        string newUrl = RemoveQueryParameter(url, key);
        newUrl += (newUrl.Contains('?') ? '&' : '?') + key + '=' + value;

        return newUrl;
    }
}