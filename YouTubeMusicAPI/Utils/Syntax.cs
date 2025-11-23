using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using YouTubeMusicAPI.Json;

namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains extension methods for fluent syntax.
/// </summary>
internal static class Syntax
{
    /// <summary>
    /// Returns the <c>value</c> if it's not <see langword="null"/>, otherwise the specified default value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="defaultValue">The value to return if <c>value</c> is  <see langword="null"/>.</param>
    /// <returns>The <c>value</c> if it's not <see langword="null"/>, otherwise the <c>defaultValue</c>.</returns>
    public static T Or<T>(
        this T? value,
        T defaultValue) where T : struct =>
        value ?? defaultValue;

    /// <summary>
    /// Returns the <c>value</c> if it's not <see langword="null"/>, otherwise the specified default value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="defaultValue">The value to return if <c>value</c> is  <see langword="null"/>.</param>
    /// <returns>The <c>value</c> if it's not <see langword="null"/>, otherwise the <c>defaultValue</c>.</returns>
    public static T Or<T>(
        this T? value,
        T defaultValue) where T : class =>
        value ?? defaultValue;


    /// <summary>
    /// Returns the value if it's not  <see langword="null"/>, otherwise throws an exception.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="expression">The original expression that produced the value (automatically provided by the compiler).</param>
    /// <returns>The <c>value</c> if it's not <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the <c>expression</c> returned <see langword="null"/>.</exception>
    public static T OrThrow<T>(
        this T? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null) where T : struct =>
        value ?? throw new InvalidOperationException($"Value was null but expected non-nullable {typeof(T).Name}: {expression}");

    /// <summary>
    /// Returns the value if it's not  <see langword="null"/>, otherwise throws an exception.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="expression">The original expression that produced the value (automatically provided by the compiler).</param>
    /// <returns>The <c>value</c> if it's not <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException">Occurs when the <c>expression</c> returned <see langword="null"/>.</exception>
    public static T OrThrow<T>(
        this T? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null) where T : class =>
        value ?? throw new InvalidOperationException($"Value was null but expected non-nullable {typeof(T).Name}: {expression}");


    /// <summary>
    /// Returns whether the <c>value</c> equals any of the specified <c>conditions</c>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to compare.</param>
    /// <param name="conditions">The values to compare against.</param>
    /// <returns><see langword="true"/> if <c>value</c> equals any of the <c>conditions</c>, otherwise <see langword="false"/>.</returns>
    public static bool Is<T>(
        this T value,
        params T[] conditions) =>
        conditions.Any(c => EqualityComparer<T>.Default.Equals(value, c));

    /// <summary>
    /// Returns whether the <c>value</c> equals to the specified <c>condition</c>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to compare.</param>
    /// <param name="condition">The value to compare against.</param>
    /// <returns><see langword="true"/> if <c>value</c> is equal to the <c>condition</c>, otherwise <see langword="false"/>.</returns>
    public static bool Is<T>(
        this T value,
        T condition) =>
        EqualityComparer<T>.Default.Equals(value, condition);


    /// <summary>
    /// Returns whether the <c>value</c> doesn't equal to any of the specified <c>conditions</c>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to compare.</param>
    /// <param name="conditions">The values to compare against.</param>
    /// <returns><see langword="true"/> if <c>value</c> equals any of the <c>conditions</c>, otherwise <see langword="false"/>.</returns>
    public static bool IsNot<T>(
        this T value,
        params T[] conditions) =>
        !conditions.Any(c => EqualityComparer<T>.Default.Equals(value, c));

    /// <summary>
    /// Returns whether the <c>value</c> doesn't equal to the specified <c>condition</c>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to compare.</param>
    /// <param name="condition">The value to compare against.</param>
    /// <returns><see langword="true"/> if <c>value</c> doesn't equal the <c>condition</c>, otherwise <see langword="false"/>.</returns>
    public static bool IsNot<T>(
        this T value,
        T condition) =>
        !EqualityComparer<T>.Default.Equals(value, condition);


