namespace ReisingerIntelliAppV1.Services;

public class PdfUploadService : IPdfUploadService
{
    public async Task<string?> PickAndSavePdfAsync()
    {
        var pickedFile = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Wähle eine PDF",
            FileTypes = FilePickerFileType.Pdf
        });

        if (pickedFile != null)
        {
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, pickedFile.FileName);
            using var sourceStream = await pickedFile.OpenReadAsync();
            using var destStream = File.Create(targetPath);
            await sourceStream.CopyToAsync(destStream);

            return targetPath; // ← GANZER Pfad, z. B. /data/user/0/com.app/files/Detroit Harry.pdf
        }

        return null;
    }
}
