using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Linq;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;

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

    [RelayCommand]
    public void ToggleBuildingDropdown()
    {
        IsBuildingDropdownVisible = !IsBuildingDropdownVisible;
        if (IsBuildingDropdownVisible)
        {
            IsFloorDropdownVisible = false;
            IsSavedDevicesDropdownVisible = false;
            IsLocalDevicesDropdownVisible = false;
        }
    }

    [RelayCommand]
    public void ToggleFloorDropdown()
    {
        IsFloorDropdownVisible = !IsFloorDropdownVisible;
        if (IsFloorDropdownVisible)
        {
            IsBuildingDropdownVisible = false;
            IsSavedDevicesDropdownVisible = false;
            IsLocalDevicesDropdownVisible = false;
        }
    }

    [RelayCommand]
    public void ToggleSavedDevicesDropdown()
    {
        IsSavedDevicesDropdownVisible = !IsSavedDevicesDropdownVisible;
        if (IsSavedDevicesDropdownVisible)
        {
            IsBuildingDropdownVisible = false;
            IsFloorDropdownVisible = false;
            IsLocalDevicesDropdownVisible = false;
        }
    }

    [RelayCommand]
    public void ToggleLocalDevicesDropdown()
    {
        IsLocalDevicesDropdownVisible = !IsLocalDevicesDropdownVisible;
        if (IsLocalDevicesDropdownVisible)
        {
            IsBuildingDropdownVisible = false;
            IsFloorDropdownVisible = false;
            IsSavedDevicesDropdownVisible = false;
        }
    }

    public FloorPlanViewModel(DeviceService deviceService)
    {
        _deviceService = deviceService;
      
    }
    partial void OnSelectedBuildingChanged(Building? value)
    {
        UpdateFloors();
        SelectedFloor = null;
    }

    private void UpdateFloors()
    {
        Floors.Clear();

        var floors = SelectedBuilding?.Floors;
        if (floors == null)
            return;

        foreach (var floor in floors)
            Floors.Add(floor);
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
    }

    public void AddBuilding(Building building)
    {
        Buildings.Add(building);
        SelectedBuilding = building;
    }

    [RelayCommand]
    public async Task UploadPdfAsync()
    {
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

            // Dummy-Zuweisung an Stockwerk:
            FloorPlans.Add(new FloorPlan
            {
                FloorName = $"Stockwerk {FloorPlans.Count + 1}",
                PdfPath = targetPath
            });

            SelectedFloorPlan = FloorPlans.Last();
        }
    }

    public async Task LoadDevicesAsync()
    {
        var all = (await _deviceService.LoadDeviceList()).Devices;

        SavedDevices.Clear();
        LocalDevices.Clear();

        foreach (var device in all.Where(d => d.ConnectionType == ConnectionType.Wifi))
            SavedDevices.Add(device);

        foreach (var device in all.Where(d => d.ConnectionType == ConnectionType.Local))
            LocalDevices.Add(device);
    }

}