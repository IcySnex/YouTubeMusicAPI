using System.Diagnostics.CodeAnalysis;
using YouTubeMusicAPI.Http;

namespace YouTubeMusicAPI.Utils;

internal static class Ensure
{
    /// <summary>
    /// Ensures that the given object is not <see langword="null"/>.
    /// </summary>
    /// <param name="value">The object to ensure not being <see langword="null"/>.</param>
    /// <param name="name">The name of the value.</param>
    /// <exception cref="ArgumentNullException">Occurs when the <c>value</c> is <see langword="null"/>./></exception>
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
    /// <param name="name">The name of the value.</param>
    /// <exception cref="ArgumentException">Occurs when the <c>value</c> is <see langword="null"/> or empty.</exception>
    public static void NotNullOrEmpty(
        [NotNull] string? value,
        string name)
    {
        if (!string.IsNullOrEmpty(value))
            return;

        throw new ArgumentException($"The value is null or empty.", name);
    }


    /// <summary>
    /// Ensures that the user is authenticated in the provided <see cref="YouTubeMusicClient"/>.
    /// </summary>
    /// <param name="client">The client to check if authenticated.</param>
    /// <exception cref="InvalidOperationException">Occurs when the <c>client</c> is not authenticated.</exception>
    public static void Authenticated(
        YouTubeMusicClient client)
    {
        if (client.RequestHandler.IsAuthenticated)
            return;

        throw new InvalidOperationException("The user must be authenticate before using this endpoint.");
    }
}