using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Services;
using ReisingerIntelliAppV1.Views.DeviceControlViews;
using ReisingerIntelliAppV1.Views.ViewTestings;
using Microsoft.Maui.Controls;

namespace ReisingerIntelliAppV1.Views;
public partial class SavedDeviceListPage : ContentPage
{
    private readonly SavedDeviceListViewModel _savedDeviceListViewModel;
    private readonly IntellidriveApiService _apiService;
    private readonly WifiService _wifiService;
    private bool _isNavigating = false;

    public SavedDeviceListPage(SavedDeviceListViewModel deviceListViewModel, IntellidriveApiService apiService, WifiService wifiService)
    {
        InitializeComponent();
        _savedDeviceListViewModel = deviceListViewModel;
        _apiService = apiService;
        _wifiService = wifiService;
        BindingContext = _savedDeviceListViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Reset des Navigations-Flags, wenn die Seite wieder erscheint
        _isNavigating = false;

        await _savedDeviceListViewModel.LoadSavedDevicesAsync();
        _savedDeviceListViewModel.StartOnlineStatusUpdater();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _savedDeviceListViewModel.StopOnlineStatusUpdater();
    }

    private async void OnConfigureButtonClicked(object sender, EventArgs e)
    {
        // Mehrfache Navigationsaufrufe verhindern
        if (_savedDeviceListViewModel.IsBusy || _isNavigating) return;

        _savedDeviceListViewModel.IsBusy = true;
        _isNavigating = true;

        var button = sender as Button;
        var device = button?.CommandParameter as DeviceModel;
        if (device == null)
        {
            _savedDeviceListViewModel.IsBusy = false;
            _isNavigating = false;
            return;
        }

        try
        {
            // Schnelle WLAN-Verbindung ohne Benutzerblockierung
            bool connected = false;

            // Timeout für WLAN-Verbindung, um Blockierung zu vermeiden
            var wifiConnectTask = _wifiService.EnsureConnectedToSsidAsync(device.Ssid);
            var timeoutTask = Task.Delay(5000); // 5 Sekunden maximale Wartezeit

            var completedTask = await Task.WhenAny(wifiConnectTask, timeoutTask);

            if (completedTask == wifiConnectTask)
            {
                connected = await wifiConnectTask;
            }
            else
            {
                // Timeout - trotzdem fortfahren, könnte bereits verbunden sein
                System.Diagnostics.Debug.WriteLine("WLAN-Verbindung Timeout, versuche trotzdem");
                connected = true;
            }

            if (!connected)
            {
                await DisplayAlert("Verbindungsproblem", "Verbindung zu WLAN konnte nicht hergestellt werden", "OK");
                _savedDeviceListViewModel.IsBusy = false;
                _isNavigating = false;
                return;
            }

            // Optimierung: Lade die Tabs sofort ohne auf Parameter zu warten
            var settingsPage = App.Current.Handler.MauiContext.Services
                .GetRequiredService<DeviceSettingsTabbedPage>();

            // Nur minimale Geräteinitialisierung durchführen (keine InitializeWithAsync mehr)
           // settingsPage.InitializeDevice(device);

            // Sofort zur Seite navigieren
            await Navigation.PushAsync(settingsPage);

            // UI-Blockierung aufheben, sobald die Navigation erfolgt ist
            _savedDeviceListViewModel.IsBusy = false;

            // Parameter werden automatisch in OnAppearing der TabbedPage geladen
            // KEIN manueller Aufruf von LoadParametersAsync mehr hier!
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler in OnConfigureButtonClicked: {ex.Message}");
            await DisplayAlert("Fehler", $"Fehler beim Abrufen der Parameter: {ex.Message}", "OK");
            _savedDeviceListViewModel.IsBusy = false;
            _isNavigating = false;
        }
    }

    public async void GoToDeviceInformationsPage(object sender, EventArgs e)
    {
        // Doppelte Navigationsaktionen verhindern
        if (_isNavigating) return;
        _isNavigating = true;

        try
        {
            var button = sender as ImageButton;
            var device = button?.CommandParameter as DeviceModel;
            if (device == null)
            {
                _isNavigating = false;
                return;
            }

            var infoPage = App.ServiceProvider.GetRequiredService<DeviceInformationsPage>();
            await infoPage.InitializeWithAsync(device);
            await Navigation.PushAsync(infoPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Navigieren zur Infoseite: {ex.Message}");
            await DisplayAlert("Fehler", "Fehler beim Öffnen der Geräteinfos", "OK");
        }
        finally
        {
            // Nach Navigation wieder freigeben
            _isNavigating = false;
        }
    }
}
