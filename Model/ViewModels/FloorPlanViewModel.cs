using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Linq;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;
using System.Diagnostics;

namespace ReisingerIntelliAppV1.Model.ViewModels;
public partial class FloorPlanViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Building> buildings = new();

    [ObservableProperty]
    private Building? selectedBuilding;

    [ObservableProperty]
    private Floor? selectedFloor;

    [ObservableProperty]
    private FloorPlan? selectedFloorPlan;

    [ObservableProperty]
    private ObservableCollection<Floor> floors = new();
    private readonly DeviceService _deviceService;

    public ObservableCollection<object> DisplayItems { get; } = new();
    public ObservableCollection<DeviceModel> SavedDevices { get; set; } = new();
    public ObservableCollection<DeviceModel> LocalDevices { get; set; } = new();

    public ObservableCollection<FloorPlan> FloorPlans { get; } = new();

    [ObservableProperty]
    private bool isBuildingDropdownVisible = false;

    [ObservableProperty]
    private bool isFloorDropdownVisible = false;

    [ObservableProperty]
    private bool isSavedDevicesDropdownVisible = false;

    [ObservableProperty]
    private bool isLocalDevicesDropdownVisible = false;

    // Flag to track if any dropdown is currently open
    private bool _isAnyDropdownOpen = false;

    [RelayCommand]
    public void ToggleBuildingDropdown()
    {
        Debug.WriteLine($"[FloorPlanViewModel] ToggleBuildingDropdown called. Current state: {IsBuildingDropdownVisible}, Any dropdown open: {_isAnyDropdownOpen}");

        // If this dropdown is already open, close it
        if (IsBuildingDropdownVisible)
        {
            IsBuildingDropdownVisible = false;
            _isAnyDropdownOpen = false;
            Debug.WriteLine("[FloorPlanViewModel] Building dropdown was open, now closed");
            return;
        }

        // Close all dropdowns first
        CloseAllDropdowns();

        // Then open this dropdown
        IsBuildingDropdownVisible = true;
        _isAnyDropdownOpen = true;

        Debug.WriteLine("[FloorPlanViewModel] Building dropdown is now open");
    }

    [RelayCommand]
    public void ToggleFloorDropdown()
    {
        Debug.WriteLine($"[FloorPlanViewModel] ToggleFloorDropdown called. Current state: {IsFloorDropdownVisible}, Any dropdown open: {_isAnyDropdownOpen}");

        // If this dropdown is already open, close it
        if (IsFloorDropdownVisible)
        {
            IsFloorDropdownVisible = false;
            _isAnyDropdownOpen = false;
            Debug.WriteLine("[FloorPlanViewModel] Floor dropdown was open, now closed");
            return;
        }

        // Close all dropdowns first
        CloseAllDropdowns();

        // Then open this dropdown
        IsFloorDropdownVisible = true;
        _isAnyDropdownOpen = true;

        Debug.WriteLine("[FloorPlanViewModel] Floor dropdown is now open");
    }

    [RelayCommand]
    public void ToggleSavedDevicesDropdown()
    {
        Debug.WriteLine($"[FloorPlanViewModel] ToggleSavedDevicesDropdown called. Current state: {IsSavedDevicesDropdownVisible}, Any dropdown open: {_isAnyDropdownOpen}");

        // If this dropdown is already open, close it
        if (IsSavedDevicesDropdownVisible)
        {
            IsSavedDevicesDropdownVisible = false;
            _isAnyDropdownOpen = false;
            Debug.WriteLine("[FloorPlanViewModel] SavedDevices dropdown was open, now closed");
            return;
        }

        // Close all dropdowns first
        CloseAllDropdowns();

        // Then open this dropdown
        IsSavedDevicesDropdownVisible = true;
        _isAnyDropdownOpen = true;

        Debug.WriteLine("[FloorPlanViewModel] SavedDevices dropdown is now open");
    }

    [RelayCommand]
    public void ToggleLocalDevicesDropdown()
    {
        Debug.WriteLine($"[FloorPlanViewModel] ToggleLocalDevicesDropdown called. Current state: {IsLocalDevicesDropdownVisible}, Any dropdown open: {_isAnyDropdownOpen}");

        // If this dropdown is already open, close it
        if (IsLocalDevicesDropdownVisible)
        {
            IsLocalDevicesDropdownVisible = false;
            _isAnyDropdownOpen = false;
            Debug.WriteLine("[FloorPlanViewModel] LocalDevices dropdown was open, now closed");
            return;
        }

        // Close all dropdowns first
        CloseAllDropdowns();

        // Then open this dropdown
        IsLocalDevicesDropdownVisible = true;
        _isAnyDropdownOpen = true;

        Debug.WriteLine("[FloorPlanViewModel] LocalDevices dropdown is now open");
    }

    // Helper method to close all dropdowns
    private void CloseAllDropdowns()
    {
        Debug.WriteLine("[FloorPlanViewModel] CloseAllDropdowns called");
        IsBuildingDropdownVisible = false;
        IsFloorDropdownVisible = false;
        IsSavedDevicesDropdownVisible = false;
        IsLocalDevicesDropdownVisible = false;
    }

    public FloorPlanViewModel(DeviceService deviceService)
    {
        _deviceService = deviceService;
        Debug.WriteLine("[FloorPlanViewModel] Constructor called");
    }

    partial void OnSelectedBuildingChanged(Building? value)
    {
        UpdateFloors();
        SelectedFloor = null;
        Debug.WriteLine($"[FloorPlanViewModel] OnSelectedBuildingChanged: {value?.Name ?? "null"}");
    }

    private void UpdateFloors()
    {
        Floors.Clear();

        var floors = SelectedBuilding?.Floors;
        if (floors == null)
            return;

        foreach (var floor in floors)
            Floors.Add(floor);

        Debug.WriteLine($"[FloorPlanViewModel] UpdateFloors: Added {Floors.Count} floors");
    }

    private void RebuildDisplayList()
    {
        DisplayItems.Clear();

        foreach (var building in Buildings)
        {
            var model = new BuildingDisplayModel { Building = building };
            DisplayItems.Add(model);

            if (model.IsExpanded)
            {
                foreach (var floor in building.Floors)
                    DisplayItems.Add(floor);
            }
        }

        Debug.WriteLine($"[FloorPlanViewModel] RebuildDisplayList: {DisplayItems.Count} items");
    }

    public void AddBuilding(Building building)
    {
        Buildings.Add(building);
        SelectedBuilding = building;
        Debug.WriteLine($"[FloorPlanViewModel] AddBuilding: {building.Name}, now {Buildings.Count} buildings");
    }

    // Method to notify that a building was changed externally (from editor page)
    public void NotifyBuildingChanged(Building building)
    {
        Debug.WriteLine($"[FloorPlanViewModel] NotifyBuildingChanged: {building.Name}");

        // Trigger UI refresh
        OnPropertyChanged(nameof(Buildings));

        // Update selection to refresh related UI
        if (SelectedBuilding == building)
        {
            var temp = SelectedBuilding;
            SelectedBuilding = null;
            SelectedBuilding = temp;
        }
    }

    [RelayCommand]
    public async Task UploadPdfAsync()
    {
        Debug.WriteLine("[FloorPlanViewModel] UploadPdfAsync called");
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "PDF auswaehlen",
            FileTypes = FilePickerFileType.Pdf
        });

        if (result != null)
        {
            var fileName = Path.GetFileName(result.FullPath);
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using var sourceStream = await result.OpenReadAsync();
            using var targetStream = File.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream);

            // Update the selected floor's PDF path
            if (SelectedFloor != null)
            {
                SelectedFloor.PdfPath = targetPath;
                Debug.WriteLine($"[FloorPlanViewModel] PDF path updated for floor {SelectedFloor.Name}: {targetPath}");

                // Force a refresh of SelectedFloor to trigger UI updates
                var currentFloor = SelectedFloor;
                SelectedFloor = null;
                SelectedFloor = currentFloor;
            }

            // Also keep the FloorPlans collection updated (for backward compatibility)
            FloorPlans.Add(new FloorPlan
            {
                FloorName = SelectedFloor?.Name ?? $"Stockwerk {FloorPlans.Count + 1}",
                PdfPath = targetPath
            });

            SelectedFloorPlan = FloorPlans.Last();
            Debug.WriteLine($"[FloorPlanViewModel] PDF uploaded: {fileName}");
        }
    }

    public async Task LoadDevicesAsync()
    {
        Debug.WriteLine("[FloorPlanViewModel] LoadDevicesAsync called");
        var all = (await _deviceService.LoadDeviceList()).Devices;

        SavedDevices.Clear();
        LocalDevices.Clear();

        foreach (var device in all.Where(d => d.ConnectionType == ConnectionType.Wifi))
            SavedDevices.Add(device);

        foreach (var device in all.Where(d => d.ConnectionType == ConnectionType.Local))
            LocalDevices.Add(device);

        Debug.WriteLine($"[FloorPlanViewModel] Loaded devices: {SavedDevices.Count} saved, {LocalDevices.Count} local");
    }

    // New methods for handling building and floor context actions

    [RelayCommand]
    public async Task EditBuildingAsync(Building building)
    {
        if (building == null) return;

        Debug.WriteLine($"[FloorPlanViewModel] EditBuildingAsync called for building: {building.Name}");

        try
        {
            // Create editor page for the building
            var editorPage = new Views.FloorManager.BuildingEditorPage(this, building);
            await Application.Current.MainPage.Navigation.PushAsync(editorPage);

            Debug.WriteLine("[FloorPlanViewModel] Navigated to BuildingEditorPage");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FloorPlanViewModel] Error navigating to BuildingEditorPage: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Fehler", $"Fehler beim Öffnen der Bearbeitungsseite: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task DeleteBuildingAsync(Building building)
    {
        if (building == null) return;

        Debug.WriteLine($"[FloorPlanViewModel] DeleteBuildingAsync called for building: {building.Name}");

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Gebäude löschen",
            $"Sind Sie sicher, dass Sie das Gebäude '{building.Name}' mit allen Stockwerken löschen möchten?",
            "Ja", "Nein");

        if (confirm)
        {
            // Delete all associated PDFs first
            foreach (var floor in building.Floors)
            {
                DeleteFloorPdfFile(floor);
            }

            // If this is the selected building, clear the selection
            if (SelectedBuilding == building)
            {
                SelectedBuilding = null;
                SelectedFloor = null;
            }

            // Remove the building from the collection
            Buildings.Remove(building);
            Debug.WriteLine($"[FloorPlanViewModel] Building deleted: {building.Name}");
        }
    }

    [RelayCommand]
    public async Task EditFloorAsync(Floor floor)
    {
        if (floor == null || SelectedBuilding == null) return;

        Debug.WriteLine($"[FloorPlanViewModel] EditFloorAsync called for floor: {floor.Name}");

        try
        {
            // Open the building editor with the current building
            var editorPage = new Views.FloorManager.BuildingEditorPage(this, SelectedBuilding);
            await Application.Current.MainPage.Navigation.PushAsync(editorPage);

            Debug.WriteLine("[FloorPlanViewModel] Navigated to BuildingEditorPage for floor editing");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FloorPlanViewModel] Error navigating to BuildingEditorPage: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Fehler", $"Fehler beim Öffnen der Bearbeitungsseite: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task DeleteFloorAsync(Floor floor)
    {
        if (floor == null || SelectedBuilding == null) return;

        Debug.WriteLine($"[FloorPlanViewModel] DeleteFloorAsync called for floor: {floor.Name}");

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Stockwerk löschen",
            $"Sind Sie sicher, dass Sie das Stockwerk '{floor.Name}' löschen möchten?",
            "Ja", "Nein");

        if (confirm)
        {
            // Delete associated PDF file
            DeleteFloorPdfFile(floor);

            // If this is the selected floor, clear the selection
            if (SelectedFloor == floor)
            {
                SelectedFloor = null;
            }

            // Remove the floor from the building
            SelectedBuilding.Floors.Remove(floor);

            // Also update the Floors collection
            Floors.Remove(floor);

            Debug.WriteLine($"[FloorPlanViewModel] Floor deleted: {floor.Name}");
        }
    }

    // Helper method to safely delete a floor's PDF file
    private void DeleteFloorPdfFile(Floor floor)
    {
        if (string.IsNullOrEmpty(floor.PdfPath)) return;

        try
        {
            if (File.Exists(floor.PdfPath))
            {
                File.Delete(floor.PdfPath);
                Debug.WriteLine($"[FloorPlanViewModel] PDF file deleted: {floor.PdfPath}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FloorPlanViewModel] Error deleting PDF file: {ex.Message}");
        }
    }
}