using Android.App;
using Android.Graphics;
using Android.Graphics.Pdf;
using Android.Net;
using Android.OS;
using Java.IO;
using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Services;
using IOPath = System.IO.Path; // Alias to resolve ambiguity
using AndroidColor = Android.Graphics.Color; // Alias to resolve ambiguity



[assembly: Dependency(typeof(ReisingerIntelliAppV1.Platforms.Android.PdfToPngConverter_Android))]

namespace ReisingerIntelliAppV1.Platforms.Android
{
    public class PdfToPngConverter_Android : IPdfToPngConverter
    {

       
            public async Task<string> ConvertPdfToPngAsync(string pdfFilePath, string outputDir)
            {
                try
                {
                    // Ensure output directory exists
                    if (!System.IO.Directory.Exists(outputDir))
                    {
                        System.IO.Directory.CreateDirectory(outputDir);
                    }

                    // Open the PDF file
                    using var fileDescriptor = ParcelFileDescriptor.Open(
                        new Java.IO.File(pdfFilePath), ParcelFileMode.ReadOnly);

                    // Create PDF renderer
                    using var pdfRenderer = new PdfRenderer(fileDescriptor);

                    // Get page count
                    int pageCount = pdfRenderer.PageCount;
                    if (pageCount == 0) return null;

                    // Get file name without extension for output file
                    string baseFileName = IOPath.GetFileNameWithoutExtension(pdfFilePath);

                    // Process only the first page
                    using var page = pdfRenderer.OpenPage(0);

                    // Create a high-resolution bitmap (2x scale for better quality)
                    int width = (int)(page.Width * 2.0);
                    int height = (int)(page.Height * 2.0);
                    using var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

                    // White background
                    bitmap.EraseColor(AndroidColor.White);

                    // Render the page
                    page.Render(bitmap, null, null, PdfRenderMode.ForDisplay);

                    // Define output file path
                    string outputFilePath = IOPath.Combine(outputDir, $"{baseFileName}.png");

                    // Save as PNG
                    using (var stream = new System.IO.FileStream(outputFilePath, System.IO.FileMode.Create))
                    {
                        await bitmap.CompressAsync(Bitmap.CompressFormat.Png, 100, stream);
                    }

                    return outputFilePath;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"PDF conversion error: {ex}");
                    return null;
                }
            }
        

    }
}