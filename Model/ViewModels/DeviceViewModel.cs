using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using ReisingerIntelliAppV1.Interface;


namespace ReisingerIntelliAppV1.Model.ViewModels
{
    public partial class DeviceViewModel : ObservableObject, IAsyncInitializable
    {
        public DeviceModel SelectedDevice { get; set; }


        private readonly WifiService _wifiService;
        private readonly DeviceService _deviceService;
        private System.Timers.Timer _statusUpdateTimer;
        private readonly IntellidriveApiService _apiService;
        public DeviceViewModel(WifiService wifiService, DeviceService deviceService, IntellidriveApiService apiService)
        {
            _wifiService = wifiService;
            _deviceService = deviceService;
            _apiService = apiService;
        }

        public ObservableCollection<DeviceModel> Devices { get; } = new ObservableCollection<DeviceModel>();

        public async Task InitializeAsync()
        {
            await RefreshDeviceStatus();
        }

        public async Task RefreshDeviceStatus()
        {
            var (isSuccessful, isEmpty, devices) = await _deviceService.LoadDeviceList();

            if (isSuccessful && !isEmpty)
            {
                var updatedDevices = await _wifiService.CheckDeviceNetworkStatusAsync(devices);

                // Geräte in der Liste aktualisieren (nicht komplett neu setzen, damit Bindings erhalten bleiben)
                for (int i = 0; i < updatedDevices.Count; i++)
                {
                    if (i < Devices.Count)
                    {
                        Devices[i].IsOnline = updatedDevices[i].IsOnline;
                    }
                    else
                    {
                        Devices.Add(updatedDevices[i]);
                    }
                }

                // Entferne überzählige Geräte
                while (Devices.Count > updatedDevices.Count)
                {
                    Devices.RemoveAt(Devices.Count - 1);
                }
            }
        }

        public void StartStatusUpdater()
        {
            if (_statusUpdateTimer != null)
                return; // Schon aktiv

            _statusUpdateTimer = new System.Timers.Timer(1000); // alle 1 Sekunde
            _statusUpdateTimer.Elapsed += async (s, e) =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await RefreshDeviceStatus(); // Online/Offline
                });
            };
            _statusUpdateTimer.AutoReset = true;
            _statusUpdateTimer.Enabled = true;
        }



        public void StopStatusUpdater()
        {
            if (_statusUpdateTimer != null)
            {
                _statusUpdateTimer.Stop();
                _statusUpdateTimer.Dispose();
                _statusUpdateTimer = null;
            }
        }

        public async Task UpdateDoorState(DeviceModel device)
        {
            try
            {
             
                var resultJson = await _apiService.GetDoorStateAsync(device);

                using var doc = JsonDocument.Parse(resultJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("Content", out var content) &&
                    content.TryGetProperty("DOOR_STATE", out var doorStateProp))
                {
                    string doorState = doorStateProp.GetString();
                    device.IsDoorClosed = doorState?.ToLower() == "closed";
                }
                else
                {
                    device.IsDoorClosed = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Türstatus: {ex.Message}");
                device.IsDoorClosed = false;
            }
        }

    }
}
