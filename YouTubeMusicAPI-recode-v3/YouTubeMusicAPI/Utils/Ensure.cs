using System.Diagnostics.CodeAnalysis;

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
}