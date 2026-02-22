namespace liv.Core;

/// <summary>
/// Scans a directory for supported image files and provides format-filtering utilities.
/// </summary>
public static class ImageFileScanner
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".webp", ".ico", ".jfif"
    };

    /// <summary>
    /// Returns all supported image files in the specified folder, sorted alphabetically
    /// by file name (case-insensitive). Returns an empty list if the folder does not exist.
    /// </summary>
    public static IReadOnlyList<string> ScanFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return Array.Empty<string>();

        return Directory.EnumerateFiles(folderPath)
            .Where(f => IsSupported(f))
            .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Determines whether the given file path has a supported image extension.
    /// </summary>
    public static bool IsSupported(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return !string.IsNullOrEmpty(ext) && SupportedExtensions.Contains(ext);
    }

    /// <summary>
    /// Returns the set of supported file extensions (e.g. ".jpg", ".png").
    /// </summary>
    public static IReadOnlySet<string> GetSupportedExtensions() => SupportedExtensions;
}
