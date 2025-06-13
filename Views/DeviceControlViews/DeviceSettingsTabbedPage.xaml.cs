using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using Microsoft.Maui.Controls;

namespace ReisingerIntelliAppV1.Views.DeviceControlViews;

public partial class DeviceSettingsTabbedPage : TabbedPage
{
    private readonly IList<IDevicePage> _devicePages = new List<IDevicePage>();
    private readonly DeviceSettingsViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    private DeviceModel? _device;
    private bool _isInitialized;
    private bool _isLoadingParameters;
    private static readonly SemaphoreSlim _loadSemaphore = new(1, 1);
    private bool _isFirstAppearance = true;
    private Page? _lastTab;

    public DeviceSettingsTabbedPage(DeviceSettingsViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        BindingContext = _viewModel;
    }

    public async Task InitializeWithAsync(DeviceModel device)
    {
        if (_isInitialized && _device?.DeviceId == device.DeviceId) return;

        _device = device;
        _viewModel.SelectedDevice = device;

        BuildTabStructure();
        _isInitialized = true;
        _isFirstAppearance = true;

        await Task.CompletedTask;
    }

    private void BuildTabStructure()
    {
        void AddTab<TPage>(string icon) where TPage : Page, IDevicePage
        {
            var page = _serviceProvider.GetRequiredService<TPage>();
            page.Title = ""; // Set an empty title to remove text labels under icons
            page.IconImageSource = icon;
            page.InitializeWith(_viewModel, _device!);

            _devicePages.Add(page);
            Children.Add((Page)page);
        }

        AddTab<DeviceTimePage>("icon_clock.png");
        AddTab<DeviceDistancesPage>("icon_ruler.png");
        AddTab<DeviceSpeedPage>("icon_speed.png");
        AddTab<DeviceIOPage>("icon_io.png");
        AddTab<DeviceBasisPage>("icon_settings.png");
        AddTab<DeviceFunctionPage>("icon_function.png");
        AddTab<DeviceDoorFunctionPage>("icon_door.png");
        AddTab<DeviceStatusPage>("icon_status.png");
        AddTab<DeviceProtocolPage>("icon_protocol.png");

        // Wichtig: ersten Tab explizit setzen, sonst kann MAUI inkorrekt initialisieren
        Device.BeginInvokeOnMainThread(() =>
        {
            CurrentPage = Children[0];
            _lastTab = CurrentPage;
        });
    }

    protected override async void OnCurrentPageChanged()
    {
        base.OnCurrentPageChanged();

        if (CurrentPage == _lastTab) return;

        var newTabName = CurrentPage?.GetType().Name ?? "Unbekannt";
        var oldTabName = _lastTab?.GetType().Name ?? "Unbekannt";
        System.Diagnostics.Debug.WriteLine($"➡️ Tab-Wechsel erkannt: {oldTabName} → {newTabName}");

        _lastTab = CurrentPage;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Task.Delay(200); // Verzögerung für Thread-Synchronität

            if (CurrentPage is IDevicePage page && _device != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 Starte Refresh für Tab: {newTabName}");
                    await page.RefreshAsync();
                    System.Diagnostics.Debug.WriteLine($"✅ Refresh abgeschlossen für {newTabName}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Fehler beim Refresh für {newTabName}: {ex.Message}");
                }
            }
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_device == null) return;

        _viewModel.StartStatusUpdater();

        if (_isFirstAppearance && !_isLoadingParameters)
        {
            _isFirstAppearance = false;

            _ = Task.Run(async () =>
            {
                await Task.Delay(300); // UI stabilisieren
                if (!await _viewModel.CheckDoorClosedAsync())
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Shell.Current.DisplayAlert("Türstatus", "Tür ist geöffnet, Parameter können nicht geladen werden.", "OK");
                    });
                    return;
                }

                await LoadParametersAsync();
            });
        }
    }

    public async Task LoadParametersAsync()
    {
        if (_device == null) return;

        if (!await _loadSemaphore.WaitAsync(0)) return;

        try
        {
            if (_isLoadingParameters)
            {
                _loadSemaphore.Release();
                return;
            }

            _isLoadingParameters = true;

            if (_device.Parameters?.Count > 0)
            {
                _viewModel.IsParameterLoadComplete = true;
                await RefreshAllTabsAsync();
                return;
            }

            await _viewModel.UpdateDoorState(_device);
            await _viewModel.InitializeAsync(loadInBackground: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Parameterladen: {ex.Message}");
        }
        finally
        {
            _isLoadingParameters = false;
            _loadSemaphore.Release();
        }
    }

    private async Task RefreshAllTabsAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            foreach (var page in _devicePages)
            {
                try { await page.RefreshAsync(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Fehler beim Aktualisieren von {page.GetType().Name}: {ex.Message}");
                }
            }
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopStatusUpdater();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Optional, wenn Parameterstatus Änderungen überwacht werden sollen
    }
}
