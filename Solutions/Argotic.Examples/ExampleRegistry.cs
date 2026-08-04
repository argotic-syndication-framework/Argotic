using System.Reflection;
using System.Text;

namespace Argotic.Examples;

/// <summary>
/// Represents information about an executable example.
/// </summary>
/// <param name="Name">The display name of the example.</param>
/// <param name="Description">A description of what the example demonstrates.</param>
/// <param name="MethodName">The method name for reference.</param>
/// <param name="IsAsync">Whether the example method is asynchronous.</param>
/// <param name="RunAsync">A function to execute the example.</param>
internal sealed record ExampleInfo(string Name, string Description, string MethodName, bool IsAsync, Func<Task> RunAsync);

/// <summary>
/// Represents a category of examples.
/// </summary>
/// <param name="Name">The display name of the category.</param>
/// <param name="Description">A description of the category.</param>
/// <param name="Key">A unique key for the category.</param>
internal sealed record ExampleCategory(string Name, string Description, string Key);

/// <summary>
/// Discovers and catalogs example methods from the Argotic.Examples assembly.
/// </summary>
internal static class ExampleRegistry
{
    private static readonly Dictionary<string, List<(Type Type, MethodInfo Method)>> CategoryMethods = new(StringComparer.OrdinalIgnoreCase);
    private static bool isInitialized;

    /// <summary>
    /// Gets the available example categories.
    /// </summary>
    public static IReadOnlyList<ExampleCategory> Categories { get; } =
    [
        new("Common", "Framework utilities and interfaces", "Common"),
        new("Atom", "Atom 1.0 syndication format", "Atom"),
        new("RSS", "RSS 2.0 syndication format", "Rss"),
        new("OPML", "Outline Processor Markup Language", "Opml"),
        new("APML", "Attention Profiling Markup Language", "Apml"),
        new("BlogML", "Blog export/import format", "BlogML"),
        new("RSD", "Really Simple Discovery", "Rsd"),
        new("Network", "Trackback and XML-RPC protocols", "Net"),
        new("Generic", "Format-agnostic syndication", "Generic"),
        new("Sitemap", "XML Sitemap protocol", "Sitemap"),
        new("Extensions", "Syndication extensions (iTunes, Dublin Core, etc.)", "Extensions")
    ];

    /// <summary>
    /// Initializes the registry by discovering all example methods.
    /// </summary>
    public static void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        Assembly assembly = typeof(ExampleRegistry).Assembly;

        // Example classes are declared `internal static`, so IsPublic excludes every one of them.
        // Filter on top-level instead, which keeps the "not a nested helper" intent and also skips
        // compiler-generated types.
        List<Type> exampleTypes = [.. assembly.GetTypes().Where(t => t.IsClass && !t.IsNested && t.Name.EndsWith("Example", StringComparison.Ordinal))];

        foreach (Type type in exampleTypes)
        {
            string category = DetermineCategory(type);
            List<MethodInfo> methods = [.. type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name.EndsWith("Example", StringComparison.Ordinal) ||
                           m.Name.EndsWith("ExampleAsync", StringComparison.Ordinal))];

