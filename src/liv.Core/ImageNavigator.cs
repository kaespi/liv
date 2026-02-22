namespace liv.Core;

/// <summary>
/// Manages circular navigation through an ordered list of image file paths.
/// The list can be dynamically updated (e.g. after a file is deleted or the folder contents change).
/// </summary>
public class ImageNavigator
{
    private List<string> _files;
    private int _currentIndex;

    /// <summary>
    /// Creates a navigator from a sorted list of file paths, starting at the specified index.
    /// </summary>
    /// <param name="sortedFiles">Alphabetically sorted file paths.</param>
    /// <param name="startIndex">Index of the initially selected file.</param>
    public ImageNavigator(IReadOnlyList<string> sortedFiles, int startIndex)
    {
        _files = new List<string>(sortedFiles ?? throw new ArgumentNullException(nameof(sortedFiles)));
        _currentIndex = _files.Count > 0 ? WrapIndex(startIndex, _files.Count) : 0;
    }

    /// <summary>
    /// The currently selected file path, or <c>null</c> if the list is empty.
    /// </summary>
    public string? Current => _files.Count > 0 ? _files[_currentIndex] : null;

    /// <summary>
    /// The current index in the file list.
    /// </summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>
    /// The total number of files in the navigator.
    /// </summary>
    public int Count => _files.Count;

    /// <summary>
    /// Moves to the next file (circular). Returns the new current file, or <c>null</c> if empty.
    /// </summary>
    public string? MoveNext()
    {
        if (_files.Count == 0) return null;
        _currentIndex = WrapIndex(_currentIndex + 1, _files.Count);
        return Current;
    }

    /// <summary>
    /// Moves to the previous file (circular). Returns the new current file, or <c>null</c> if empty.
    /// </summary>
    public string? MovePrevious()
    {
        if (_files.Count == 0) return null;
        _currentIndex = WrapIndex(_currentIndex - 1, _files.Count);
        return Current;
    }

    /// <summary>
    /// Peeks at a file at a relative offset from the current position without moving the cursor.
    /// Wraps circularly. Returns <c>null</c> if the list is empty.
    /// </summary>
    public string? PeekRelative(int offset)
    {
        if (_files.Count == 0) return null;
        return _files[WrapIndex(_currentIndex + offset, _files.Count)];
    }

    /// <summary>
    /// Replaces the file list (e.g. after external changes to the folder).
    /// Tries to keep the current file selected; if it was removed, clamps to the nearest valid index.
    /// </summary>
    public void UpdateFiles(IReadOnlyList<string> sortedFiles)
    {
        var currentFile = Current;
        _files = new List<string>(sortedFiles ?? throw new ArgumentNullException(nameof(sortedFiles)));

        if (_files.Count == 0)
        {
            _currentIndex = 0;
            return;
        }

        if (currentFile != null)
        {
            var idx = _files.FindIndex(f => string.Equals(f, currentFile, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                _currentIndex = idx;
                return;
            }
        }

        // Current file was removed — clamp index to valid range
        _currentIndex = Math.Min(_currentIndex, _files.Count - 1);
    }

    /// <summary>
    /// Removes a specific file from the internal list and advances to the next image.
    /// Returns the new current file, or <c>null</c> if the list is now empty.
    /// </summary>
    public string? RemoveAndMoveNext(string filePath)
    {
        var idx = _files.FindIndex(f => string.Equals(f, filePath, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return Current;

        _files.RemoveAt(idx);
        if (_files.Count == 0)
        {
            _currentIndex = 0;
            return null;
        }

        if (idx < _currentIndex)
        {
            _currentIndex--;
        }
        else if (idx == _currentIndex)
        {
            // Stay at the same index (now points to the next file), but wrap if needed
            if (_currentIndex >= _files.Count)
                _currentIndex = 0;
        }

        return Current;
    }

    /// <summary>
    /// Wraps an index into the range [0, count) using modular arithmetic.
    /// Returns 0 when <paramref name="count"/> is zero or negative.
    /// </summary>
    public static int WrapIndex(int index, int count)
    {
        if (count <= 0) return 0;
        return ((index % count) + count) % count;
    }
}
