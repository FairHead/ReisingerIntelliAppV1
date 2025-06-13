using CommunityToolkit.Maui.Views;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Services;
using KeyInputPopup = ReisingerIntelliAppV1.Views.PopUp.KeyInputPopup;

namespace ReisingerIntelliAppV1.Views;

public partial class ScanListPage : ContentPage
{
    private readonly ScanListViewModel _scanListViewModel;
    private readonly IntellidriveApiService _intellidriveApiService;
    private bool _isBusy;

    public ScanListPage(ScanListViewModel scanListViewModel, IntellidriveApiService intellidriveApiService)
    {
        InitializeComponent();
        _scanListViewModel = scanListViewModel;
        _intellidriveApiService = intellidriveApiService;
        BindingContext = _scanListViewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (locationStatus != PermissionStatus.Granted)
        {
            locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (locationStatus == PermissionStatus.Granted)
        {
            await _scanListViewModel.LoadWifiNetworksAsync();
        }
        else
        {
            bool goToSettings = await DisplayAlert(
                "Standort-Berechtigung benötigt",
                "Damit verfügbare WLAN-Geräte angezeigt werden können, muss die Standortfreigabe aktiviert sein.",
                "Zu den Einstellungen",
                "Abbrechen");

            if (goToSettings)
            {
#if ANDROID
            try
            {
                Android.Content.Intent intent = new(Android.Provider.Settings.ActionApplicationDetailsSettings);
                intent.SetData(Android.Net.Uri.Parse("package:" + Android.App.Application.Context.PackageName));
                intent.SetFlags(Android.Content.ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Konnte Einstellungen nicht öffnen: {ex.Message}", "OK");
            }
#endif
            }
        }
    }


    private async void OnClick(object sender, EventArgs e)
    {
        if (_isBusy) return;

        try
        {
            _isBusy = true;

            if (sender is not Button btn || btn.CommandParameter is not NetworkDataModel network)
                return;

            var action = await _scanListViewModel.TryPairDevice(network);

            switch (action)
            {
                case ScanResultAction.NotConnected:
                    await DisplayAlert("Fehler", "Verbindung fehlgeschlagen.", "OK");
                    return;

                case ScanResultAction.AlreadyExists:
                    bool goToSaved = await DisplayAlert("Gerät existiert",
                        $"Gerät mit SSID {network.Ssid} existiert bereits. Zur Geräteliste wechseln?",
                        "Ja", "Nein");

                    if (goToSaved)
                    {
                        var mauiContext = App.Current?.Handler?.MauiContext;
                        if (mauiContext == null)
                        {
                            await DisplayAlert("Fehler", "MauiContext ist nicht verfügbar.", "OK");
                            return;
                        }

                        var page = mauiContext.Services.GetRequiredService<SavedDeviceListPage>();
                        await Navigation.PushAsync(page);
                    }
                    return;

                case ScanResultAction.ReadyToPair:
                    var result = await this.ShowPopupAsync(new KeyInputPopup());

                    if (result is AuthDataModel authData)
                    {
                        var popup = await _scanListViewModel.PrepareSavePopupAsync(network, authData);

                        if (popup != null)
                            await this.ShowPopupAsync(popup);
                        else
                            await DisplayAlert("Fehlgeschlagen", "Authentifizierung ungültig.", "OK");
                    }
                    return;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Ein unerwarteter Fehler ist aufgetreten: {ex.Message}", "OK");
        }
        finally
        {
            _isBusy = false;
        }
    }
}