            if (methods.Count > 0)
            {
                if (!CategoryMethods.TryGetValue(category, out List<(Type Type, MethodInfo Method)>? list))
                {
                    list = [];
                    CategoryMethods[category] = list;
                }

                foreach (MethodInfo method in methods)
                {
                    list.Add((type, method));
                }
            }
        }

        isInitialized = true;
    }

    /// <summary>
    /// Gets all examples for a given category.
    /// </summary>
    /// <param name="categoryKey">The category key to filter by.</param>
    /// <returns>A list of example information for the category.</returns>
    public static IReadOnlyList<ExampleInfo> GetExamples(string categoryKey)
    {
        Initialize();

        if (!CategoryMethods.TryGetValue(categoryKey, out List<(Type Type, MethodInfo Method)>? methods))
        {
            return [];
        }

        List<ExampleInfo> examples = [];

        foreach ((Type type, MethodInfo method) in methods)
        {
            string name = FormatExampleName(type, method);
            string description = GetMethodDescription(type, method);
            bool isAsync = method.ReturnType == typeof(Task) ||
                           (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));

            Task runAsync() => ExecuteMethodAsync(method);

            examples.Add(new ExampleInfo(name, description, method.Name, isAsync, runAsync));
        }

        return examples.OrderBy(e => e.Name).ToList();
    }

    /// <summary>
    /// Gets all examples as a flat list.
    /// </summary>
    /// <returns>A list of all example information with category.</returns>
    public static IReadOnlyList<(string Category, ExampleInfo Example)> GetAllExamples()
    {
        Initialize();

        List<(string Category, ExampleInfo Example)> allExamples = [];

        foreach (ExampleCategory category in Categories)
        {
            IReadOnlyList<ExampleInfo> examples = GetExamples(category.Key);
            foreach (ExampleInfo example in examples)
            {
                allExamples.Add((category.Key, example));
            }
        }

        return allExamples;
    }

    /// <summary>
    /// Gets all example classes organized by category.
    /// </summary>
    /// <param name="categoryKey">The category key to filter by.</param>
    /// <returns>A dictionary mapping class names to their example methods.</returns>
    public static IReadOnlyDictionary<string, IReadOnlyList<ExampleInfo>> GetExamplesByClass(string categoryKey)
    {
        Initialize();

        if (!CategoryMethods.TryGetValue(categoryKey, out List<(Type Type, MethodInfo Method)>? methods))
        {
            return new Dictionary<string, IReadOnlyList<ExampleInfo>>();
        }

        Dictionary<string, IReadOnlyList<ExampleInfo>> grouped = methods
            .GroupBy(m => m.Type)
            .ToDictionary(
                g => FormatClassName(g.Key),
                g => (IReadOnlyList<ExampleInfo>)[.. g.Select(m =>
                {
                    bool isAsync = m.Method.ReturnType == typeof(Task) ||
                                   (m.Method.ReturnType.IsGenericType && m.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
                    return new ExampleInfo(
                        FormatMethodName(m.Method),
                        GetMethodDescription(m.Type, m.Method),
                        m.Method.Name,
                        isAsync,
                        () => ExecuteMethodAsync(m.Method));
                }).OrderBy(e => e.Name)]);

        return grouped;
    }

    private static string DetermineCategory(Type type)
    {
        string ns = type.Namespace ?? string.Empty;

        if (ns.Contains(".Extensions", StringComparison.OrdinalIgnoreCase))
        {
            return "Extensions";
        }

        if (ns.Contains(".Common", StringComparison.OrdinalIgnoreCase))
        {
            return "Common";
        }

        if (ns.Contains(".Atom", StringComparison.OrdinalIgnoreCase))
        {
            return "Atom";
        }

        if (ns.Contains(".Rss", StringComparison.OrdinalIgnoreCase))
        {
            return "Rss";
        }

        if (ns.Contains(".Opml", StringComparison.OrdinalIgnoreCase))
        {
            return "Opml";
        }

        if (ns.Contains(".Apml", StringComparison.OrdinalIgnoreCase))
        {
            return "Apml";
        }

        if (ns.Contains(".BlogML", StringComparison.OrdinalIgnoreCase))
        {
            return "BlogML";
        }

        if (ns.Contains(".Rsd", StringComparison.OrdinalIgnoreCase))
        {
            return "Rsd";
        }

        if (ns.Contains(".Net", StringComparison.OrdinalIgnoreCase))
        {
            return "Net";
        }

        if (ns.Contains(".Sitemap", StringComparison.OrdinalIgnoreCase))
        {
            return "Sitemap";
        }

        // Check type name for Generic
        if (type.Name.StartsWith("Generic", StringComparison.OrdinalIgnoreCase))
        {
            return "Generic";
        }

        // Default to Common for anything in Core without a specific subfolder
        return "Common";
    }

    private static string FormatClassName(Type type)
    {
        string name = type.Name;
        if (name.EndsWith("Example", StringComparison.Ordinal))
        {
            name = name[..^7]; // Remove "Example" suffix
        }

        return AddSpacesToPascalCase(name);
    }

    private static string FormatExampleName(Type type, MethodInfo method)
    {
        string className = FormatClassName(type);
        string methodName = FormatMethodName(method);
        string category = DetermineCategory(type);

        // Strip redundant category prefix from class name
        // e.g., "Atom Category" in "Atom" category → "Category"
        string categoryPrefix = AddSpacesToPascalCase(category) + " ";
        if (className.StartsWith(categoryPrefix, StringComparison.OrdinalIgnoreCase))
        {
            className = className[categoryPrefix.Length..];
        }

        return $"{className} - {methodName}";
    }

    private static string FormatMethodName(MethodInfo method)
    {
        string name = method.Name;

        // Remove common suffixes
        if (name.EndsWith("ExampleAsync", StringComparison.Ordinal))
        {
            name = name[..^12];
        }
        else if (name.EndsWith("Example", StringComparison.Ordinal))
        {
            name = name[..^7];
        }
        else if (name.EndsWith("Async", StringComparison.Ordinal))
        {
            name = name[..^5];
        }

        return AddSpacesToPascalCase(name);
    }

    private static string AddSpacesToPascalCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        StringBuilder result = new();
        result.Append(text[0]);

        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && !char.IsUpper(text[i - 1]))
            {
                result.Append(' ');
            }
            else if (char.IsUpper(text[i]) && i + 1 < text.Length && !char.IsUpper(text[i + 1]))
            {
                result.Append(' ');
            }

            result.Append(text[i]);
        }

        return result.ToString();
    }

    private static string GetMethodDescription(Type type, MethodInfo method)
    {
        // Generate a description based on the method name and type
        string className = type.Name.Replace("Example", string.Empty, StringComparison.Ordinal);
        string methodName = method.Name
            .Replace("ExampleAsync", string.Empty, StringComparison.Ordinal)
            .Replace("Example", string.Empty, StringComparison.Ordinal)
            .Replace("Async", string.Empty, StringComparison.Ordinal);

        return methodName switch
        {
            "Class" => $"Demonstrates creating and configuring a {className}",
            "Create" => $"Creates a {className} from a URI",
            "Load" or "LoadUri" => $"Loads a {className} from a URI",
            "LoadAsync" => $"Asynchronously loads a {className} with event notification",
            "LoadStream" => $"Loads a {className} from a stream",
            "LoadXmlReader" => $"Loads a {className} from an XmlReader",
            "LoadIXPathNavigable" => $"Loads a {className} from an IXPathNavigable",
            "Save" or "SaveStream" => $"Saves a {className} to a stream",
            "SaveXmlWriter" => $"Saves a {className} to an XmlWriter",
            "SyndicationContentFormatGet" => "Determines the syndication format of a feed URL",
            "SourceReferencesTarget" => "Checks if a source URL references a target URL",
            "UriExists" => "Verifies if a URI exists and is accessible",
            "ConditionalGet" => "Performs a conditional GET request",
            "LocateDiscoverableSyndicationEndpoints" => "Discovers syndication endpoints from a web page",
            "IsPingbackEnabled" => "Checks if pingback is enabled for a URL",
            "LocatePingbackNotificationServer" => "Locates the pingback notification server",
            "IsTrackbackEnabled" => "Checks if trackback is enabled for a URL",
            "LocateTrackbackNotificationServers" => "Locates trackback notification servers",
            _ => $"Demonstrates {AddSpacesToPascalCase(methodName).ToLowerInvariant()} functionality"
        };
    }

    private static async Task ExecuteMethodAsync(MethodInfo method)
    {
        try
        {
            object? result = method.Invoke(null, null);

            if (result is Task task)
            {
                await task.ConfigureAwait(false);
            }
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }
}