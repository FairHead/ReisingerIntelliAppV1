using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;
using ReisingerIntelliAppV1.Views.DeviceControlViews;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows.Input;

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
    private DeviceModel? selectedDevice;

    [ObservableProperty]
    private PlacedDeviceModel? selectedPlacedDevice;

    private readonly DeviceService _deviceService;
    private readonly WifiService _wifiService;
    private readonly BuildingStorageService _buildingStorageService;
    private System.Timers.Timer? _statusUpdateTimer;
    private readonly IntellidriveApiService _apiService;

    public ObservableCollection<object> DisplayItems { get; } = new();
    public ObservableCollection<DeviceModel> SavedDevices { get; set; } = new();
    public ObservableCollection<DeviceModel> LocalDevices { get; set; } = new();

    public ObservableCollection<Floor> Floors { get; } = new();

    [ObservableProperty]
    private bool isBuildingDropdownVisible = false;

    [ObservableProperty]
    private bool isFloorDropdownVisible = false;

    [ObservableProperty]
    private bool isSavedDevicesDropdownVisible = false;

    [ObservableProperty]
    private bool isLocalDevicesDropdownVisible = false;

    public ICommand AddDeviceToCenterCommand { get; }

    // Property für die Anzeige des Bauplans (PNG) des ausgewählten Stockwerks
    public string? FloorPlanImageSource => SelectedFloor?.PngPath;

    // Property für die Sichtbarkeit, ob ein Bauplan vorhanden ist
    public bool IsFloorPlanAvailable => !string.IsNullOrEmpty(SelectedFloor?.PngPath);
    public bool IsNoFloorPlanAvailable => string.IsNullOrEmpty(SelectedFloor?.PngPath);


    // Flag to track if any dropdown is currently open
    private bool _isAnyDropdownOpen = false;


    public FloorPlanViewModel(DeviceService deviceService, WifiService wifiService, BuildingStorageService buildingStorageService, IntellidriveApiService apiService)
    {
        _deviceService = deviceService;
        _wifiService = wifiService;
        _buildingStorageService = buildingStorageService;
        Debug.WriteLine("[FloorPlanViewModel] Constructor called");

        // Lade die Gebäude beim Start asynchron
        _ = LoadBuildingsAsync();
        _apiService = apiService;
        AddDeviceToCenterCommand = new Command<DeviceModel>(AddDeviceToCenter);
    }

    // Füge einen RelayCommand für RemovePlacedDevice hinzu
    [RelayCommand]
    private void RemovePlacedDevice(PlacedDeviceModel device)
    {
        if (SelectedFloor?.PlacedDevices != null && device != null)
        {
            Debug.WriteLine($"[FloorPlanViewModel] Removing placed device: {device.Name}");
            SelectedFloor.PlacedDevices.Remove(device);
            _ = SaveBuildingsAsync();
        }
    }

    private async void AddDeviceToCenter(DeviceModel device)
    {
        if (SelectedFloor == null || device == null)
        {
            Debug.WriteLine("[FloorPlanViewModel] AddDeviceToCenter: SelectedFloor or device is null");
            return;
        }

        Debug.WriteLine($"[FloorPlanViewModel] AddDeviceToCenter: Adding device {device.Name} to center of floor plan");

        // Erstelle das PlacedDeviceModel mit anfänglich unsichtbarem Status
        var placed = new PlacedDeviceModel
        {
            DeviceInfo = device,
            Name = device.Name,
            IsOnline = device.IsOnline,
            DeviceId = device.DeviceId,
            RelativeX = 0.5,
            RelativeY = 0.5,
            Scale = 0.1,
            // Wichtig: Füge eine Visibility-Property hinzu
            IsVisible = false
        };

        if (SelectedFloor.PlacedDevices == null)
            SelectedFloor.PlacedDevices = new ObservableCollection<PlacedDeviceModel>();

        // Zuerst zum SelectedFloor hinzufügen, aber noch unsichtbar
        SelectedFloor.PlacedDevices.Add(placed);

        // Frage den Türstatus ab, bevor das Element sichtbar gemacht wird
        if (device.IsOnline)
        {
            try
            {
                bool connected = await _wifiService.EnsureConnectedToSsidAsync(device.Ssid);
                if (connected)
                {
                    var doorStateJson = await _apiService.GetDoorStateAsync(device);
                    var doorState = JsonSerializer.Deserialize<DoorStateResponse>(doorStateJson);

                    bool isClosed = doorState?.Content?.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true;
                    placed.IsDoorOpen = !isClosed;

                    Debug.WriteLine($"[FloorPlanViewModel] Door state retrieved: IsDoorOpen={placed.IsDoorOpen}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FloorPlanViewModel] Error retrieving door state: {ex.Message}");
            }
        }

        // Jetzt mache das Element sichtbar
        placed.IsVisible = true;

        // Benachrichtige die UI, dass sich die Collection geändert hat
        OnPropertyChanged(nameof(SelectedFloor));

        // Speichere die Änderungen
        _ = SaveBuildingsAsync();
    }

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



    partial void OnSelectedBuildingChanged(Building? value)
    {
        UpdateFloors();
        SelectedFloor = null;
        Debug.WriteLine($"[FloorPlanViewModel] OnSelectedBuildingChanged: {value?.BuildingName ?? "null"}");
    }

    partial void OnSelectedFloorChanged(Floor? value)
    {
        OnPropertyChanged(nameof(FloorPlanImageSource));
        OnPropertyChanged(nameof(IsFloorPlanAvailable));
        OnPropertyChanged(nameof(IsNoFloorPlanAvailable));
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
        Debug.WriteLine($"[FloorPlanViewModel] AddBuilding: {building.BuildingName}, now {Buildings.Count} buildings");

        // Speichere die Änderungen
        SaveBuildingsAsync().ConfigureAwait(false);
    }

    // Method to notify that a building was changed externally (from editor page)
    public void NotifyBuildingChanged(Building building)
    {
        Debug.WriteLine($"[FloorPlanViewModel] NotifyBuildingChanged: {building.BuildingName}");

        // Trigger UI refresh
        OnPropertyChanged(nameof(Buildings));

        // Update selection to refresh related UI
        if (SelectedBuilding == building)
        {
            var temp = SelectedBuilding;
            SelectedBuilding = null;
            SelectedBuilding = temp;
        }

        // Speichere die Änderungen
        SaveBuildingsAsync().ConfigureAwait(false);
    }

    // Speichere alle Gebäude und Stockwerke in den SecureStorage
    public async Task SaveBuildingsAsync()
    {
        Debug.WriteLine("[FloorPlanViewModel] SaveBuildingsAsync called");
        try
        {
            await _buildingStorageService.SaveBuildingsAsync(Buildings);
            Debug.WriteLine($"[FloorPlanViewModel] {Buildings.Count} Gebäude gespeichert");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FloorPlanViewModel] Fehler beim Speichern der Gebäude: {ex.Message}");
        }
    }

    // Lade alle Gebäude und Stockwerke aus dem SecureStorage
    public async Task LoadBuildingsAsync()
    {
        Debug.WriteLine("[FloorPlanViewModel] LoadBuildingsAsync called");
        try
        {
            var loadedBuildings = await _buildingStorageService.LoadBuildingsAsync();

            Buildings.Clear();
            foreach (var building in loadedBuildings)
            {
                Buildings.Add(building);
            }

            Debug.WriteLine($"[FloorPlanViewModel] {Buildings.Count} Gebäude geladen");

            // Aktualisiere die UI entsprechend
            if (Buildings.Count > 0)
            {
                SelectedBuilding = Buildings.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FloorPlanViewModel] Fehler beim Laden der Gebäude: {ex.Message}");
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

        // Update the online status immediately after loading
        await UpdateDevicesOnlineStatusAsync();
    }
// Start online status checker
public async Task StartOnlineStatusUpdater()
{
    Debug.WriteLine("[FloorPlanViewModel] StartOnlineStatusUpdater");
    
    // Always immediately update on start
    await MainThread.InvokeOnMainThreadAsync(UpdateDevicesOnlineStatusAsync);

    if (_statusUpdateTimer != null) return;

    _statusUpdateTimer = new System.Timers.Timer(2000);
    _statusUpdateTimer.Elapsed += async (_, _) => await MainThread.InvokeOnMainThreadAsync(UpdateDevicesOnlineStatusAsync);
    _statusUpdateTimer.AutoReset = true;
    _statusUpdateTimer.Start();
}
    // Stop online status checker
    public void StopOnlineStatusUpdater()
    {
        Debug.WriteLine("[FloorPlanViewModel] StopOnlineStatusUpdater");
        _statusUpdateTimer?.Stop();
        _statusUpdateTimer?.Dispose();
        _statusUpdateTimer = null;
    }

    // Update online status of all devices
    private async Task UpdateDevicesOnlineStatusAsync()
    {
        Debug.WriteLine("[FloorPlanViewModel] UpdateDevicesOnlineStatusAsync");

        // Update saved devices status
        if (SavedDevices.Count > 0)
        {
            var updatedSaved = await _wifiService.CheckDeviceNetworkStatusAsync(SavedDevices.ToList());
            for (int i = 0; i < updatedSaved.Count && i < SavedDevices.Count; i++)
            {
                SavedDevices[i].IsOnline = updatedSaved[i].IsOnline;
            }
        }

        // Update local devices status
        if (LocalDevices.Count > 0)
        {
            var updatedLocal = await _wifiService.CheckDeviceNetworkStatusAsync(LocalDevices.ToList());
            for (int i = 0; i < updatedLocal.Count && i < LocalDevices.Count; i++)
            {
                LocalDevices[i].IsOnline = updatedLocal[i].IsOnline;
            }
        }

        // Aktualisiere auch den Status der platzierten Geräte
        if (SelectedFloor?.PlacedDevices != null)
        {
            foreach (var placedDevice in SelectedFloor.PlacedDevices)
            {
                // Guard against null DeviceInfo to prevent crashes
                if (placedDevice.DeviceInfo == null)
                    continue;

                // Finde das entsprechende Gerät in den Listen und kopiere den Status
                var savedDevice = SavedDevices.FirstOrDefault(d => d.DeviceId == placedDevice.DeviceId);
                if (savedDevice != null)
                {
                    placedDevice.IsOnline = savedDevice.IsOnline;
                    // Update the nested DeviceInfo if it exists
                    if (placedDevice.DeviceInfo != null)
                        placedDevice.DeviceInfo.IsOnline = savedDevice.IsOnline;
                }
                else
                {
                    var localDevice = LocalDevices.FirstOrDefault(d => d.DeviceId == placedDevice.DeviceId);
                    if (localDevice != null)
                    {
                        placedDevice.IsOnline = localDevice.IsOnline;
                        // Update the nested DeviceInfo if it exists
                        if (placedDevice.DeviceInfo != null)
                            placedDevice.DeviceInfo.IsOnline = localDevice.IsOnline;
                    }
                }
            }
        }
    }

    // Öffne/Schließe die Tür eines Geräts
    [RelayCommand]
    public async Task ToggleDoorAsync(PlacedDeviceModel device)
    {
        if (device?.DeviceInfo == null)
            return;

        try
        {
            Debug.WriteLine($"[PlacedDeviceControl] Attempting to toggle door for device: {device.DeviceInfo.Name}");

            // Ensure we're connected to the device's WiFi
            var connected = await _wifiService.EnsureConnectedToSsidAsync(device.DeviceInfo.Ssid);
            if (!connected)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Verbindungsproblem",
                    $"Die Verbindung zum Gerät '{device.DeviceInfo.Name}' konnte nicht hergestellt werden.",
                    "OK");
                return;
            }

            // Check current door state
            var initialJson = await _apiService.GetDoorStateAsync(device.DeviceInfo);
            var initialResult = JsonSerializer.Deserialize<DoorStateResponse>(initialJson);
            bool wasClosed = initialResult?.Content?.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true;

            // Toggle the door state - the API automatically opens if closed, closes if open
            await _apiService.OpenDoorAsync(device.DeviceInfo);

            // Wait and check if the door state changed
            const int maxAttempts = 10;
            for (int i = 0; i < maxAttempts; i++)
            {
                await Task.Delay(1000);
                var status = await _apiService.GetDoorStateAsync(device.DeviceInfo);
                var newState = JsonSerializer.Deserialize<DoorStateResponse>(status);

                bool isClosedNow = newState?.Content?.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true;
                if (isClosedNow != wasClosed)
                {
                    // Update the UI state
                    device.IsDoorOpen = !isClosedNow;
                    await Application.Current.MainPage.DisplayAlert(
                        "Status",
                        isClosedNow ? "Tür geschlossen." : "Tür geöffnet.",
                        "OK");
                    return;
                }
            }

            await Application.Current.MainPage.DisplayAlert(
                "Hinweis",
                "Türstatus hat sich nicht geändert.",
                "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PlacedDeviceControl] Error toggling door: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Fehler",
                $"Ein Fehler ist aufgetreten: {ex.Message}",
                "OK");
        }
    }

    // Öffne die Gerätesettings eines platzierten Geräts
    [RelayCommand]
    public async Task OpenDeviceSettingsAsync(PlacedDeviceModel device)
    {
        if (device == null || device.DeviceInfo == null)
            return;

        Debug.WriteLine($"[FloorPlanViewModel] Opening settings for device: {device.DeviceInfo.Name}");

        try
        {
            // Verbindung zum Gerät herstellen
            var connected = await _wifiService.EnsureConnectedToSsidAsync(device.DeviceInfo.Ssid);
            if (!connected)
            {
                // Benachrichtigung, dass keine Verbindung hergestellt werden konnte
                await Application.Current.MainPage.DisplayAlert(
                    "Verbindungsproblem",
                    $"Die Verbindung zum Gerät '{device.DeviceInfo.Name}' konnte nicht hergestellt werden.",
                    "OK");
                return;
            }

            // Navigiere zur DeviceSettingsTabbedPage
            var settingsPage = App.Current.Handler.MauiContext.Services
                .GetRequiredService<DeviceSettingsTabbedPage>();

            await settingsPage.InitializeWithAsync(device.DeviceInfo);
            await Application.Current.MainPage.Navigation.PushAsync(settingsPage);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FloorPlanViewModel] Error opening device settings: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Fehler",
                $"Die Geräteeinstellungen konnten nicht geöffnet werden: {ex.Message}",
                "OK");
        }
    }

    // Update position of a placed device
    [RelayCommand]
    public void UpdateDevicePosition(object positionData)
    {
        try
        {
            // Use reflection to access anonymous object properties
            var type = positionData.GetType();
            var deviceProperty = type.GetProperty("DeviceModel");
            var xProperty = type.GetProperty("RelativeX");
            var yProperty = type.GetProperty("RelativeY");

            if (deviceProperty != null && xProperty != null && yProperty != null)
            {
                var device = deviceProperty.GetValue(positionData) as PlacedDeviceModel;
                var newX = (double)xProperty.GetValue(positionData);
                var newY = (double)yProperty.GetValue(positionData);

                if (device != null)
                {
                    Debug.WriteLine($"[FloorPlanViewModel] Updating device position: {device.Name} to X={newX:F3}, Y={newY:F3}");
                    
                    device.RelativeX = newX;
                    device.RelativeY = newY;

                    // Save changes
                    _ = SaveBuildingsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FloorPlanViewModel] Error updating device position: {ex.Message}");
        }
    }

    // New methods for handling building and floor context actions

    [RelayCommand]
    public async Task EditBuildingAsync(Building building)
    {
        if (building == null) return;

        Debug.WriteLine($"[FloorPlanViewModel] EditBuildingAsync called for building: {building.BuildingName}");

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

        Debug.WriteLine($"[FloorPlanViewModel] DeleteBuildingAsync called for building: {building.BuildingName}");

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Gebäude löschen",
            $"Sind Sie sicher, dass Sie das Gebäude '{building.BuildingName}' mit allen Stockwerken löschen möchten?",
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
            Debug.WriteLine($"[FloorPlanViewModel] Building deleted: {building.BuildingName}");

            // Speichere die Änderungen
            await SaveBuildingsAsync();
        }
    }

    [RelayCommand]
    public async Task EditFloorAsync(Floor floor)
    {
        if (floor == null || SelectedBuilding == null) return;

        Debug.WriteLine($"[FloorPlanViewModel] EditFloorAsync called for floor: {floor.FloorName}");

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

        Debug.WriteLine($"[FloorPlanViewModel] DeleteFloorAsync called for floor: {floor.FloorName}");

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Stockwerk löschen",
            $"Sind Sie sicher, dass Sie das Stockwerk '{floor.FloorName}' löschen möchten?",
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

            Debug.WriteLine($"[FloorPlanViewModel] Floor deleted: {floor.FloorName}");

            // Speichere die Änderungen
            await SaveBuildingsAsync();
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

    private void SaveFloorChanges()
    {
        try
        {
            // Save the changes to persistent storage
            SaveBuildingsAsync().ConfigureAwait(false);
            Debug.WriteLine("[FloorPlanViewModel] Floor changes saved");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FloorPlanViewModel] Error saving floor changes: {ex.Message}");
        }
    }

}