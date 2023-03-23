# liv ("Lightweight image viewer")

Lightweight image viewer built with [Qt](https://www.qt.io/).

## Build

To build the application, Qt needs to be installed with version >= 6.2. 
Then build it with CMake (in subfolder `build`): 

```
cmake -S . -B build
cmake --build build
```

In case Qt was installed with the installer, you need to set the `Qt6_DIR` 
shell variable to the installation path. E.g. when Qt version 6.4.2 was 
installed to `~/Qt`:

```
Qt6_DIR=~/Qt/6.4.2/gcc_64/ cmake -S . -B build
cmake --build build
```

## Usage

Run the application by providing a folder with pictures or a picture file directly:

```
liv <folder|image file>
```

With arrow keys you can now step forward and backward through all the (supported)
picture files in the corresponding directory (or the given image file's directory).
