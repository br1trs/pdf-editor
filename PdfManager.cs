using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using ImageMagick;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using Environment = System.Environment;
using Path = System.IO.Path;

namespace PDFEditor;

public static class PdfManager
{
    private static EditorMode currentMode = EditorMode.None;
    private static List<int> PageNumbersCache = new();
    private static Texture2D previewTexture;
    private static PdfDocument openedPdfDocument;
    private static bool ispdflandscape = false;
    public static void LoadPDFToImage(string path, IProgress<float> progress)
    {
        openedPdfDocument = new PdfDocument(new PdfReader(path));
        var settings = new MagickReadSettings
        {
            Density = new Density(300, 300)
        };

        using var images = new MagickImageCollection();

        // Add all the pages of the pdf file to the collection
        images.Read(path, settings);
        int totalImages = images.Count;
        int processedImages = 0;

    // Use ConcurrentBag to store the output paths because it's thread-safe
    var outputPaths = new ConcurrentBag<string>();

    Parallel.ForEach(images, (image, state, index) =>
    {
        // Create a new MagickReadSettings instance for each thread
        var threadSettings = new MagickReadSettings
        {
            Density = new Density(300, 300)
        };

        // Create a new MagickImageCollection instance for each thread
        using var threadImages = new MagickImageCollection();

        // Add the current page to the thread's image collection
        threadImages.Add(image);

        // Write page to file that contains the page number
        string outputDirectory = Path.Combine(Environment.CurrentDirectory, "Temp");
        string outputPath = Path.Combine(outputDirectory, "temp" + (index + 1) + ".png");
        threadImages[0].Write(outputPath);

        // Add the output path to the ConcurrentBag
        outputPaths.Add(outputPath);

        // Update the progress
        Interlocked.Increment(ref processedImages);
        float progressPercentage = (float)processedImages / totalImages;
        progress.Report(progressPercentage);
    });
    }
    public static void DeletePages(string path)
    {
        PdfDocument newPdfDoc = new PdfDocument(new PdfWriter(path));
        var numberOfPages = openedPdfDocument.GetNumberOfPages();
        for (int i = 1; i <= numberOfPages; i++)
        {
            if (!PageNumbersCache.Contains(i))
            {
                openedPdfDocument.CopyPagesTo(i, i, newPdfDoc);
                
            }
        }
        newPdfDoc.Close();
    }
    public static void RearrangePages(string path, List<int> pageOrders)
    {
        PdfDocument newPdfDoc = new PdfDocument(new PdfWriter(path));
        foreach (var pageNumber in pageOrders)
        {
            openedPdfDocument.CopyPagesTo(pageNumber,pageNumber, newPdfDoc);
        }
        newPdfDoc.Close();
    }
    public static void SetCurrentMode(EditorMode mode)
    {
        currentMode = mode;
    }
    
    public static PageOrientation CheckPageOrienation(int pageNumber)
    {
        var page = openedPdfDocument.GetPage(pageNumber);
        Rectangle pageSize = page.GetPageSize();
        return pageSize.GetHeight() >= pageSize.GetWidth() ? PageOrientation.Portrait : PageOrientation.Landscape;
    }
    public static EditorMode getcurrentMode()
    {
        return currentMode;
    }
    public static void AddPageNumberToCache(int pageNumber)
    {
        PageNumbersCache.Add(pageNumber);
    }
    public static void RemovePageNumberFromCache(int pageNumber)
    {
        PageNumbersCache.Remove(pageNumber);
    }
    public static List<int> GetPageNumbersCache()
    {
        return PageNumbersCache;
    }
    public static void SetPreviewTexture(Texture2D texture)
    {
        previewTexture = texture;
    }
    public static Texture2D GetPreviewTexture()
    {
        return previewTexture;
    }
    public static bool IsPdfLandscape()
    {
        return ispdflandscape;
    }

}