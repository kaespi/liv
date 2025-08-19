namespace liv.Services;

public class ImageBufferChangedEventArgs : EventArgs
{
    public string FilePath { get; }
    public Image? Image { get; }

    public ImageBufferChangedEventArgs(string filePath, Image? image)
    {
        FilePath = filePath;
        Image = image;
    }
}