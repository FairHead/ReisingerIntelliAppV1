using CommunityToolkit.Maui.Views;
using ReisingerIntelliAppV1.Model.Models;
using System.Diagnostics;

namespace ReisingerIntelliAppV1.Views.PopUp;

public partial class LocalDeviceNamePopup : Popup
{
    private readonly LocalNetworkDeviceModel _device;
    
    public LocalDeviceNamePopup(LocalNetworkDeviceModel device)
    {
        InitializeComponent();
        _device = device;
        BindingContext = device;
        
        // Pre-populate the device name field with the serial number
        DeviceNameEntry.Text = device.SerialNumber;
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        var deviceName = DeviceNameEntry.Text?.Trim();
        
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            Application.Current?.MainPage?.DisplayAlert(
                "Fehler",
                "Bitte geben Sie einen Namen für das Gerät ein.",
                "OK"
            );
            return;
        }
        
        // Create a copy of the device with the new name
        var updatedDevice = new LocalNetworkDeviceModel
        {
            IpAddress = _device.IpAddress,
            DeviceId = _device.DeviceId,
            SerialNumber = _device.SerialNumber,
            FirmwareVersion = _device.FirmwareVersion,
            SoftwareVersion = _device.SoftwareVersion,
            LatestFirmware = _device.LatestFirmware,
            IsAlreadySaved = _device.IsAlreadySaved,
            // Add a custom name property to store user-entered name
            CustomName = deviceName
        };
        
        Debug.WriteLine($"Device named as: {deviceName}");
        
        this.Close(updatedDevice);
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        this.Close(null);
    }
}