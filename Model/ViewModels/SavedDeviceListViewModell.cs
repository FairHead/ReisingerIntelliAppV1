using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;

namespace ReisingerIntelliAppV1.Model.ViewModels
{
    public partial class SavedDeviceListViewModel : BaseViewModel 
    {
        private readonly WifiService _wifiService;
        private readonly DeviceService _deviceService;
        private System.Timers.Timer? _statusUpdateTimer;
        public ObservableCollection<DeviceModel> Devices { get; } = new();

        public SavedDeviceListViewModel(WifiService wifiService, DeviceService deviceService)
        {
            _wifiService = wifiService;
            _deviceService = deviceService;
            
            //ettingsCommand = new AsyncRelayCommand<DeviceModel>(OnSettingsAsync);
            DeleteCommand = new AsyncRelayCommand<DeviceModel>(OnDeleteAsync);
        }

       // public IAsyncRelayCommand<DeviceModel> SettingsCommand { get; }
        public IAsyncRelayCommand<DeviceModel> DeleteCommand { get; }

        // private async Task OnSettingsAsync(DeviceModel device)
        // {
        //     if (device == null) return;
            
        //     bool connected = await _wifiService.EnsureConnectedToSsidAsync(device.Ssid);
        //     if (!connected) return;

        //     try
        //     {
        //         IsBusy = true;
                
        //         if (Application.Current?.Handler?.MauiContext?.Services is IServiceProvider services)
        //         {
        //             var settingsPage = services.GetRequiredService<DeviceSettingsTabbedPage>();
        //             await settingsPage.InitializeWithAsync(device);
        //             await Application.Current.MainPage.Navigation.PushAsync(settingsPage);
        //         }
        //     }
        //     finally
        //     {
        //         IsBusy = false;
        //     }
        // }

        private async Task OnDeleteAsync(DeviceModel device)
        {
            if (device == null) return;

            bool answer = await Application.Current.MainPage.DisplayAlert(
                "Gerät löschen",
                $"Möchten Sie das Gerät '{device.Name}' wirklich löschen?",
                "Ja",
                "Nein");

            if (answer)
            {
                await _deviceService.DeleteDeviceAsync(device);
                Devices.Remove(device);
            }
        }

        public async Task InitializeAsync()
        {
            await LoadSavedDevicesAsync();
        }

        public async Task LoadSavedDevicesAsync()
        {
            var (success, isEmpty, devices) = await _deviceService.LoadDeviceList();
            if (!success || isEmpty)
                return;

            Devices.Clear();

            // Nur WLAN-Geräte anzeigen
            foreach (var device in devices.Where(d => d.ConnectionType == ConnectionType.Wifi))
            {
                Devices.Add(device);
            }
        }

        public async Task UpdateDeviceOnlineStatusAsync()
        {
            if (Devices.Count == 0)
                return;

            var updated = await _wifiService.CheckDeviceNetworkStatusAsync(Devices.ToList());

            for (int i = 0; i < updated.Count; i++)
            {
                Devices[i].IsOnline = updated[i].IsOnline;
            }
        }


        public void StartOnlineStatusUpdater()
        {
            if (_statusUpdateTimer != null)
                return;
              MainThread.InvokeOnMainThreadAsync(UpdateDeviceOnlineStatusAsync);
            _statusUpdateTimer = new System.Timers.Timer(5000); // alle 5 Sekunden
            _statusUpdateTimer.Elapsed += async (s, e) =>
            {
                await MainThread.InvokeOnMainThreadAsync(UpdateDeviceOnlineStatusAsync);
            };
            _statusUpdateTimer.AutoReset = true;
            _statusUpdateTimer.Start();
        }

        public void StopOnlineStatusUpdater()
        {
            _statusUpdateTimer?.Stop();
            _statusUpdateTimer?.Dispose();
            _statusUpdateTimer = null;
        }

    }

}
