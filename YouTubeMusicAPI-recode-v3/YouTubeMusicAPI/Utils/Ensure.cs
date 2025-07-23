using System.Diagnostics.CodeAnalysis;
using YouTubeMusicAPI.Http;

namespace YouTubeMusicAPI.Utils;

internal static class Ensure
{
    /// <summary>
    /// Ensures that the given object is not <see langword="null"/>.
    /// </summary>
    /// <param name="value">The object to ensure not being <see langword="null"/>.</param>
    /// <param name="name">The name of the object.</param>
    /// <exception cref="ArgumentNullException">Occurrs when the object is <see langword="null"/>./></exception>
    public static void NotNull(
        [NotNull] object? value,
        string name)
    {
        if (value is not null)
            return;

        throw new ArgumentNullException(name);
    }

    /// <summary>
    /// Ensures that the given string is not <see langword="null"/> or empty.
    /// </summary>
    /// <param name="value">The string to ensure not being <see langword="null"/> or empty.</param>
    /// <param name="name">The name of the string.</param>
    /// <exception cref="ArgumentException">Occurrs when the string is <see langword="null"/> or empty.</exception>
    public static void NotNullOrEmpty(
        [NotNull] string? value,
        string name)
    {
        if (!string.IsNullOrEmpty(value))
            return;

        throw new ArgumentException($"String is null or empty.", name);
    }


    /// <summary>
    /// Ensures that the user is authenticated in the provided <see cref="RequestHandler"/>.
    /// </summary>
    /// <param name="requestHandler">The request handler to check authentication status on.</param>
    /// <exception cref="InvalidOperationException">Occurrs when the user is not authenticated.</exception>
    public static void IsAuthenticated(
        RequestHandler requestHandler)
    {
        if (requestHandler.IsAuthenticated)
            return;

        throw new InvalidOperationException("The user must be authenticate before using this endpoint.");
    }
}