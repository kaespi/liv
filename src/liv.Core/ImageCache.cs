using System.Collections.Concurrent;

namespace liv.Core;

/// <summary>
/// A generic prefetching cache that keeps <c>N</c> images ahead of and behind the
/// current position pre-loaded in memory. Evicts entries that fall outside the buffer window.
/// </summary>
/// <typeparam name="TImage">
/// The image representation type (e.g. <c>BitmapSource</c>).
/// Must be a reference type so <c>null</c> can signal a failed load.
/// </typeparam>
public class ImageCache<TImage> : IDisposable where TImage : class
{
    private readonly int _bufferSize;
    private readonly Func<string, CancellationToken, Task<TImage?>> _loader;
    private readonly ConcurrentDictionary<string, Task<TImage?>> _cache = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource _prefetchCts = new();
    private readonly object _prefetchLock = new();

    /// <summary>
    /// Creates a new image cache.
    /// </summary>
    /// <param name="bufferSize">
    /// Number of images to prefetch in each direction
    /// (e.g. 3 means 3 previous + 3 next = 6 prefetched neighbours).
    /// </param>
    /// <param name="loader">
    /// Async factory that loads an image from a file path.
    /// Must be thread-safe and support cancellation.
    /// </param>
    public ImageCache(int bufferSize, Func<string, CancellationToken, Task<TImage?>> loader)
    {
        if (bufferSize < 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
        _bufferSize = bufferSize;
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
    }

    /// <summary>Number of images prefetched in each direction.</summary>
    public int BufferSize => _bufferSize;

    /// <summary>Number of entries currently held in the cache.</summary>
    public int CachedCount => _cache.Count;

    /// <summary>
    /// Returns a cached image or loads it on demand. Returns <c>null</c> when loading fails.
    /// </summary>
    public async Task<TImage?> GetAsync(string filePath, CancellationToken ct = default)
    {
        var task = _cache.GetOrAdd(filePath, key => _loader(key, ct));
        try
        {
            return await task.ConfigureAwait(false);
        }
        catch
        {
            // Remove failed entries so they can be retried next time
            _cache.TryRemove(filePath, out _);
            return null;
        }
    }

    /// <summary>
    /// Triggers background prefetching of neighbours around the current position
    /// and evicts entries that are no longer within the buffer window.
    /// </summary>
    /// <param name="currentFile">The file currently being displayed.</param>
    /// <param name="peekRelative">
    /// A function that returns the file path at a given offset from the current position
    /// (e.g. +1 = next, −1 = previous). May return <c>null</c>.
    /// </param>
    public void PrefetchAround(string currentFile, Func<int, string?> peekRelative)
    {
        CancellationTokenSource newCts;
        CancellationTokenSource oldCts;

        lock (_prefetchLock)
        {
            oldCts = _prefetchCts;
            newCts = new CancellationTokenSource();
            _prefetchCts = newCts;
        }

        // Cancel any in-flight prefetch operations from the previous position
        try { oldCts.Cancel(); oldCts.Dispose(); }
        catch { /* best effort */ }

        var ct = newCts.Token;

        // Build the set of files that should be cached
        var keepers = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { currentFile };
        for (int i = 1; i <= _bufferSize; i++)
        {
            var next = peekRelative(i);
            var prev = peekRelative(-i);
            if (next != null) keepers.Add(next);
            if (prev != null) keepers.Add(prev);
        }

        // Evict files outside the buffer window
        foreach (var key in _cache.Keys)
        {
            if (!keepers.Contains(key))
                _cache.TryRemove(key, out _);
        }

        // Start loading files not yet in the cache
        foreach (var file in keepers)
        {
            _cache.GetOrAdd(file, key => _loader(key, ct));
        }
    }

    /// <summary>
    /// Removes a specific file from the cache (e.g. before deletion).
    /// </summary>
    public void Evict(string filePath) => _cache.TryRemove(filePath, out _);

    /// <summary>
    /// Clears all cached entries and cancels pending prefetch work.
    /// </summary>
    public void Clear()
    {
        lock (_prefetchLock)
        {
            try { _prefetchCts.Cancel(); } catch { }
        }
        _cache.Clear();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Clear();
        lock (_prefetchLock)
        {
            _prefetchCts.Dispose();
        }
    }
}
