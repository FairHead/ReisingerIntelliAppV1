using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReisingerIntelliAppV1.Interface;

namespace ReisingerIntelliAppV1.Interface
{
    public interface IPdfToPngConverter
    {
        /// <summary>
        /// Converts a PDF file to PNG and returns the path to the generated PNG file
        /// </summary>
        /// <param name="pdfFilePath">Path to the PDF file</param>
        /// <param name="outputDir">Directory where the PNG file will be saved</param>
        /// <returns>Path to the generated PNG file, or null if conversion failed</returns>
        Task<string> ConvertPdfToPngAsync(string pdfFilePath, string outputDir);
    }
}