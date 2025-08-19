using liv.Services;
using Xunit;

namespace liv.Tests;

public class ImageBufferCircularTests : IDisposable
{
    private readonly string _testImagesPath;

    public ImageBufferCircularTests()
    {
        _testImagesPath = Path.Combine(Path.GetTempPath(), "liv_circular_tests");
        Directory.CreateDirectory(_testImagesPath);
    }

    [Fact]
    public async Task MoveCircular_DoesNotThrow()
    {
        // Arrange
        using var buffer = new ImageBuffer();
        var imgA = CreateTestImage("a.png");
        var imgB = CreateTestImage("b.png");
        var imgC = CreateTestImage("c.png");
        buffer.Initialize(imgA);
        var sortedFiles = buffer.GetType().GetField("_imageFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(buffer) as List<string>;
        Assert.NotNull(sortedFiles);

        // Move to last image (c)
        await buffer.MoveNextAsync(); // b
        await buffer.MoveNextAsync(); // c

        // Move next (should wrap to first)
        Exception? ex = await Record.ExceptionAsync(() => buffer.MoveNextAsync());
        Assert.Null(ex);
        Assert.Equal(sortedFiles[0], buffer.CurrentFilePath);

        // Move previous (should wrap to last)
        ex = await Record.ExceptionAsync(() => buffer.MovePreviousAsync());
        Assert.Null(ex);
        Assert.Equal(sortedFiles[^1], buffer.CurrentFilePath);
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