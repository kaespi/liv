# liv — Lightweight Image Viewer

![GitHub](https://img.shields.io/github/license/kaespi/liv)

A blazing-fast, lightweight image viewer for Windows built with WPF on .NET.

## Features

| Feature | Description |
|---------|-------------|
| **Instant startup** | Opens maximized with the selected image displayed immediately |
| **Fast navigation** | Browse images in the same folder with ← → arrow keys or mouse clicks |
| **Prefetch buffer** | N previous + N next images are pre-loaded in the background for instant navigation |
| **Anchor-point zoom** | Mouse-wheel zoom keeps the point under the cursor fixed |
| **Pan when zoomed** | Drag with left mouse button to pan around zoomed images |
| **Fullscreen** | Toggle borderless fullscreen with F11 |
| **Quick delete** | Delete key removes the current image file and advances to the next |
| **Live folder sync** | FileSystemWatcher detects external file additions, deletions, and renames |

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `←` / `→` | Previous / Next image |
| `F11` | Toggle fullscreen |
| `Escape` | Exit fullscreen, or close the application |
| `Delete` | Delete the current image and navigate to the next |

## Mouse Controls

| Input | Action |
|-------|--------|
| **Scroll wheel** | Zoom in / out (anchored to cursor position) |
| **Left-click drag** | Pan the image (when zoomed in) |
| **Hover left / right 1/5 of window** | Shows a navigation arrow; click to navigate |

## Supported Image Formats

JPG · JPEG · PNG · BMP · GIF · TIFF · TIF · WebP · ICO · JFIF

## Usage

```
liv.exe <image-file-path>
```

To open images by double-clicking in Windows Explorer, associate `liv.exe` with the desired image file extensions.

## Building

### Prerequisites

- [.NET 8.0+ SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 (17.8+) with the **.NET desktop development** workload

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Publish (single-file, self-contained)

```bash
dotnet publish src/liv/liv.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/
```

## Architecture

The solution follows a clean separation of concerns to keep core logic testable
and the UI layer thin:

```
liv.sln
├── src/
│   ├── liv.Core/           Core logic — no UI dependencies
│   │   ├── ImageNavigator       Circular navigation through sorted file list
│   │   ├── ImageCache<T>        Generic prefetch buffer (configurable depth)
│   │   ├── ZoomController       Anchor-point zoom math & pan offset clamping
│   │   └── ImageFileScanner     Directory scanning & format filtering
│   │
│   └── liv/                 WPF application — thin UI shell
│       ├── App.xaml(.cs)         Startup & command-line handling
│       └── MainWindow.xaml(.cs)  Rendering, input, overlays
│
└── tests/
    └── liv.Core.Tests/      xUnit tests for all core components
        ├── ImageNavigatorTests
        ├── ImageCacheTests
        ├── ZoomControllerTests
        └── ImageFileScannerTests
```

### Design Decisions

| Decision | Rationale |
|----------|-----------|
| **WPF (.NET)** | Hardware-accelerated rendering via DirectX; WIC-based image decoding; first-class Visual Studio tooling |
| **`BitmapImage` with `BitmapCacheOption.OnLoad`** | File handles are released immediately; images can be frozen for cross-thread use |
| **`RenderTransform` for zoom/pan** | GPU-driven scaling — no re-decode on zoom; smooth pan via `TranslateTransform` |
| **Generic `ImageCache<T>`** | Core logic stays UI-framework-agnostic; WPF app instantiates `ImageCache<BitmapSource>` |
| **`FileSystemWatcher`** | Automatically refreshes the file list when the user (or another process) modifies the folder |

## Configuration

The prefetch buffer size is set in `MainWindow.xaml.cs`:

```csharp
_cache = new ImageCache<BitmapSource>(bufferSize: 3, LoadBitmapAsync);
```

Change `3` to prefetch more or fewer neighbours (e.g. `5` for 5 previous + 5 next).

## License

MIT
