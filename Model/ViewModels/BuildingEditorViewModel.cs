using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace ReisingerIntelliAppV1.Model.ViewModels;

public partial class BuildingEditorViewModel : ObservableObject
{
    private readonly PdfConversionService _pdfConversionService;
    [ObservableProperty]
    private string buildingName = string.Empty;
    [ObservableProperty]
    private Floor currentFloor;

    [ObservableProperty]
    private ObservableCollection<Floor> floors = new();

    [ObservableProperty]
    private string pageTitle = "Gebäude hinzufügen";

    public BuildingEditorViewModel(PdfConversionService pdfConversionService)
    {
        _pdfConversionService = pdfConversionService;
    }


    [RelayCommand]
    public void AddFloor()
    {
        Floors.Add(new Floor { FloorName = $"Stockwerk {Floors.Count + 1}" });
    }

    [RelayCommand]
    public void RemoveFloor(Floor floor)
    {
        if (floor == null) return;

        Debug.WriteLine($"[BuildingEditorViewModel] Removing floor: {floor.FloorName}");

        // Delete PDF file if it exists
        DeletePdfFile(floor);

        // Remove floor from collection
        if (Floors.Contains(floor))
            Floors.Remove(floor);
    }

    [RelayCommand]
    public async Task UploadPdfAsync(Floor floor)
    {
        try
        {
            if (floor == null) return;
            // Pick PDF file
            var options = new PickOptions
            {
                PickerTitle = "Select a PDF file",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/pdf" } },
                    { DevicePlatform.iOS, new[] { "com.adobe.pdf" } },
                    { DevicePlatform.WinUI, new[] { ".pdf" } },
                    { DevicePlatform.MacCatalyst, new[] { "pdf" } }
                })
            };

            var result = await FilePicker.PickAsync(options);
            if (result == null) return;

            // Create a copy in app storage
            var pdfDirectory = Path.Combine(FileSystem.AppDataDirectory, "PDFs");
            if (!Directory.Exists(pdfDirectory))
                Directory.CreateDirectory(pdfDirectory);

            var destinationPath = Path.Combine(pdfDirectory, $"{Guid.NewGuid()}.pdf");
            using (var sourceStream = await result.OpenReadAsync())
            using (var destinationStream = File.Create(destinationPath))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }

            // Update floor model with PDF path
            floor.PdfPath = destinationPath;

            // Convert PDF to PNG
            await _pdfConversionService.ConvertFloorPdfToPngAsync(floor);

            // Update UI or save floor to database as needed
            OnPropertyChanged(nameof(Floors));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to upload PDF: {ex.Message}", "OK");
        }
    }
    

    [RelayCommand]
    public async Task DeletePdf(Floor floor)
    {
        if (floor == null) return;

        Debug.WriteLine($"[BuildingEditorViewModel] Deleting PDF for floor: {floor.FloorName}");

        if (string.IsNullOrEmpty(floor.PdfPath))
            return;

        bool confirmed = await Application.Current.MainPage.DisplayAlert(
            "PDF löschen",
            "Sind Sie sicher, dass Sie den PDF-Bauplan löschen möchten?",
            "Ja", "Nein");

        if (confirmed)
        {
            DeletePdfFile(floor);
            floor.PdfPath = null;
            OnPropertyChanged(nameof(Floors));
        }
    }

    // Helper method to safely delete a PDF file
    private void DeletePdfFile(Floor floor)
    {
        if (floor == null || string.IsNullOrEmpty(floor.PdfPath))
            return;

        try
        {
            if (File.Exists(floor.PdfPath))
            {
                File.Delete(floor.PdfPath);
                Debug.WriteLine($"[BuildingEditorViewModel] Deleted PDF file: {floor.PdfPath}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BuildingEditorViewModel] Error deleting PDF file: {ex.Message}");
        }
    }
}
