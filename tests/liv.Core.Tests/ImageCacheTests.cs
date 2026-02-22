using liv.Core;

namespace liv.Core.Tests;

public class ImageCacheTests
{
    // ---- GetAsync -------------------------------------------------------

    [Fact]
    public async Task GetAsync_LoadsAndReturnsValue()
    {
        var cache = new ImageCache<string>(3, (path, ct) => Task.FromResult<string?>(path));

        var result = await cache.GetAsync("test.jpg");

        Assert.Equal("test.jpg", result);
    }

    [Fact]
    public async Task GetAsync_CachesResult_LoaderCalledOnce()
    {
        int loadCount = 0;
        var cache = new ImageCache<string>(3, (path, ct) =>
        {
            Interlocked.Increment(ref loadCount);
            return Task.FromResult<string?>(path);
        });

        await cache.GetAsync("test.jpg");
        await cache.GetAsync("test.jpg");

        Assert.Equal(1, loadCount);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenLoaderFails()
    {
        var cache = new ImageCache<string>(3, (path, ct) =>
            Task.FromException<string?>(new InvalidOperationException("fail")));

        var result = await cache.GetAsync("fail.jpg");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_RetriesAfterFailure()
    {
        int attempt = 0;
        var cache = new ImageCache<string>(3, (path, ct) =>
        {
            int current = Interlocked.Increment(ref attempt);
            if (current == 1)
                return Task.FromException<string?>(new InvalidOperationException("first try fails"));
            return Task.FromResult<string?>("ok");
        });

        var r1 = await cache.GetAsync("retry.jpg");
        Assert.Null(r1); // first attempt fails

        var r2 = await cache.GetAsync("retry.jpg");
        Assert.Equal("ok", r2); // second attempt succeeds
    }

    // ---- PrefetchAround -------------------------------------------------

    [Fact]
    public async Task PrefetchAround_LoadsNeighbours()
    {
        var loaded = new HashSet<string>();
        var cache = new ImageCache<string>(2, (path, ct) =>
        {
            lock (loaded) loaded.Add(path);
            return Task.FromResult<string?>(path);
        });

        string[] files = { "a.jpg", "b.jpg", "c.jpg", "d.jpg", "e.jpg" };
        // current = c.jpg (index 2), buffer = 2 → should load a,b,c,d,e
        cache.PrefetchAround("c.jpg", offset =>
        {
            int idx = (2 + offset % files.Length + files.Length) % files.Length;
            return files[idx];
        });

        // Give background tasks a moment
        await Task.Delay(100);

        lock (loaded)
        {
            Assert.Contains("c.jpg", loaded);
            Assert.Contains("b.jpg", loaded);
            Assert.Contains("d.jpg", loaded);
        }
    }

    [Fact]
    public async Task PrefetchAround_EvictsDistantEntries()
    {
        var cache = new ImageCache<string>(1, (path, ct) => Task.FromResult<string?>(path));

        // Load a far-away file first
        await cache.GetAsync("far.jpg");
        Assert.Equal(1, cache.CachedCount);

        // Now prefetch around "a.jpg" with buffer size 1 → keeps a, b (next), z (prev)
        cache.PrefetchAround("a.jpg", offset => offset switch
        {
            0 => "a.jpg",
            1 => "b.jpg",
            -1 => "z.jpg",
            _ => null
        });

        // "far.jpg" should have been evicted
        // (a.jpg, b.jpg, z.jpg should remain)
        Assert.True(cache.CachedCount <= 3);
    }

    // ---- Evict ----------------------------------------------------------

    [Fact]
    public async Task Evict_RemovesSpecificEntry()
    {
        var cache = new ImageCache<string>(3, (path, ct) => Task.FromResult<string?>(path));
        await cache.GetAsync("x.jpg");

        Assert.Equal(1, cache.CachedCount);

        cache.Evict("x.jpg");

        Assert.Equal(0, cache.CachedCount);
    }

    // ---- Clear ----------------------------------------------------------

    [Fact]
    public async Task Clear_RemovesAllEntries()
    {
        var cache = new ImageCache<string>(3, (path, ct) => Task.FromResult<string?>(path));
        await cache.GetAsync("a.jpg");
        await cache.GetAsync("b.jpg");

        cache.Clear();

        Assert.Equal(0, cache.CachedCount);
    }

    // ---- Constructor validation -----------------------------------------

    [Fact]
    public void Constructor_NegativeBufferSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ImageCache<string>(-1, (_, _) => Task.FromResult<string?>(null)));
    }

    [Fact]
    public void Constructor_NullLoader_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ImageCache<string>(3, null!));
    }

    // ---- Dispose --------------------------------------------------------

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var cache = new ImageCache<string>(3, (path, ct) => Task.FromResult<string?>(path));

        cache.Dispose();
        cache.Dispose(); // no exception
    }
}
