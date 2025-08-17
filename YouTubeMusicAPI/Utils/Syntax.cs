using System.Runtime.CompilerServices;

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
    /// <exception cref="InvalidOperationException">Occurrs when the <c>expression</c> returned <see langword="null"/>.</exception>
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
    /// <exception cref="InvalidOperationException">Occurrs when the <c>expression</c> returned <see langword="null"/>.</exception>
    public static T OrThrow<T>(
        this T? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null) where T : class =>
        value ?? throw new InvalidOperationException($"Value was null but expected non-nullable {typeof(T).Name}: {expression}");


    /// <summary>
    /// Returns one of two results depending on whether the specified <c>value</c> equals the provided <c>condition</c>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value being compared.</typeparam>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="value">The value to compare.</param>
    /// <param name="condition">The value to compare against.</param>
    /// <param name="trueResult">The result to return if <c>value</c> equals <c>condition</c>>.</param>
    /// <param name="falseResult">The result to return if <c>value</c> does not equal <c>condition</c>.</param>
    /// <returns>The <c>trueResult</c> if the values are equal, otherwise the <c>falseResult</c>.</returns>
    public static TResult If<TValue, TResult>(
        this TValue value,
        TValue condition,
        TResult trueResult,
        TResult falseResult) =>
        EqualityComparer<TValue>.Default.Equals(value, condition) ? trueResult : falseResult;

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
    /// Returns whether the <c>value</c> is <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> if <c>value</c> is <see langword="null"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsNull<T>(
        this T? value) where T : class =>
        value is null;

    /// <summary>
    /// Returns the logical negation of the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The boolean value to negate.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is <see langword="false"/>, otherwise <see langword="false"/>.</returns>
    public static bool Not(
        this bool value) =>
        !value;


    public static void ForEach<T>(
        this IEnumerable<T> source,
        Action<T> action)
    {
        foreach (T item in source)
            action(item);
    }



    /// <summary>
    /// Converts a value type to its nullable equivalent.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The <c>value</c> as a nullable <c>T?</c>.</returns>
    public static T? AsNullable<T>(
        this T value) where T : struct =>
        EqualityComparer<T>.Default.Equals(value, default) ? null : value;


    /// <summary>
    /// Evaluates two expressions on the same object and returns the first non-null result.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="first">The first expression to evaluate.</param>
    /// <param name="second">The fallback expression to evaluate if the first returns null.</param>
    /// <returns>The first non-null result of <c>first</c> or <c>second</c>.</returns>
    public static TResult Coalesce<TSource, TResult>(
        this TSource source,
        Func<TSource, TResult> first,
        Func<TSource, TResult> second) =>
        first(source) ?? second(source);
}