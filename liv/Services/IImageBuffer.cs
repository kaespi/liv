namespace liv.Services;

public interface IImageBuffer
{
    /// <summary>
    /// Gets the current image being displayed
    /// </summary>
    Image? CurrentImage { get; }
    
    /// <summary>
    /// Initializes the buffer with the given file path
    /// </summary>
    /// <param name="filePath">Path to the image file to start with</param>
    void Initialize(string filePath);

    /// <summary>
    /// Initializes the buffer with the first image from the given directory
    /// </summary>
    /// <param name="directoryPath">Path to the directory containing images</param>
    /// <returns>True if initialization was successful, false if no valid images were found</returns>
    bool InitializeFromDirectory(string directoryPath);
    
    /// <summary>
    /// Moves to the next image in the directory
    /// </summary>
    /// <returns>The next image</returns>
    Task<Image?> MoveNextAsync();
    
    /// <summary>
    /// Moves to the previous image in the directory
    /// </summary>
    /// <returns>The previous image</returns>
    Task<Image?> MovePreviousAsync();
    
    /// <summary>
    /// Gets the current file path
    /// </summary>
    string? CurrentFilePath { get; }
    
    /// <summary>
    /// Event raised when the buffer content changes
    /// </summary>
    event EventHandler<ImageBufferChangedEventArgs>? BufferChanged;
}