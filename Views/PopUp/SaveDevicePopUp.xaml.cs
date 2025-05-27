using CommunityToolkit.Maui.Views;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;

namespace ReisingerIntelliAppV1.Views.PopUp;

public partial class SaveDevicePopUp : Popup
{
    private readonly DeviceService _deviceService;

    public SaveDevicePopUp(NetworkDataModel network, DeviceService deviceService)
    {
        InitializeComponent();
        BindingContext = network;
        _deviceService = deviceService;
    }
    private void OnCancelClicked(object? sender, EventArgs e)
    {
        // Popup schließen
        this.Close();
    }
    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not NetworkDataModel network)
            return;

        if (!IsValidNetworkName(network.Name))
            return;

        try
        {
            var deviceModel = DeviceModel.FromNetworkData(network);
            
            // Set connection type explicitly to WiFi
            deviceModel.ConnectionType = ConnectionType.Wifi;
            
            // Make sure to use the standard IP for WiFi devices
            if (deviceModel.ConnectionType == ConnectionType.Wifi)
            {
                deviceModel.Ip = "192.168.4.100";
                System.Diagnostics.Debug.WriteLine($"Saving WiFi device with IP: {deviceModel.Ip}");
            }
            
            if (await _deviceService.DeviceExists(deviceModel.DeviceId))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Warnung",
                    $"Ein Gerät mit der ID '{deviceModel.DeviceId}' existiert bereits in der Liste und kann nicht erneut gespeichert werden.",
                    "OK"
                );
                return;
            }

            await SaveDeviceAndShowSuccessMessage(deviceModel);
            this.Close(); // Popup schließen
        }
        catch (Exception ex)
        {
            await HandleSaveError(ex);
        }
    }
    private bool IsValidNetworkName(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Application.Current.MainPage.DisplayAlert(
                "Fehler",
                "Bitte geben Sie einen Gerätenamen ein.",
                "OK"
            ).Wait();
            return false;
        }

        return true;
    }
   
    private async Task SaveDeviceAndShowSuccessMessage(DeviceModel deviceModel)
    {
        // Gerät speichern und aktualisierte Liste holen
        var devices = await _deviceService.AddDeviceAndReturnUpdatedList(deviceModel);

        // Geräteliste für die Erfolgsmeldung erstellen
        var deviceNames = devices.Select(d => d.Name).ToList();
        string deviceListMessage = string.Join("\n", deviceNames);

        // Erfolgsmeldung anzeigen
        await Application.Current.MainPage.DisplayAlert(
            "Gerät gespeichert!",
            $"Das Gerät wurde erfolgreich gespeichert.\n\nListe der Geräte:\n{deviceListMessage}",
            "OK"
        );
    }
    private async Task HandleSaveError(Exception ex)
    {
        Console.WriteLine($"Fehler beim Speichern des Geräts: {ex.Message}");
        await Application.Current.MainPage.DisplayAlert(
            "Fehler",
            $"Ein Fehler ist aufgetreten: {ex.Message}",
            "OK"
        );
    }

}