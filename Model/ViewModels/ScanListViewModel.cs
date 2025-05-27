using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Services;
using System.Collections.ObjectModel;
using SaveDevicePopUp = ReisingerIntelliAppV1.Views.PopUp.SaveDevicePopUp;

namespace ReisingerIntelliAppV1.ViewModels;

public partial class ScanListViewModel : BaseViewModel
{
    private readonly WifiService _wifiService;
    private readonly DeviceService _deviceService;
    private readonly IntellidriveApiService _apiService;

    public ObservableCollection<NetworkDataModel> WifiNetworks { get; } = new();

    public ScanListViewModel(WifiService wifiService, DeviceService deviceService, IntellidriveApiService apiService)
    {
        _wifiService = wifiService;
        _deviceService = deviceService;
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadWifiNetworksAsync()
    {
        if (IsBusy) return;
        
        try
        {
            IsBusy = true;
            WifiNetworks.Clear();

            var networks = await _wifiService.ScanNetworksAsync();
            var savedWifiSsids = (await _deviceService.LoadDeviceList()).Devices
                .Where(d => d.ConnectionType == ConnectionType.Wifi)
                .Select(d => d.Ssid);

            foreach (var net in networks)
            {
                // jetzt true nur, wenn dieselbe SSID schon als WLAN-Gerät gespeichert ist
                net.IsAlreadySaved = savedWifiSsids.Contains(net.Ssid);
                WifiNetworks.Add(net);
            }
        }
        catch (Exception ex)
        {
            var mainPage = Application.Current?.Windows[0]?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlert("Fehler", 
                    $"Fehler beim Scannen der WLAN-Netzwerke: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<ScanResultAction> TryPairDevice(NetworkDataModel network)
    {
        if (string.IsNullOrEmpty(network.Ssid))
            return ScanResultAction.NotConnected;

        bool connected = await _wifiService.EnsureConnectedToSsidAsync(network.Ssid);
        if (!connected) return ScanResultAction.NotConnected;

        if (await _deviceService.DeviceExistsBySsidAsync(network.Ssid))
            return ScanResultAction.AlreadyExists;

        return ScanResultAction.ReadyToPair;
    }



    public async Task<SaveDevicePopUp?> PrepareSavePopupAsync(NetworkDataModel network, AuthDataModel authData)
    {
        if (string.IsNullOrEmpty(authData.Username) || string.IsNullOrEmpty(authData.Password))
            return null;
   
        var response = await _apiService.TestUserAuthAsync(authData.Username, authData.Password);
        if (response?.Success != true)
            return null;

        var responseData = await _apiService.GetRequestNoAuthForWifi("intellidrive/version");


        if (responseData == null)
        {
            var mainPage = Application.Current?.Windows[0]?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlert("Verbindungsfehler", "Das Gerät konnte nicht erreicht werden.", "OK");
            }
            return null;
        }
        var model = NetworkDataModel.FromAuthResponseAndAuthData(response, network, authData, responseData);
        return new SaveDevicePopUp(model, _deviceService);
    }



}

public enum ScanResultAction
{
    NotConnected,
    AlreadyExists,
    ReadyToPair
}
