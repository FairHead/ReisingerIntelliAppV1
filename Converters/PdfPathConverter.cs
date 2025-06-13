using System.Globalization;

namespace ReisingerIntelliAppV1.Converters
{
    public class PdfPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine($"[PdfPathConverter] Converting value: {value?.ToString() ?? "null"}, Parameter: {parameter?.ToString() ?? "null"}");
            
            // Handle the case where we're bound to a Floor object
            if (value is ReisingerIntelliAppV1.Model.Models.Floor floor)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfPathConverter] Floor object: {floor.FloorName}, PdfPath: {floor.PdfPath ?? "null"}");
                return HandlePath(floor.PdfPath, parameter);
            }
            
            // Handle the case where we're bound directly to a string path
            if (value is string stringPath)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfPathConverter] String path: {stringPath}");
                return HandlePath(stringPath, parameter);
            }
            
            // Handle null case
            if (value == null)
            {
                System.Diagnostics.Debug.WriteLine("[PdfPathConverter] Value is null");
                if (parameter is string param)
                {
                    if (param == "IsEmpty")
                        return true; // Show empty placeholder for null
                    if (param == "HasValue")
                        return false; // Hide PDF viewer for null
                }
                return null;
            }
            
            System.Diagnostics.Debug.WriteLine($"[PdfPathConverter] Unhandled value type: {value.GetType().Name}");
            return null;
        }

        private object HandlePath(string path, object parameter)
        {
            // First check if the path is null or empty before trying to check if file exists
            if (string.IsNullOrWhiteSpace(path))
            {
                System.Diagnostics.Debug.WriteLine("[PdfPathConverter] Path is null or empty");
                
                if (parameter is string param)
                {
                    if (param == "IsEmpty")
                        return true;
                    if (param == "HasValue")
                        return false;
                }
                
                return null;
            }
            
            // Safely check if file exists
            bool pathExists = false;
            try
            {
                pathExists = File.Exists(path);
                System.Diagnostics.Debug.WriteLine($"[PdfPathConverter] Checking PDF path: {path}, Exists: {pathExists}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfPathConverter] Error checking if file exists: {ex.Message}");
                // If there's an error checking the file, assume it doesn't exist
                pathExists = false;
            }
            
            // If parameter is "IsEmpty", return true if path doesn't exist
            if (parameter is string paramString)
            {
                if (paramString == "IsEmpty")
                {
                    System.Diagnostics.Debug.WriteLine($"[PdfPathConverter] IsEmpty result: {!pathExists}");
                    return !pathExists;
                }
                if (paramString == "HasValue")
                {
                    System.Diagnostics.Debug.WriteLine($"[PdfPathConverter] HasValue result: {pathExists}");
                    return pathExists;
                }
            }
            
            // For the URI property, return the path if it exists, null otherwise
            if (pathExists)
            {
                System.Diagnostics.Debug.WriteLine($"[PdfPathConverter] Returning valid path: {path}");
                return path;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[PdfPathConverter] Path doesn't exist, returning null");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value; // Not used for one-way binding
        }
    }
}