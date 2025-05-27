using CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Services;

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
}