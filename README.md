# Computer Graphics

A collection of C# and WPF applications exploring fundamental computer graphics, image processing and geometric transformation techniques. The repository contains coursework exercises implemented as separate desktop applications, with the algorithms written directly in C#.

## Contents

- **Lab 1 — Drawing primitives:** interactive creation and editing of lines, rectangles and circles, with JSON save and load support.
- **Lab 2 — PPM image viewer:** decoding of P3 and P6 PPM files, pixel inspection and JPEG export with configurable quality.
- **Lab 3 — Colour models:** RGB–CMYK conversion and an interactive 3D RGB colour cube with slice visualization.
- **Lab 4 — Image filtering:** arithmetic and brightness transformations, grayscale conversion, smoothing, median filtering, Sobel edge detection, sharpening, Gaussian blur and custom convolution kernels.
- **Lab 5 — Histograms and binarization:** histogram stretching and equalization together with manual, percent-black, iterative mean, entropy, minimum-error and fuzzy minimum-error thresholding.
- **Lab 6 — Curves and rasterization:** Bresenham line drawing and interactive Bézier curve construction using configurable control points.
- **Lab 7 — 2D transformations:** polygon creation, vertex editing, translation, rotation and scaling, with JSON persistence.
- **Lab 9 — Image analysis:** HSV range selection, live colour segmentation and calculation of the percentage of matching pixels.

## Technology stack

- C#
- .NET 8 and .NET 10
- Windows Presentation Foundation (WPF)
- XAML
- `WriteableBitmap` pixel-buffer processing

## Repository structure

Each `GK.Lab*` directory is an independent WPF project. Shared sample images and PPM files are stored in `img/`.

## Running the applications

The projects target Windows because they use WPF. Open the selected `.csproj` file in Visual Studio on Windows, or build it with a compatible .NET SDK:

```powershell
dotnet run --project GK.Lab1/GK.Lab1.csproj
```

Replace `GK.Lab1` with the lab you want to run. Labs 1–2 target .NET 8, while the later projects currently target .NET 10.

## Project status

This repository is a completed collection of computer graphics coursework. The applications are intended for local Windows use and are not deployed services.

## Author

Developed by [Michał Grochowski](https://github.com/grochochotowski).