    /// <summary>
    /// Returns whether the <c>value</c> is <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> if <c>value</c> is <see langword="null"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsNull<T>(
        this T? value) where T : class =>
        value is null;

    /// <summary>
    /// Returns whether the <c>value</c> is <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> if <c>value</c> is <see langword="null"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsNull<T>(
        this T? value) where T : struct =>
        value is null;


    /// <summary>
    /// Returns whether the <c>value</c> is not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> if <c>value</c> is not <see langword="null"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsNotNull<T>(
        this T? value) where T : class =>
        value is not null;

    /// <summary>
    /// Returns whether the <c>value</c> is not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> if <c>value</c> is not <see langword="null"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsNotNull<T>(
        this T? value) where T : struct =>
        value is not null;


    /// <summary>
    /// Returns whether the <c>value</c> is not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="result">The non-null result.</param>
    /// <returns><see langword="true"/> if <c>value</c> is not <see langword="null"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsNotNull<T>(
        this T? value,
        [NotNullWhen(true)] out T? result) where T : class
    {
        if (value is null)
        {
            result = null;
            return false;
        }

        result = value;
        return true;
    }

    /// <summary>
    /// Returns whether the <c>value</c> is not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="result">The non-null result.</param>
    /// <returns><see langword="true"/> if <c>value</c> is not <see langword="null"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsNotNull<T>(
        this T? value,
        [NotNullWhen(true)] out T? result) where T : struct
    {
        if (value is null)
        {
            result = null;
            return false;
        }

        result = value;
        return true;
    }


    /// <summary>
    /// Returns the logical AND of the specified boolean values.
    /// </summary>
    /// <param name="value">The first boolean value.</param>
    /// <param name="other">The second boolean value.</param>
    /// <returns>
    /// <see langword="true"/> if both <c>value</c> and <c>other</c> are <see langword="true"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool And(
        this bool value,
        bool other) =>
        value && other;

    /// <summary>
    /// Returns the logical OR of the specified boolean values.
    /// </summary>
    /// <param name="value">The first boolean value.</param>
    /// <param name="other">The second boolean value.</param>
    /// <returns>
    /// <see langword="true"/> if either <c>value</c> or <c>other</c> is <see langword="true"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool Or(
        this bool value,
        bool other) =>
        value || other;

    /// <summary>
    /// Returns the logical negation of the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The boolean value to negate.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is <see langword="false"/>, otherwise <see langword="false"/>.</returns>
    public static bool Not(
        this bool value) =>
        !value;


    /// <summary>
    /// Performs the specified <c>action</c> on each element of the <c>source</c>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence of elements to iterate over.</param>
    /// <param name="action">The action to perform on each element.</param>
    public static void ForEach<T>(
        this IEnumerable<T> source,
        Action<T> action)
    {
        foreach (T item in source)
            action(item);
    }


    /// <summary>
    /// Applies a specified mapping function to the source value if not null and returns the result.
    /// </summary>
    /// <typeparam name="TSource">The type of the input value to be mapped.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the mapping function.</typeparam>
    /// <param name="source">The value to be transformed by the mapping function.</param>
    /// <param name="mapper">A function that defines how to map the source value to a result. Cannot be null.</param>
    /// <returns>The result of applying the mapping function to the source value.</returns>
    public static TResult? Map<TSource, TResult>(
        this TSource? source,
        Func<TSource, TResult> mapper) =>
        source is null ? default : mapper(source);


    /// <summary>
    /// Concatenates the elements of a string sequence using the specified <c>separator</c>.
    /// </summary>
    /// <param name="source">The sequence of strings to join.</param>
    /// <param name="separator">The string to use as a separator.</param>
    /// <returns>The connected string.</returns>
    public static string Join(
        this IEnumerable<string> source,
        string separator) =>
        string.Join(separator, source);


    /// <summary>
    /// Evaluates two expressions on the same <see cref="JElement"/> and returns the first non-undefined result.
    /// </summary>
    /// <param name="source">The source element.</param>
    /// <param name="first">The first expression to evaluate.</param>
    /// <param name="second">The fallback expression to evaluate if the first is undefined.</param>
    /// <returns>The first non-undefined result of <c>first</c> or <c>second</c>.</returns>
    public static JElement Coalesce(
        this JElement source,
        Func<JElement, JElement> first,
        Func<JElement, JElement> second)
    {
        JElement result = first(source);
        if (result.IsUndefined)
            result = second(source);

        return result;
    }

    /// <summary>
    /// Evaluates the expressions on the same <see cref="JElement"/> and returns the first non-undefined result.
    /// </summary>
    /// <param name="source">The source element.</param>
    /// <param name="expressions">The expressions to evaluate.</param>
    /// <returns>The first non-undefined result of <c>expressions</c>.</returns>
    public static JElement Coalesce(
        this JElement source,
        params Func<JElement, JElement>[] expressions)
    {
        foreach (Func<JElement, JElement> expression in expressions)
        {
            JElement result = expression(source);
            if (!result.IsUndefined)
                return result;
        }

        return default;
    }

    /// <summary>
    /// Parses a JSON string into a <see cref="JsonDocument"/> and wraps its root element in a <see cref="JElement"/>.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="root">A <see cref="JElement"/> wrapping the root element of the docuemnt.</param>
    /// <returns>An <see cref="IDisposable"/> representing the document. Has to be disposed.</returns>
    public static IDisposable ParseJson(
        this string json,
        out JElement root)
    {
        JsonDocument document = JsonDocument.Parse(json);
        root = new JElement(document.RootElement);

        return document;
    }
}