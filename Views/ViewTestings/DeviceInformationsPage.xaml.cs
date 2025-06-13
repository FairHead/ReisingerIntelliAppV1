using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;
using System.Diagnostics;
using ReisingerIntelliAppV1.Model.ViewModels;

namespace ReisingerIntelliAppV1.Views.ViewTestings;

public partial class DeviceInformationsPage : ContentPage
{
    private readonly DeviceInformationsViewModel _viewModel;
    private readonly DeviceService _deviceService;
    
    public DeviceInformationsPage(
        DeviceInformationsViewModel viewModel,
        DeviceService deviceService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _deviceService = deviceService;
        BindingContext = _viewModel;
    }

    public async Task InitializeWithAsync(DeviceModel device)
    {
        await _viewModel.InitializeAsync(device);
    }

    public async void GoToDeviceInformationsPage(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var device = button?.CommandParameter as DeviceModel;
        if (device == null) return;

        var infoPage = App.ServiceProvider.GetRequiredService<DeviceInformationsPage>();
        await infoPage.InitializeWithAsync(device);
        await Navigation.PushAsync(infoPage);
    }

    private async void OnSaveDeviceNameClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(DeviceNameEntry.Text))
            {
                await DisplayAlert("Fehler", "Bitte geben Sie einen Gerätenamen ein.", "OK");
                return;
            }

            // Update the view model with the new name
            _viewModel.SelectedDevice.Name = DeviceNameEntry.Text.Trim();
            
            // Save the updated name
            bool success = await _viewModel.SaveDeviceNameAsync();
            
            if (success)
            {
                await DisplayAlert("Erfolg", "Gerätename wurde erfolgreich aktualisiert.", "OK");
                Debug.WriteLine($"Device name updated: {_viewModel.SelectedDevice.Name}");
            }
            else
            {
                await DisplayAlert("Fehler", "Gerätename konnte nicht gespeichert werden.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving device name: {ex.Message}");
            await DisplayAlert("Fehler", $"Fehler beim Speichern des Gerätenamens: {ex.Message}", "OK");
        }
    }
}