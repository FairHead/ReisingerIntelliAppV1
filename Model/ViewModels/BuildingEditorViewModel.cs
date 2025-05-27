using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Linq;
using ReisingerIntelliAppV1.Model.Models;
using System.Diagnostics;

namespace ReisingerIntelliAppV1.Model.ViewModels;

public partial class BuildingEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private string buildingName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Floor> floors = new();

    [ObservableProperty]
    private string pageTitle = "Gebäude hinzufügen";

    [RelayCommand]
    public void AddFloor()
    {
        Floors.Add(new Floor { Name = $"Stockwerk {Floors.Count + 1}" });
    }

    [RelayCommand]
    public void RemoveFloor(Floor floor)
    {
        if (floor == null) return;

        Debug.WriteLine($"[BuildingEditorViewModel] Removing floor: {floor.Name}");

        // Delete PDF file if it exists
        DeletePdfFile(floor);

        // Remove floor from collection
        if (Floors.Contains(floor))
            Floors.Remove(floor);
    }

    [RelayCommand]
    public async Task UploadPdfAsync(Floor floor)
    {
        if (floor == null) return;

        Debug.WriteLine($"[BuildingEditorViewModel] Uploading PDF for floor: {floor.Name}");

        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "PDF auswählen",
            FileTypes = FilePickerFileType.Pdf
        });

        if (result != null)
        {
            var fileName = Path.GetFileName(result.FullPath);
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            try
            {
                // Delete old PDF file if it exists
                if (!string.IsNullOrEmpty(floor.PdfPath) && File.Exists(floor.PdfPath))
                {
                    File.Delete(floor.PdfPath);
                    Debug.WriteLine($"[BuildingEditorViewModel] Deleted old PDF: {floor.PdfPath}");
                }

                // Copy new PDF file
                using var sourceStream = await result.OpenReadAsync();
                using var targetStream = File.Create(targetPath);
                await sourceStream.CopyToAsync(targetStream);

                floor.PdfPath = targetPath;
                OnPropertyChanged(nameof(Floors));

                Debug.WriteLine($"[BuildingEditorViewModel] PDF uploaded: {targetPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BuildingEditorViewModel] Error uploading PDF: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Fehler",
                    $"PDF konnte nicht hochgeladen werden: {ex.Message}",
                    "OK");
            }
        }
    }

    [RelayCommand]
    public async Task DeletePdf(Floor floor)
    {
        if (floor == null) return;

        Debug.WriteLine($"[BuildingEditorViewModel] Deleting PDF for floor: {floor.Name}");

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
