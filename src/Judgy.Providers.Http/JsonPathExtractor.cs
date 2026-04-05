using System.Text.Json;

namespace Judgy.Providers.Http;

internal static class JsonPathExtractor
{
    internal static string Extract(string json, string jsonPath)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON content cannot be null or whitespace.", nameof(json));

        if (string.IsNullOrWhiteSpace(jsonPath))
            throw new ArgumentException("JSON path cannot be null or whitespace.", nameof(jsonPath));

        var segments = ParsePath(jsonPath);

        using var document = JsonDocument.Parse(json);
        var current = document.RootElement;

        foreach (var segment in segments)
        {
            current = Navigate(current, segment, jsonPath);
        }

        return current.ValueKind == JsonValueKind.String
            ? current.GetString()!
            : current.GetRawText();
    }

    private static List<PathSegment> ParsePath(string jsonPath)
    {
        var path = jsonPath.StartsWith('$') ? jsonPath[1..] : jsonPath;
        var segments = new List<PathSegment>();
        var i = 0;

        while (i < path.Length)
        {
            if (path[i] == '.')
            {
                i++;
                var start = i;
                while (i < path.Length && path[i] != '.' && path[i] != '[')
                    i++;

                if (i > start)
                    segments.Add(new PropertySegment(path[start..i]));
            }
            else if (path[i] == '[')
            {
                i++;
                var start = i;
                while (i < path.Length && path[i] != ']')
                    i++;

                if (i >= path.Length)
                    throw new InvalidOperationException($"Unterminated bracket in JSON path: '{jsonPath}'");

                var indexText = path[start..i];
                if (!int.TryParse(indexText, out var index))
                    throw new InvalidOperationException($"Invalid array index '{indexText}' in JSON path: '{jsonPath}'");

                segments.Add(new ArrayIndexSegment(index));
                i++; // skip ']'
            }
            else
            {
                var start = i;
                while (i < path.Length && path[i] != '.' && path[i] != '[')
                    i++;

                if (i > start)
                    segments.Add(new PropertySegment(path[start..i]));
            }
        }

        return segments;
    }

    private static JsonElement Navigate(JsonElement current, PathSegment segment, string fullPath)
    {
        return segment switch
        {
            PropertySegment prop => NavigateProperty(current, prop.Name, fullPath),
            ArrayIndexSegment arr => NavigateArray(current, arr.Index, fullPath),
            _ => throw new InvalidOperationException($"Unknown path segment type in JSON path: '{fullPath}'")
        };
    }

    private static JsonElement NavigateProperty(JsonElement element, string propertyName, string fullPath)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException(
                $"Expected JSON object for property '{propertyName}' but found {element.ValueKind} in path: '{fullPath}'");

        if (!element.TryGetProperty(propertyName, out var value))
            throw new InvalidOperationException(
                $"Property '{propertyName}' not found in JSON path: '{fullPath}'");

        return value;
    }

    private static JsonElement NavigateArray(JsonElement element, int index, string fullPath)
    {
        if (element.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException(
                $"Expected JSON array for index [{index}] but found {element.ValueKind} in path: '{fullPath}'");

        var length = element.GetArrayLength();
        if (index < 0 || index >= length)
            throw new InvalidOperationException(
                $"Array index [{index}] out of range (length: {length}) in path: '{fullPath}'");

        return element[index];
    }

    private abstract record PathSegment;
    private sealed record PropertySegment(string Name) : PathSegment;
    private sealed record ArrayIndexSegment(int Index) : PathSegment;
}
