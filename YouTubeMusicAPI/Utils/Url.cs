namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains utility methods for manipulating URLs.
/// </summary>
internal static class Url
{
    /// <summary>
    /// Removes a specific query parameter from the given URL.
    /// </summary>
    /// <param name="url">The URL from which the query parameter should be removed.</param>
    /// <param name="key">The key of the query parameter to remove.</param>
    /// <returns>The URL with the specified query parameter removed. If the parameter is not found, the original URL is returned.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>url</c> or <c>key</c> is <see langword="null"/> or empty.</exception>
    public static string RemoveQueryParameter(
        this string url,
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

        return query.Length == 0 ? baseUrl : baseUrl + '?' + query;
    }

    /// <summary>
    /// Adds or updates a query parameter in the specified URL.
    /// </summary>
    /// <param name="url">The URL to which the query parameter will be added or updated.</param>
    /// <param name="key">The key of the query parameter to add or update.</param>
    /// <param name="value">The value of the query parameter to associate with the specified key.</param>
    /// <returns>A new URL with the specified query parameter added or updated. If the key already exists in the query string, its value is replaced.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>url</c> or <c>key</c> is <see langword="null"/> or empty.</exception>
    public static string SetQueryParameter(
        this string url,
        string key,
        string value)
    {
        Ensure.NotNullOrEmpty(url, nameof(url));
        Ensure.NotNullOrEmpty(key, nameof(key));

        if (!url.Contains('?'))
            return url + '?' + key + '=' + value;

        string newUrl = RemoveQueryParameter(url, key);
        newUrl += (newUrl.Contains('?') ? '&' : '?') + key + '=' + value;

        return newUrl;
    }
}