using liv.Services;
using Xunit;

namespace liv.Tests;

public class ImageBufferTests : IDisposable
{
    private readonly string _testImagesPath;

    public ImageBufferTests()
    {
        // Create a temporary directory for test images
        _testImagesPath = Path.Combine(Path.GetTempPath(), "liv_tests");
        Directory.CreateDirectory(_testImagesPath);
    }

    [Fact]
    public void Initialize_WithValidPath_LoadsImage()
    {
        // Arrange
        using var buffer = new ImageBuffer();
        var testImagePath = CreateTestImage("test1.png");

        // Act
        buffer.Initialize(testImagePath);

        // Assert
        Assert.NotNull(buffer.CurrentImage);
        Assert.Equal(testImagePath, buffer.CurrentFilePath);
    }

    [Fact]
    public async Task MoveNext_WithMultipleImages_MovesToNextImage()
    {
        // Arrange
        using var buffer = new ImageBuffer();
        var testImage1 = CreateTestImage("a.png");
        var testImage2 = CreateTestImage("b.png");
        buffer.Initialize(testImage1);
        var sortedFiles = buffer.GetType().GetField("_imageFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(buffer) as List<string>;
        Assert.NotNull(sortedFiles);
        var initialIndex = sortedFiles.IndexOf(buffer.CurrentFilePath!);
        var expectedIndex = (initialIndex + 1) % sortedFiles.Count;

        // Act
        await buffer.MoveNextAsync();

        // Assert
        Assert.NotEqual(sortedFiles[initialIndex], buffer.CurrentFilePath);
        Assert.Equal(sortedFiles[expectedIndex], buffer.CurrentFilePath);
    }

    [Fact]
    public async Task MovePrevious_WithMultipleImages_MovesToPreviousImage()
    {
        // Arrange
        using var buffer = new ImageBuffer();
        var testImage1 = CreateTestImage("a.png");
        var testImage2 = CreateTestImage("b.png");
        buffer.Initialize(testImage2);
        var sortedFiles = buffer.GetType().GetField("_imageFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(buffer) as List<string>;
        Assert.NotNull(sortedFiles);
        var initialIndex = sortedFiles.IndexOf(buffer.CurrentFilePath!);
        var expectedIndex = (initialIndex - 1 + sortedFiles.Count) % sortedFiles.Count;

        // Act
        await buffer.MovePreviousAsync();

        // Assert
        Assert.NotEqual(sortedFiles[initialIndex], buffer.CurrentFilePath);
        Assert.Equal(sortedFiles[expectedIndex], buffer.CurrentFilePath);
    }

    private string CreateTestImage(string fileName)
    {
        var uniqueName = Guid.NewGuid().ToString() + "_" + fileName;
        var path = Path.Combine(_testImagesPath, uniqueName);
        using var bitmap = new Bitmap(100, 100);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        return path;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testImagesPath))
                Directory.Delete(_testImagesPath, true);
        }
        catch { }
    }
}