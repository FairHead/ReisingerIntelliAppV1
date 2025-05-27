// DeviceDoorFunctionPage.xaml.cs
using ReisingerIntelliAppV1.Base;
using ReisingerIntelliAppV1.Helpers;
using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using System.Text.Json;

namespace ReisingerIntelliAppV1.Views.DeviceControlViews;

[QueryProperty(nameof(Device), "Device")]
public partial class DeviceDoorFunctionPage : BasePage, IDevicePage
{    private new DeviceModel? _device;
    private new DeviceSettingsViewModel? _viewModel;
    private readonly IntellidriveApiService _apiService;

    public DeviceModel Device
    {
        get => _device!;
        set => _device = value;
    }

    public DeviceDoorFunctionPage(IntellidriveApiService apiService) : base(apiService)
    {
        _apiService = apiService;
        InitializeComponent();
    }    public Task RefreshAsync()
    {
        // Nothing specific to refresh for the door function page
        // We rely on the DeviceSettingsViewModel's door state updater
        
        // Make sure loading indicator is not showing after refresh
        if (_viewModel != null)
        {
            _viewModel.IsBusy = false;
        }
        
        return Task.CompletedTask;
    }


    public override void InitializeWith(DeviceSettingsViewModel viewModel, DeviceModel device)
    {
        _device = device;
        _viewModel = viewModel;
        BindingContext = _viewModel;
        base.InitializeWith(viewModel, device);
    }

