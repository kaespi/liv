namespace liv.Services;

public class ImageBuffer : IImageBuffer, IDisposable
{
    private readonly int _bufferSize;
    private readonly List<string> _imageFiles = new();
    private readonly Dictionary<string, Image?> _imageCache = new();
    private int _currentIndex = -1;
    private readonly string[] _supportedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

    public Image? CurrentImage => _currentIndex >= 0 ? GetImageFromCache(_imageFiles[_currentIndex]) : null;
    public string? CurrentFilePath => _currentIndex >= 0 ? _imageFiles[_currentIndex] : null;

    public event EventHandler<ImageBufferChangedEventArgs>? BufferChanged;

    public ImageBuffer(int bufferSize = 3)
    {
        _bufferSize = bufferSize;
    }

    public void Initialize(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(directory))
            throw new ArgumentException("Invalid file path", nameof(filePath));

        // Get all image files in the directory
        _imageFiles.Clear();
        _imageFiles.AddRange(Directory.GetFiles(directory)
            .Where(file => _supportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
            .OrderBy(f => f));

        // Find the index of the current file
        _currentIndex = _imageFiles.IndexOf(filePath);
        if (_currentIndex == -1)
            throw new ArgumentException("File not found in directory", nameof(filePath));

        // Load the current image and buffer
        LoadCurrentImage();
        _ = BufferImagesAsync();
    }

    public async Task<Image?> MoveNextAsync()
    {
        if (_imageFiles.Count == 0) return null;

        _currentIndex = (_currentIndex + 1) % _imageFiles.Count;
        LoadCurrentImage();
        await BufferImagesAsync();
        return CurrentImage;
    }

    public async Task<Image?> MovePreviousAsync()
    {
        if (_imageFiles.Count == 0) return null;

        _currentIndex = (_currentIndex - 1 + _imageFiles.Count) % _imageFiles.Count;
        LoadCurrentImage();
        await BufferImagesAsync();
        return CurrentImage;
    }

    private void LoadCurrentImage()
    {
        if (_currentIndex >= 0 && _currentIndex < _imageFiles.Count)
        {
            var image = GetImageFromCache(_imageFiles[_currentIndex]);
            BufferChanged?.Invoke(this, new ImageBufferChangedEventArgs(_imageFiles[_currentIndex], image));
        }
    }

    private Image? GetImageFromCache(string filePath)
    {
        if (_imageCache.TryGetValue(filePath, out var cachedImage))
        {
            // Check if cached image is disposed or invalid
            try
            {
                var _ = cachedImage?.Width; // Access property to check validity
            }
            catch
            {
                // Remove invalid/disposed image from cache
                if (cachedImage != null)
                {
                    cachedImage.Dispose();
                }
                _imageCache.Remove(filePath);
                cachedImage = null;
            }
        }
        if (!_imageCache.ContainsKey(filePath) || _imageCache[filePath] == null)
        {
            try
            {
                _imageCache[filePath] = Image.FromFile(filePath);
            }
            catch
            {
                _imageCache[filePath] = null;
            }
        }
        return _imageCache[filePath];
    }

    private async Task BufferImagesAsync()
    {
        await Task.Run(() =>
        {
            for (int i = 1; i <= _bufferSize; i++)
            {
                // Buffer next images
                var nextIndex = (_currentIndex + i) % _imageFiles.Count;
                _ = GetImageFromCache(_imageFiles[nextIndex]);

                // Buffer previous images
                var prevIndex = (_currentIndex - i + _imageFiles.Count) % _imageFiles.Count;
                _ = GetImageFromCache(_imageFiles[prevIndex]);
            }
        });
    }

    public void Dispose()
    {
        foreach (var image in _imageCache.Values)
        {
            image?.Dispose();
        }
        _imageCache.Clear();
        GC.SuppressFinalize(this);
    }
}