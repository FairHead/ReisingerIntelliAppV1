using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Model.Models;

namespace ReisingerIntelliAppV1.Services
{
    public class PdfConversionService
    {
        private readonly IPdfToPngConverter _pdfToPngConverter;

        public PdfConversionService(IPdfToPngConverter pdfToPngConverter)
        {
            _pdfToPngConverter = pdfToPngConverter;
        }

        /// <summary>
        /// Converts a floor's PDF to PNG if needed and updates the Floor model
        /// </summary>
        public async Task<bool> ConvertFloorPdfToPngAsync(Floor floor)
        {
            // Check if conversion is needed
            if (floor == null || string.IsNullOrEmpty(floor.PdfPath) || !File.Exists(floor.PdfPath))
            {
                return false;
            }

            // If PNG already exists and is newer than PDF, no need to convert
            if (!string.IsNullOrEmpty(floor.PngPath) &&
                File.Exists(floor.PngPath) &&
                File.GetLastWriteTime(floor.PngPath) >= File.GetLastWriteTime(floor.PdfPath))
            {
                return true;
            }

            try
            {
                // Define output directory
                var outputDirectory = Path.Combine(FileSystem.AppDataDirectory, "FloorPlans");

                // Convert PDF to PNG
                var pngPath = await _pdfToPngConverter.ConvertPdfToPngAsync(floor.PdfPath, outputDirectory);

                if (!string.IsNullOrEmpty(pngPath) && File.Exists(pngPath))
                {
                    // Update the floor model with the new PNG path
                    floor.PngPath = pngPath;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting PDF: {ex.Message}");
                return false;
            }
        }
    }
}