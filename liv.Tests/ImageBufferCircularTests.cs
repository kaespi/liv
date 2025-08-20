using liv.Services;
using Xunit;

namespace liv.Tests;

public class ImageBufferCircularTests : IDisposable
{
    private readonly string _testImagesPath;
    private readonly List<string> _createdFiles;

    public ImageBufferCircularTests()
    {
        _testImagesPath = Path.Combine(Path.GetTempPath(), "liv_circular_tests");
        Directory.CreateDirectory(_testImagesPath);
        _createdFiles = new List<string>();
    }

    [Fact]
    public async Task MoveCircular_DoesNotThrow()
    {
        // Clear the test folder before running the test
        foreach (var file in Directory.GetFiles(_testImagesPath))
        {
            try { File.Delete(file); } catch { }
        }

        // Arrange - Create all test images first
        var imgA = CreateTestImage("a.png");
        var imgB = CreateTestImage("b.png");
        var imgC = CreateTestImage("c.png");

        try
        {
            using var buffer = new ImageBuffer();
            buffer.Initialize(imgA);
            var imageFiles = buffer.GetType().GetField("_imageFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(buffer) as List<string>;
            Assert.NotNull(imageFiles);

            // Sort files by filename (a_GUID.png, b_GUID.png, c_GUID.png)
            var sortedFiles = imageFiles.OrderBy(f => Path.GetFileName(f)).ToList();

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
        finally
        {
            // Clean up individual test files immediately
            foreach (var file in _createdFiles)
            {
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch { }
            }
        }
    }

    private string CreateTestImage(string fileName)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);
        var uniqueName = nameWithoutExt + "_" + Guid.NewGuid().ToString() + ext;
        var path = Path.Combine(_testImagesPath, uniqueName);
        using var bitmap = new Bitmap(100, 100);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        _createdFiles.Add(path); // Track created files for cleanup
        return path;
    }

    public void Dispose()
    {
        try
        {
            // Clean up any remaining files and the directory
            foreach (var file in _createdFiles)
            {
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch { }
            }

            if (Directory.Exists(_testImagesPath))
                Directory.Delete(_testImagesPath, true);
        }
        catch { }
    }
}