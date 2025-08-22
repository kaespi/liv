namespace liv.Services;

public class ImageBuffer : IImageBuffer, IDisposable
{
    public static readonly string[] SupportedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
    private readonly int _bufferSize;
    private readonly List<string> _imageFiles = new();
    private readonly Dictionary<string, Image?> _imageCache = new();
    private int _currentIndex = -1;

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
            .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
            .OrderBy(f => f));

        // Find the index of the current file
        _currentIndex = _imageFiles.IndexOf(filePath);
        if (_currentIndex == -1)
            throw new ArgumentException("File not found in directory", nameof(filePath));

        // Load the current image and buffer
        LoadCurrentImage();
        _ = BufferImagesAsync();
    }

    public bool InitializeFromDirectory(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
            throw new ArgumentException("Directory path cannot be empty", nameof(directoryPath));

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        // Get all image files in the directory
        _imageFiles.Clear();
        _imageFiles.AddRange(Directory.GetFiles(directoryPath)
            .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
            .OrderBy(f => f));

        if (_imageFiles.Count == 0)
            return false;

        // Set to first image
        _currentIndex = 0;
        LoadCurrentImage();
        _ = BufferImagesAsync();
        return true;
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
                var img = Image.FromFile(filePath);
                ApplyExifOrientation(img);
                _imageCache[filePath] = img;
            }
            catch
            {
                _imageCache[filePath] = null;
            }
        }
        return _imageCache[filePath];
    }

    /// <summary>
    /// EXIF orientation values as per specification:
    /// https://exiftool.org/TagNames/EXIF.html#Orientation
    /// </summary>
    private enum ExifOrientation
    {
        Normal = 1,
        FlipHorizontal = 2,
        Rotate180 = 3,
        FlipVertical = 4,
        Transpose = 5,
        Rotate90 = 6,
        Transverse = 7,
        Rotate270 = 8
    }

    private static void ApplyExifOrientation(Image img)
    {
        const int ExifOrientationId = 0x0112;
        if (img.PropertyIdList.Contains(ExifOrientationId))
        {
            var prop = img.GetPropertyItem(ExifOrientationId);
            if (prop?.Value != null && prop.Value.Length > 0)
            {
                int orientation = prop.Value[0];
                switch ((ExifOrientation)orientation)
                {
                    case ExifOrientation.FlipHorizontal:
                        img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case ExifOrientation.Rotate180:
                        img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case ExifOrientation.FlipVertical:
                        img.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case ExifOrientation.Transpose:
                        img.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case ExifOrientation.Rotate90:
                        img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case ExifOrientation.Transverse:
                        img.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case ExifOrientation.Rotate270:
                        img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private async Task BufferImagesAsync()
    {
        await Task.Run(() =>
        {
            int count = _imageFiles.Count;
            if (count < 2) return; // Nothing to buffer if only one image
            int maxBuffer = Math.Min(_bufferSize, count - 1);
            for (int i = 1; i <= maxBuffer; i++)
            {
                // Buffer next images
                var nextIndex = (_currentIndex + i) % count;
                _ = GetImageFromCache(_imageFiles[nextIndex]);

                // Buffer previous images
                var prevIndex = (_currentIndex - i + count) % count;
                _ = GetImageFromCache(_imageFiles[prevIndex]);
            }
        });
    }

    public void Dispose()
    {
        foreach (var image in _imageCache.Values.ToList())
        {
            image?.Dispose();
        }
        _imageCache.Clear();
        GC.SuppressFinalize(this);
    }
}