    private async void DoorFullOpen(object? sender, EventArgs e)
    {
        if (_viewModel?.IsLocked == true)
        {
            await DisplayAlert("Tür verriegelt", "Bitte zuerst entriegeln.", "OK");
            return;
        }

        try
        {
            string result = await _apiService.OpenDoorFullAsync(_device);
            await DisplayAlert("Erfolg", "Tür vollständig geöffnet.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void OnLockUnlockClicked(object sender, EventArgs e)
    {
        if (_device == null || _viewModel == null) return;

        try
        {
            if (!_viewModel.IsLocked)
            {
                var doorStateJson = await _apiService.GetDoorStateAsync(_device);
                var doorState = JsonSerializer.Deserialize<DoorStateResponse>(doorStateJson);
                bool isDoorClosed = doorState?.Content?.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true;

                if (!isDoorClosed)
                {
                    await DisplayAlert("Tür geöffnet", "Tür wird geschlossen...", "OK");
                    await _apiService.OpenDoorAsync(_device);
                }

                await _apiService.LockDoorAsync(_device);
                _viewModel.IsLocked = true;
                LockUnlockButton.Text = "Unlock";
                LockUnlockButton.BackgroundColor = Colors.Red;
            }
            else
            {
                await _apiService.UnlockDoorAsync(_device);
                _viewModel.IsLocked = false;
                LockUnlockButton.Text = "Lock";
                LockUnlockButton.BackgroundColor = Colors.Green;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void OnSummerModeClicked(object sender, EventArgs e)
    {
        if (_device == null || _viewModel == null) return;

        try
        {
            if (_viewModel.IsSummerModeEnabled)
            {
                await _apiService.DisableSummerModeAsync(_device);
                _viewModel.IsSummerModeEnabled = false;
                SummerModeButton.Text = "Enable Summer Mode";
                SummerModeButton.BackgroundColor = Colors.Green;
            }
            else
            {
                await _apiService.EnableSummerModeAsync(_device);
                _viewModel.IsSummerModeEnabled = true;
                SummerModeButton.Text = "Disable Summer Mode";
                SummerModeButton.BackgroundColor = Colors.Red;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void OnOpenCloseCheckStatus(object sender, EventArgs e)
    {
        if (_device == null || _viewModel == null) return;

        if (_viewModel.IsLocked)
        {
            await DisplayAlert("Tür verriegelt", "Erst entriegeln, um die Tür zu öffnen.", "OK");
            return;
        }

        try
        {
            var initialJson = await _apiService.GetDoorStateAsync(_device);
            var initialResult = JsonSerializer.Deserialize<DoorStateResponse>(initialJson);
            bool wasClosed = initialResult?.Content?.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true;

            await _apiService.OpenDoorAsync(_device);

            const int maxAttempts = 10;
            for (int i = 0; i < maxAttempts; i++)
            {
                await Task.Delay(1000);
                var status = await _apiService.GetDoorStateAsync(_device);
                var newState = JsonSerializer.Deserialize<DoorStateResponse>(status);

                bool isClosedNow = newState?.Content?.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true;
                if (isClosedNow != wasClosed)
                {
                    _viewModel.IsDoorOpen = !isClosedNow;
                    await DisplayAlert("Status", isClosedNow ? "Tür geschlossen." : "Tür geöffnet.", "OK");
                    return;
                }
            }

            await DisplayAlert("Hinweis", "Türstatus hat sich nicht geändert.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void Identify(object? sender, EventArgs e)
    {
        try
        {
            string result = await _apiService.BeepAsync(_device);
            await DisplayAlert("Signal", "Gerät hat Signalton ausgegeben.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Identifizierung fehlgeschlagen: {ex.Message}", "OK");
        }
    }

    private async void DoorPartialOpen(object? sender, EventArgs e)
    {
        if (_device == null || _viewModel == null) return;

        try
        {
            // Aktuellen Türstatus abrufen
            var doorStateJson = await _apiService.GetDoorStateAsync(_device);
            var doorState = JsonSerializer.Deserialize<DoorStateResponse>(doorStateJson);

            bool isClosed = doorState?.Content?.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true;

            // Falls offen: zuerst schließen
            if (!isClosed)
            {
                await DisplayAlert("Hinweis", "🚪 Tür wird zuerst geschlossen.", "OK");
                await _apiService.OpenDoorAsync(_device); // Toggle = schließt wenn offen

                const int maxAttempts = 10;
                int attempts = 0;
                while (attempts < maxAttempts)
                {
                    await Task.Delay(1000);
                    doorStateJson = await _apiService.GetDoorStateAsync(_device);
                    doorState = JsonSerializer.Deserialize<DoorStateResponse>(doorStateJson);

                    isClosed = doorState?.Content?.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true;

                    if (isClosed)
                        break;

                    attempts++;
                }

                if (!isClosed)
                {
                    await DisplayAlert("Fehler", "❗️Tür konnte nicht geschlossen werden.", "OK");
                    return;
                }
            }

            // Jetzt Teilöffnung
            string result = await _apiService.OpenDoorShortAsync(_device);
            await DisplayAlert("Erfolg", "🚪 Tür wurde teilweise geöffnet.", "OK");

            // UI-Status aktualisieren
            await _viewModel.UpdateDoorState(_device);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"Teilöffnung fehlgeschlagen: {ex.Message}", "OK");
        }
    }


    private async void DoorForceClose(object? sender, EventArgs e)
    {
        if (_device == null || _viewModel == null) return;

        try
        {
            // 1. Türstatus abrufen
            var doorStateJson = await _apiService.GetDoorStateAsync(_device);
            var doorState = JsonSerializer.Deserialize<DoorStateResponse>(doorStateJson);
            bool isClosed = doorState?.Content?.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true;

            if (isClosed)
            {
                await DisplayAlert("Bereits geschlossen", "🚪 Die Tür ist bereits geschlossen.", "OK");
                return;
            }

            // 2. Sicherheitswarnung
            bool confirm = await DisplayAlert("Warnung",
                "⚠️ Die Tür ist offen und wird jetzt **erzwingend geschlossen**.\n" +
                "Bitte sicherstellen, dass sich **keine Gegenstände oder Personen** im Türbereich befinden.",
                "Tür schließen", "Abbrechen");

            if (!confirm)
                return;

            // 3. Tür erzwingen schließen
            await _apiService.ForceCloseDoorAsync(_device);

            // 4. Abwarten bis Tür tatsächlich zu ist
            const int maxAttempts = 10;
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                await Task.Delay(1000);

                doorStateJson = await _apiService.GetDoorStateAsync(_device);
                doorState = JsonSerializer.Deserialize<DoorStateResponse>(doorStateJson);
                isClosed = doorState?.Content?.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true;

                if (isClosed)
                {
                    _viewModel.IsDoorOpen = false;
                    await DisplayAlert("Erfolg", "🚪 Die Tür wurde erfolgreich geschlossen.", "OK");
                    return;
                }

                attempts++;
            }

            await DisplayAlert("Fehler", "❗️Die Tür konnte nicht vollständig geschlossen werden.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", $"❌ Fehler beim Erzwingen des Türschließens: {ex.Message}", "OK");
        }
    }

}
