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
    /// <exception cref="NullReferenceException">Occurrs when the <c>value</c> is <see langword="null"/>.</exception>
    public static T OrThrow<T>(
        this T? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null) where T : struct =>
        value ?? throw new NullReferenceException($"Value is null: {expression}");

    /// <summary>
    /// Returns the value if it's not  <see langword="null"/>, otherwise throws an exception.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="expression">The original expression that produced the value (automatically provided by the compiler).</param>
    /// <returns>The <c>value</c> if it's not <see langword="null"/>.</returns>
    /// <exception cref="NullReferenceException">Occurrs when the <c>value</c> is <see langword="null"/>.</exception>
    public static T OrThrow<T>(
        this T? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null) where T : class =>
        value ?? throw new NullReferenceException($"Value is null: {expression}");


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
}