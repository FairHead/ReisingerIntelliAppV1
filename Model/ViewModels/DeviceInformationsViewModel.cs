using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;

namespace ReisingerIntelliAppV1.Model.ViewModels;

public partial class DeviceInformationsViewModel : BaseViewModel
{
    private readonly DeviceService _deviceService;
    private readonly IntellidriveApiService _apiService;

    [ObservableProperty]
    private DeviceModel selectedDevice;

    [ObservableProperty]
    private bool isLoading;

    public DeviceInformationsViewModel(DeviceService deviceService, IntellidriveApiService apiService)
    {
        _deviceService = deviceService;
        _apiService = apiService;
    }

    public async Task InitializeAsync(DeviceModel device)
    {
        SelectedDevice = device;

        try
        {
            IsLoading = true;
            // Beispiel: ggf. zusätzliche Infos laden
            var parameters = await _apiService.GetParametersAsync(device);
            SelectedDevice.Parameters = parameters;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> SaveDeviceNameAsync()
    {
        if (SelectedDevice == null || string.IsNullOrWhiteSpace(SelectedDevice.Name))
        {
            return false;
        }

        try
        {
            IsBusy = true;
            Debug.WriteLine($"Updating device name to: {SelectedDevice.Name}");

            // Load all devices
            var deviceListResult = await _deviceService.LoadDeviceList();
            if (!deviceListResult.IsSuccessful)
            {
                Debug.WriteLine("Failed to load device list");
                return false;
            }

            var devices = deviceListResult.Devices;
            var deviceToUpdate = devices.FirstOrDefault(d => d.Id == SelectedDevice.Id);

            if (deviceToUpdate != null)
            {
                // Update name but keep serial number unchanged
                deviceToUpdate.Name = SelectedDevice.Name;

                // Save the updated list
                await _deviceService.SaveDeviceListToSecureStore(devices);
                Debug.WriteLine($"Device name updated successfully: {SelectedDevice.Name}");
                return true;
            }

            Debug.WriteLine("Device not found in the list");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving device name: {ex.Message}");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}