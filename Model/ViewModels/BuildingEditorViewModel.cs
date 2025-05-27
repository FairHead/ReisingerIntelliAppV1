using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Linq;
using ReisingerIntelliAppV1.Model.Models;

namespace ReisingerIntelliAppV1.Model.ViewModels;

public partial class BuildingEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private string buildingName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Floor> floors = new();

    [RelayCommand]
    public void AddFloor()
    {
        Floors.Add(new Floor { Name = $"Stockwerk {Floors.Count + 1}" });
    }

    [RelayCommand]
    public void RemoveFloor(Floor floor)
    {
        if (Floors.Contains(floor))
            Floors.Remove(floor);
    }

    [RelayCommand]
    public async Task UploadPdfAsync(Floor floor)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "PDF auswählen",
            FileTypes = FilePickerFileType.Pdf
        });

        if (result != null && floor != null)
        {
            var fileName = Path.GetFileName(result.FullPath);
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using var sourceStream = await result.OpenReadAsync();
            using var targetStream = File.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream);

            floor.PdfPath = targetPath;
            OnPropertyChanged(nameof(Floors));
        }
    }
}
