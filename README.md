# liv ("Lightweight image viewer")

![GitHub](https://img.shields.io/github/license/kaespi/liv) ![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/kaespi/liv/build.yml?label=build) ![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/kaespi/liv/tests.yml?label=tests)
 ![Coveralls](https://img.shields.io/coverallsCoverage/github/kaespi/liv)

Lightweight image viewer built with C#/.NET.

## Build

To build the application use the Visual Studio solution [liv.sln](./liv.sln). It includes the application and unittests.

## Usage

Run the application by providing a folder with pictures or a picture file directly:

```
liv.exe <image file>
```

With arrow keys you can now step forward and backward through all the (supported)
picture files in the corresponding directory (or the given image file's directory).
