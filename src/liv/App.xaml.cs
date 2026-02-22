using System.IO;
using System.Windows;
using liv.Core;
using Microsoft.Win32;

namespace liv;

/// <summary>
/// Application entry point. Accepts an optional command-line argument: a path to an image file
/// or a directory. When no argument is provided, a file-choose dialog is shown.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string? filePath;

        if (e.Args.Length == 0)
        {
            filePath = ShowOpenFileDialog();
            if (filePath == null)
            {
                Shutdown();
                return;
            }
        }
        else
        {
            var rawArg = e.Args[0].TrimEnd('"', '\\', '/');
            var argPath = Path.GetFullPath(rawArg);

            if (Directory.Exists(argPath))
            {
                var images = ImageFileScanner.ScanFolder(argPath);
                if (images.Count == 0)
                {
                    MessageBox.Show(
                        $"No supported image files found in:\n{argPath}",
                        "liv",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    Shutdown();
                    return;
                }
                filePath = images[0];
            }
            else if (File.Exists(argPath))
            {
                filePath = argPath;
            }
            else
            {
                MessageBox.Show(
                    $"File not found:\n{argPath}",
                    "liv",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
                return;
            }
        }

        var window = new MainWindow(filePath);
        window.Show();
    }

    private static string? ShowOpenFileDialog()
    {
        var extensions = ImageFileScanner.GetSupportedExtensions();
        var wildcards = string.Join(";", extensions.Select(ext => $"*{ext}"));
        var filter = $"Image files ({wildcards})|{wildcards}|All files (*.*)|*.*";

        var dialog = new OpenFileDialog
        {
            Title = "Select an image file",
            Filter = filter,
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}

