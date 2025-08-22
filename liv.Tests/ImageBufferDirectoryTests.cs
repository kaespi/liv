using liv.Services;
using Xunit;

namespace liv.Tests;

public class ImageBufferDirectoryTests : IDisposable
{
    private readonly string _testImagesPath;

    public ImageBufferDirectoryTests()
    {
        // Create a temporary directory for test images
        _testImagesPath = Path.Combine(Path.GetTempPath(), "liv_directory_tests");
        Directory.CreateDirectory(_testImagesPath);
    }

    [Fact]
    public void InitializeFromDirectory_WithValidImages_LoadsFirstImage()
    {
        // Arrange
        using var buffer = new ImageBuffer();
        var testImage1 = CreateTestImage("a.png");
        var testImage2 = CreateTestImage("b.png");
        var directory = Path.GetDirectoryName(testImage1)!;

        // Act
        var result = buffer.InitializeFromDirectory(directory);

        // Assert
        Assert.True(result);
        Assert.NotNull(buffer.CurrentImage);
        Assert.Equal(testImage1, buffer.CurrentFilePath); // Should load first image alphabetically
    }

    [Fact]
    public void InitializeFromDirectory_WithNoImages_ReturnsFalse()
    {
        // Arrange
        using var buffer = new ImageBuffer();
        var emptyDir = Path.Combine(_testImagesPath, "empty");
        Directory.CreateDirectory(emptyDir);

        // Act
        var result = buffer.InitializeFromDirectory(emptyDir);

        // Assert
        Assert.False(result);
        Assert.Null(buffer.CurrentImage);
        Assert.Null(buffer.CurrentFilePath);
    }

    [Fact]
    public void InitializeFromDirectory_WithInvalidDirectory_ThrowsException()
    {
        // Arrange
        using var buffer = new ImageBuffer();
        var invalidDir = Path.Combine(_testImagesPath, "nonexistent");

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => buffer.InitializeFromDirectory(invalidDir));
    }

    [Fact]
    public void InitializeFromDirectory_WithEmptyPath_ThrowsException()
    {
        // Arrange
        using var buffer = new ImageBuffer();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => buffer.InitializeFromDirectory(string.Empty));
    }

    private string CreateTestImage(string fileName)
    {
        var path = Path.Combine(_testImagesPath, fileName);
        using var bitmap = new Bitmap(100, 100);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        bitmap.Save(path);
        return path;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testImagesPath))
                Directory.Delete(_testImagesPath, true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}