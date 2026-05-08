using System.Collections;

namespace Xunit;

[AttributeUsage(AttributeTargets.Method)]
public sealed class FactAttribute : Attribute
{
}

public static class Assert
{
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message ?? "Expected condition to be true.");
        }
    }

    public static void False(bool condition, string? message = null)
    {
        if (condition)
        {
            throw new InvalidOperationException(message ?? "Expected condition to be false.");
        }
    }

    public static void Null(object? value, string? message = null)
    {
        if (value != null)
        {
            throw new InvalidOperationException(message ?? "Expected value to be null.");
        }
    }

    public static void NotNull(object? value, string? message = null)
    {
        if (value == null)
        {
            throw new InvalidOperationException(message ?? "Expected value to be non-null.");
        }
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (expected is IEnumerable expectedEnumerable &&
            actual is IEnumerable actualEnumerable &&
            expected is not string &&
            actual is not string)
        {
            AssertEnumerablesEqual(expectedEnumerable, actualEnumerable);
            return;
        }

        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected '{expected}', but found '{actual}'.");
        }
    }

    public static void Equal<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        AssertEnumerablesEqual(expected, actual);
    }

    public static void StartsWith(string expectedStart, string? actual)
    {
        if (actual == null || !actual.StartsWith(expectedStart, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected '{actual}' to start with '{expectedStart}'.");
        }
    }

    public static void Contains(string expectedSubstring, string? actualString)
    {
        if (actualString == null || !actualString.Contains(expectedSubstring, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected '{actualString}' to contain '{expectedSubstring}'.");
        }
    }

    public static void Contains<T>(IEnumerable<T> collection, Predicate<T> match)
    {
        if (!collection.Any(item => match(item)))
        {
            throw new InvalidOperationException("Expected collection to contain a matching item.");
        }
    }

    public static void DoesNotContain<T>(IEnumerable<T> collection, Predicate<T> match)
    {
        if (collection.Any(item => match(item)))
        {
            throw new InvalidOperationException("Expected collection not to contain a matching item.");
        }
    }

    private static void AssertEnumerablesEqual(IEnumerable expected, IEnumerable actual)
    {
        object?[] expectedArray = expected.Cast<object?>().ToArray();
        object?[] actualArray = actual.Cast<object?>().ToArray();

        if (expectedArray.Length != actualArray.Length)
        {
            throw new InvalidOperationException($"Expected sequence length {expectedArray.Length}, but found {actualArray.Length}.");
        }

        for (int index = 0; index < expectedArray.Length; index++)
        {
            if (!Equals(expectedArray[index], actualArray[index]))
            {
                throw new InvalidOperationException(
                    $"Expected element '{expectedArray[index]}' at index {index}, but found '{actualArray[index]}'.");
            }
        }
    }
}
