using liv.Core;

namespace liv.Core.Tests;

public class ImageFileScannerTests
{
    // ---- IsSupported ----------------------------------------------------

    [Theory]
    [InlineData("photo.jpg", true)]
    [InlineData("photo.jpeg", true)]
    [InlineData("photo.JPG", true)]
    [InlineData("photo.Png", true)]
    [InlineData("photo.bmp", true)]
    [InlineData("photo.gif", true)]
    [InlineData("photo.tiff", true)]
    [InlineData("photo.tif", true)]
    [InlineData("photo.webp", true)]
    [InlineData("photo.ico", true)]
    [InlineData("photo.jfif", true)]
    [InlineData("document.pdf", false)]
    [InlineData("readme.txt", false)]
    [InlineData("video.mp4", false)]
    [InlineData("noext", false)]
    [InlineData("", false)]
    public void IsSupported_ReturnsExpected(string path, bool expected)
    {
        Assert.Equal(expected, ImageFileScanner.IsSupported(path));
    }

    // ---- GetSupportedExtensions -----------------------------------------

    [Fact]
    public void GetSupportedExtensions_ContainsCommonFormats()
    {
        var exts = ImageFileScanner.GetSupportedExtensions();

        Assert.Contains(".jpg", exts);
        Assert.Contains(".png", exts);
        Assert.Contains(".bmp", exts);
        Assert.Contains(".gif", exts);
    }

    // ---- ScanFolder (requires temp directory) ---------------------------

    [Fact]
    public void ScanFolder_NonExistentPath_ReturnsEmpty()
    {
        var result = ImageFileScanner.ScanFolder(@"C:\__nonexistent_folder_for_test__");

        Assert.Empty(result);
    }

    [Fact]
    public void ScanFolder_ReturnsOnlySupportedFiles_SortedAlphabetically()
    {
        var tempDir = CreateTempDirWithFiles("c.jpg", "a.png", "b.txt", "d.bmp", "e.cs");
        try
        {
            var result = ImageFileScanner.ScanFolder(tempDir);

            Assert.Equal(3, result.Count);
            Assert.Equal("a.png", Path.GetFileName(result[0]));
            Assert.Equal("c.jpg", Path.GetFileName(result[1]));
            Assert.Equal("d.bmp", Path.GetFileName(result[2]));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ScanFolder_EmptyDirectory_ReturnsEmpty()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = ImageFileScanner.ScanFolder(tempDir);
            Assert.Empty(result);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ScanFolder_CaseInsensitiveSorting()
    {
        var tempDir = CreateTempDirWithFiles("B.jpg", "a.jpg", "C.jpg");
        try
        {
            var result = ImageFileScanner.ScanFolder(tempDir);

            Assert.Equal("a.jpg", Path.GetFileName(result[0]));
            Assert.Equal("B.jpg", Path.GetFileName(result[1]));
            Assert.Equal("C.jpg", Path.GetFileName(result[2]));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    // ---- Helpers --------------------------------------------------------

    private static string CreateTempDirWithFiles(params string[] fileNames)
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        foreach (var name in fileNames)
        {
            File.WriteAllText(Path.Combine(dir, name), "");
        }
        return dir;
    }
}
