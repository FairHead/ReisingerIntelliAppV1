namespace ReisingerIntelliAppV1.Services
{
    public interface IPdfUploadService
    {
        Task<string?> PickAndSavePdfAsync();
    }
}
