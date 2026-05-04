using System.Reflection;

namespace BankApp.Server.Tests;

public static class Program
{
    public static int Main()
    {
        IReadOnlyList<TestCase> testCases = DiscoverTests();
        int passedCount = 0;
        List<string> failures = new ();

        foreach (TestCase testCase in testCases)
        {
            try
            {
                object? instance = Activator.CreateInstance(testCase.testClass);
                testCase.method.Invoke(instance, Array.Empty<object>());
                Console.WriteLine($"PASS {testCase.testClass.Name}.{testCase.method.Name}");
                passedCount++;
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                failures.Add($"{testCase.testClass.Name}.{testCase.method.Name}: {exception.InnerException.Message}");
                Console.WriteLine($"FAIL {testCase.testClass.Name}.{testCase.method.Name}");
                Console.WriteLine(exception.InnerException.Message);
            }
            catch (Exception exception)
            {
                failures.Add($"{testCase.testClass.Name}.{testCase.method.Name}: {exception.Message}");
                Console.WriteLine($"FAIL {testCase.testClass.Name}.{testCase.method.Name}");
                Console.WriteLine(exception.Message);
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Executed {testCases.Count} tests: {passedCount} passed, {failures.Count} failed.");

        if (failures.Count == 0)
        {
            return 0;
        }

        Console.WriteLine("Failures:");
        foreach (string failure in failures)
        {
            Console.WriteLine(failure);
        }

        return 1;
    }

    private static IReadOnlyList<TestCase> DiscoverTests()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsClass && type.Namespace == typeof(Program).Namespace)
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(method => method.GetParameters().Length == 0 &&
                                 method.GetCustomAttribute<Xunit.FactAttribute>() != null)
                .Select(method => new TestCase(type, method)))
            .OrderBy(testCase => testCase.testClass.Name, StringComparer.Ordinal)
            .ThenBy(testCase => testCase.method.Name, StringComparer.Ordinal)
            .ToList();
    }

    private sealed record TestCase(Type testClass, MethodInfo method);
}
