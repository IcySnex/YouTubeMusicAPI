using System.Runtime.CompilerServices;

namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains extension methods for fluent syntax.
/// </summary>
internal static class Syntax
{
    /// <summary>
    /// Returns the value if it is not <c>null</c>; otherwise, returns the specified default value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="defaultValue">The value to return if <paramref name="value"/> is <c>null</c>.</param>
    /// <returns><paramref name="value"/> if it is not <c>null</c>; otherwise, <paramref name="defaultValue"/>.</returns>
    public static T Or<T>(
        this T? value,
        T defaultValue) where T : struct =>
        value ?? defaultValue;
    /// <summary>
    /// Returns the value if it is not <c>null</c>; otherwise, returns the specified default value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="defaultValue">The value to return if <paramref name="value"/> is <c>null</c>.</param>
    /// <returns><paramref name="value"/> if it is not <c>null</c>; otherwise, <paramref name="defaultValue"/>.</returns>
    public static T Or<T>(
        this T? value,
        T defaultValue) where T : class =>
        value ?? defaultValue;


    /// <summary>
    /// Returns the value if it is not <c>null</c>; otherwise, throws a <see cref="NullReferenceException"/> with the original expression text included in the message.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="expression">The original expression that produced the value (automatically provided by the compiler).</param>
    /// <returns><paramref name="value"/> if it is not <c>null</c>.</returns>
    /// <exception cref="NullReferenceException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static T OrThrow<T>(
        this T? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null) where T : struct =>
        value ?? throw new NullReferenceException($"Value was null: {expression}");

    /// <summary>
    /// Returns the value if it is not <c>null</c>; otherwise, throws a <see cref="NullReferenceException"/> with the original expression text included in the message.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="expression">The original expression that produced the value (automatically provided by the compiler).</param>
    /// <returns><paramref name="value"/> if it is not <c>null</c>.</returns>
    /// <exception cref="NullReferenceException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static T OrThrow<T>(
        this T? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null) where T : class =>
        value ?? throw new NullReferenceException($"Value was null: {expression}");


    /// <summary>
    /// Returns one of two results depending on whether the specified value equals the provided condition.
    /// </summary>
    /// <typeparam name="TValue">The type of the value being compared.</typeparam>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="value">The value to compare.</param>
    /// <param name="condition">The value to compare against.</param>
    /// <param name="trueResult">The result to return if <paramref name="value"/> equals <paramref name="condition"/>.</param>
    /// <param name="falseResult">The result to return if <paramref name="value"/> does not equal <paramref name="condition"/>.</param>
    /// <returns><paramref name="trueResult"/> if the values are equal; otherwise, <paramref name="falseResult"/>.</returns>
    public static TResult If<TValue, TResult>(
        this TValue value,
        TValue condition,
        TResult trueResult,
        TResult falseResult) =>
        EqualityComparer<TValue>.Default.Equals(value, condition) ? trueResult : falseResult;
}