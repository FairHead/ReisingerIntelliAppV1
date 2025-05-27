using ReisingerIntelliAppV1.Base;
using ReisingerIntelliAppV1.Helpers;
using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using System.Text.Json;

namespace ReisingerIntelliAppV1.Views.DeviceControlViews;

[QueryProperty(nameof(Device), "Device")]
public partial class DeviceFunctionPage : BasePage, IDevicePage
{
    private new DeviceModel? _device;
    private new DeviceSettingsViewModel? _viewModel;
    private readonly IntellidriveApiService _apiService;

    public DeviceModel Device
    {
        get => _device!;
        set => _device = value;
    }
    public DeviceFunctionPage(IntellidriveApiService apiService) : base(apiService)
    {
        _apiService = apiService;
        InitializeComponent();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine($"📋 Tab sichtbar: {GetType().Name}");
    }

    public override void InitializeWith(DeviceSettingsViewModel viewModel, DeviceModel device)
    {
        _device = device;
        _viewModel = viewModel;
        base.InitializeWith(viewModel, device);
    }

    private async void OnClick(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await NavigateAsync(button);
        }
    }
    public async Task RefreshAsync()
    {
        
                _viewModel.IsBusy = false;
     
    }
    private async void OnCalibrateClicked(object sender, EventArgs e)
    {
        try
        {
            var json = await _apiService.GetDoorStateAsync(_device);
            var result = JsonSerializer.Deserialize<DoorStateResponse>(json);

            bool isClosed = result?.Content?.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true;

            if (!isClosed)
            {
                await DisplayAlert("Achtung", "Die Tür ist offen. Kalibrierung nicht möglich.", "OK");
                return;
            }

            // Tür ist geschlossen – Kalibrierung starten
            string resultText = await _apiService.CalibrateAsync(_device);

            // Optional: Türstatus nochmal anzeigen
            string doorState = result?.Content?.DOOR_STATE ?? "Unbekannt";

            await DisplayAlert("Erfolg", $"Referenzfahrt gestartet. Türstatus: {doorState}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Kalibrierung fehlgeschlagen: {ex.Message}", "OK");
        }
    }


   

   




    private void UpdateData(object sender, EventArgs e)
    {
        // Code für Datenaktualisierung
    }

    private void SetDateTime(object sender, EventArgs e)
    {
        // Code zum Setzen von Datum und Zeit
    }
}