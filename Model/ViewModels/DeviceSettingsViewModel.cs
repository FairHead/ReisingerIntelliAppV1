using CommunityToolkit.Mvvm.ComponentModel;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;
using System.Text.Json;
using Timer = System.Timers.Timer;

namespace ReisingerIntelliAppV1.Model.ViewModels;

public partial class DeviceSettingsViewModel : BaseViewModel
{
    private readonly IntellidriveApiService _apiService;
    private Timer? _statusTimer;
    private CancellationTokenSource? _loadCancellationTokenSource;
    private static readonly SemaphoreSlim _parameterLoadSemaphore = new SemaphoreSlim(1, 1);
    private bool _parametersLoading = false;

    [ObservableProperty] private DeviceModel? selectedDevice;
    [ObservableProperty] private bool isParameterLoadComplete;
    [ObservableProperty] private string loadingMessage = "Parameter werden geladen...";
    [ObservableProperty] private bool isDoorOpen;
    [ObservableProperty] private bool isLocked;
    [ObservableProperty] private bool isSummerModeEnabled;
    [ObservableProperty] private bool isLoadingParameters; // <--- NEUß

    public DeviceSettingsViewModel(IntellidriveApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task InitializeAsync(bool loadInBackground = false)
    {
        if (SelectedDevice == null)
            throw new InvalidOperationException("SelectedDevice muss gesetzt werden.");

        if (SelectedDevice.Parameters == null)
            SelectedDevice.Parameters = new Dictionary<string, string>();

        if (SelectedDevice.Parameters.Count > 0)
        {
            IsParameterLoadComplete = true;
            return;
        }

        if (loadInBackground)
        {
            try
            {
                await UpdateDoorState(SelectedDevice);
                StartStatusUpdater();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Laden des Türstatus: {ex.Message}");
            }

            await LoadParametersAsync();
        }
        else
        {
            await LoadParametersAsync();
            StartStatusUpdater();
        }
    }

    public async Task LoadParametersAsync()
    {
        if (SelectedDevice == null) return;
        if (_parametersLoading) return;
        if (!await _parameterLoadSemaphore.WaitAsync(0)) return;

        try
        {
            _parametersLoading = true;
            IsLoadingParameters = true; // <--- NEU

            if (SelectedDevice.Parameters != null && SelectedDevice.Parameters.Count > 0 && IsParameterLoadComplete)
            {
                _parametersLoading = false;
                _parameterLoadSemaphore.Release();
                IsLoadingParameters = false; // <--- NEU
                return;
            }

            IsParameterLoadComplete = false;
            LoadingMessage = "Parameter werden im Hintergrund geladen...";

            await UpdateDoorState(SelectedDevice);

            if (IsDoorOpen)
            {
                LoadingMessage = "Tür ist geöffnet - einige Parameter könnten eingeschränkt sein";
            }

            _loadCancellationTokenSource?.Cancel();
            _loadCancellationTokenSource = new CancellationTokenSource();
            int timeoutMs = IsDoorOpen ? 3000 : 5000;
            _loadCancellationTokenSource.CancelAfter(timeoutMs);
            var token = _loadCancellationTokenSource.Token;

            Dictionary<string, string> parameters;

            try
            {
                var parametersTask = _apiService.GetParametersAsync(SelectedDevice);
                var completedTask = await Task.WhenAny(parametersTask, Task.Delay(timeoutMs, token));

                if (completedTask == parametersTask)
                {
                    parameters = await parametersTask;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Timeout beim Laden der Parameter ({timeoutMs}ms)");
                    parameters = new Dictionary<string, string>();
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Parameterladung abgebrochen");
                parameters = new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Parameter: {ex.Message}");
                parameters = new Dictionary<string, string>();
            }

            if (parameters.Count > 0)
            {
                SelectedDevice.Parameters = parameters;
                LoadingMessage = $"Parameter erfolgreich geladen ({parameters.Count})";
            }
            else if (SelectedDevice.Parameters == null || SelectedDevice.Parameters.Count == 0)
            {
                SelectedDevice.Parameters = parameters;
                LoadingMessage = "Keine Parameter geladen";
            }

            IsParameterLoadComplete = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Parameter: {ex.Message}");
            LoadingMessage = $"Fehler: {ex.Message}";
        }
        finally
        {
            _parametersLoading = false;
            _parameterLoadSemaphore.Release();
            IsLoadingParameters = false; // <--- NEU
        }
    }

    public void StartStatusUpdater()
    {
        if (_statusTimer != null) return;

        _statusTimer = new Timer(5000);
        _statusTimer.Elapsed += async (s, e) =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (SelectedDevice != null)
                {
                    await UpdateDoorState(SelectedDevice);
                }
            });
        };
        _statusTimer.AutoReset = true;
        _statusTimer.Start();
    }

    public void StopStatusUpdater()
    {
        _statusTimer?.Stop();
        _statusTimer?.Dispose();
        _statusTimer = null;

        _loadCancellationTokenSource?.Cancel();
    }

    public async Task<bool> CheckDoorClosedAsync()
    {
        try
        {
            var resultJson = await _apiService.GetDoorStateAsync(SelectedDevice);

            using var doc = JsonDocument.Parse(resultJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("Content", out var content) &&
                content.TryGetProperty("DOOR_STATE", out var doorStateProp))
            {
                string doorState = doorStateProp.GetString();
                return doorState?.ToLower() == "closed";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Türstatus prüfen: {ex.Message}");
        }

        return false;
    }

    public async Task UpdateDoorState(DeviceModel device)
    {
        if (device == null) return;

        try
        {
            var timeoutTokenSource = new CancellationTokenSource();
            timeoutTokenSource.CancelAfter(1500);

            var doorStateTask = _apiService.GetDoorStateAsync(device);
            var completedTask = await Task.WhenAny(doorStateTask, Task.Delay(1500, timeoutTokenSource.Token));

            if (completedTask == doorStateTask)
            {
                timeoutTokenSource.Cancel();
                var json = await doorStateTask;
                var result = JsonSerializer.Deserialize<DoorStateResponse>(json);

                if (result?.Content != null)
                {
                    MainThread.BeginInvokeOnMainThread(() => {
                        IsDoorOpen = !result.Content.DOOR_STATE.Equals("Closed", StringComparison.OrdinalIgnoreCase);
                        IsLocked = result.Content.DOOR_LOCK_STATE.Equals("Locked", StringComparison.OrdinalIgnoreCase);
                        IsSummerModeEnabled = result.Content.SUMMER_MODE.Equals("On", StringComparison.OrdinalIgnoreCase);
                    });
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Timeout beim Abrufen des Türstatus");
                MainThread.BeginInvokeOnMainThread(() => {
                    IsDoorOpen = false;
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Abrufen des Türstatus: {ex.Message}");
        }
    }
}