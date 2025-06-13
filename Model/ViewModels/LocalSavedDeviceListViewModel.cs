using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;
using ReisingerIntelliAppV1.Views.DeviceControlViews;

namespace ReisingerIntelliAppV1.Model.ViewModels
{
    public partial class LocalSavedDeviceListViewModel : ObservableObject
    {
        private readonly DeviceService _deviceService;
        private readonly WifiService _wifiService;
        private System.Timers.Timer? _timer;

        public ObservableCollection<Grouping<string, DeviceModel>> Groups { get; } = new();

        public IAsyncRelayCommand<DeviceModel> DeleteCommand { get; }
        public IAsyncRelayCommand<DeviceModel> ConfigureCommand { get; }

        [ObservableProperty] bool isBusy;
        public bool IsNotBusy => !IsBusy;

   


        public LocalSavedDeviceListViewModel(
            DeviceService deviceService,
            WifiService wifiService)
        {
            _deviceService = deviceService;
            _wifiService = wifiService;

            DeleteCommand = new AsyncRelayCommand<DeviceModel>(DeleteAsync);
            ConfigureCommand = new AsyncRelayCommand<DeviceModel>(ConfigureAsync);
        }

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                Groups.Clear();

                var all = (await _deviceService.LoadDeviceList())
                    .Devices
                    .Where(d => d.ConnectionType == ConnectionType.Local);

                var bySsid = all
                    .GroupBy(d => d.Ssid)
                    .Select(g => new Grouping<string, DeviceModel>(g.Key, g));

                foreach (var grp in bySsid)
                    Groups.Add(grp);
            }
            finally { IsBusy = false; }
        }

        async Task DeleteAsync(DeviceModel device)
        {
            bool ok = await Application.Current.MainPage.DisplayAlert(
                "Löschen?",
                $"Gerät '{device.Name}' löschen?",
                "Ja", "Nein");
            if (!ok) return;
            await _deviceService.DeleteDeviceAsync(device);
            await LoadAsync();
        }

        async Task ConfigureAsync(DeviceModel device)
        {
            try
            {
                IsBusy = true;

                if (string.IsNullOrEmpty(device.Ip) || device.Ip == "0.0.0.0")
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Fehler",
                        "Ungültige IP-Adresse für das Gerät",
                        "OK");
                    return;
                }

                bool connected = await _wifiService.EnsureConnectedToSsidAsync(device.Ssid);
                if (!connected)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Verbindungsfehler",
                        $"Konnte keine Verbindung zum WLAN '{device.Ssid}' herstellen. Bitte stelle sicher, dass das Gerät eingeschaltet ist und sich in Reichweite befindet.",
                        "OK");
                    return;
                }

                await Task.Delay(2000); // kurze Wartezeit, um die Verbindung zu stabilisieren

                var page = App.Current.Handler.MauiContext.Services
                    .GetRequiredService<DeviceSettingsTabbedPage>();

                await page.InitializeWithAsync(device); // nur Grundstruktur
                await Application.Current.MainPage.Navigation.PushAsync(page);

                // 🚀 Parameter werden jetzt im Hintergrund geladen
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(300); // UI-Pause für flüssiges Erlebnis
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await page.LoadParametersAsync(); // startet Türstatus + Parametervorgang
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Fehler beim Hintergrund-Parameterladen: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nDetails: {ex.InnerException.Message}";
                }

                if (ex.ToString().Contains("net_http_client_execution_error"))
                {
                    errorMessage = "Netzwerkfehler: Verbindung zum Gerät fehlgeschlagen. Bitte überprüfe, ob das Gerät eingeschaltet ist und du mit dem richtigen WLAN verbunden bist.";
                }

                await Application.Current.MainPage.DisplayAlert(
                    "Fehler beim Laden der Geräteeinstellungen",
                    errorMessage,
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void StartOnlineStatusUpdater()
        {
            MainThread.InvokeOnMainThreadAsync(UpdateStatusAsync);
            if (_timer != null) return;
            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += async (_, _) => await MainThread.InvokeOnMainThreadAsync(UpdateStatusAsync);
            _timer.AutoReset = true;
            _timer.Start();
        }

        public void StopOnlineStatusUpdater()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        async Task UpdateStatusAsync()
        {
            var flat = Groups.SelectMany(g => g).ToList();
            var updated = await _wifiService.CheckDeviceNetworkStatusAsync(flat);
            for (int i = 0; i < updated.Count; i++)
                flat[i].IsOnline = updated[i].IsOnline;
        }
    }

    public class Grouping<K, T> : ObservableCollection<T>
    {
        public K Key { get; }
        public Grouping(K key, IEnumerable<T> items) : base(items) => Key = key;
    }
}
