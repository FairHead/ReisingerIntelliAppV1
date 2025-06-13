using ReisingerIntelliAppV1.Base;
using ReisingerIntelliAppV1.Helpers;
using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Services;

namespace ReisingerIntelliAppV1.Views.DeviceControlViews;

[QueryProperty(nameof(Device), "Device")]
public partial class DeviceTimePage : BasePage, IDevicePage
{
    private new DeviceModel? _device;
    private new DeviceSettingsViewModel? _viewModel;
    private readonly IntellidriveApiService _apiService;
    private bool _isRefreshing = false;
    private bool _isInitialized = false;
    private bool _isLocalLoading = false;
    private CancellationTokenSource? _loadCts;

    public DeviceModel Device
    {
        get => _device!;
        set => _device = value;
    }

    public DeviceTimePage(IntellidriveApiService apiService) : base(apiService)
    {
        _apiService = apiService;
        InitializeComponent();
    }

    public override void InitializeWith(DeviceSettingsViewModel viewModel, DeviceModel device)
    {
        if (_isInitialized) return;

        _device = device;
        _viewModel = viewModel;

        base.InitializeWith(viewModel, device);

        if (_device.Parameters == null)
            _device.Parameters = new Dictionary<string, string>();

        InitializeUIElements();
        _isInitialized = true;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine($"📋 Tab sichtbar: {GetType().Name}");
    }

    private void InitializeUIElements()
    {
        if (_viewModel == null || _device == null) return;

        if (_viewModel.IsDoorOpen && ErrorMessage != null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ErrorMessage.Text = "Tür ist geöffnet - Parameter können eingeschränkt sein";
                ErrorMessage.TextColor = Colors.Orange;
                ErrorMessage.IsVisible = true;

                if (RetryLoadButton != null)
                {
                    RetryLoadButton.Text = "Erneut versuchen wenn Tür geschlossen";
                    RetryLoadButton.IsVisible = true;
                }
            });
        }
    }

    private async void OnClick(object sender, EventArgs e)
    {
        if (_viewModel?.IsBusy == true || _isLocalLoading) return;

        if (sender is Button button)
        {
            await NavigateAsync(button);
        }
    }

    private async void OnRetryLoadClicked(object sender, EventArgs e)
    {
        if (_viewModel == null || _device == null) return;

        try
        {
            if (_isLocalLoading) return;
            _isLocalLoading = true;

            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();

            _viewModel.LoadingMessage = "Parameter werden neu geladen...";

            if (ErrorMessage != null) ErrorMessage.IsVisible = false;
            if (RetryLoadButton != null) RetryLoadButton.IsVisible = false;

            await Task.Run(async () =>
            {
                try
                {
                    await _viewModel.LoadParametersAsync();

                    if (_viewModel.IsParameterLoadComplete)
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await ApplyParametersToUI();
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Fehler beim Laden im Hintergrund: {ex.Message}");

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        if (ErrorMessage != null)
                        {
                            ErrorMessage.Text = $"Fehler: {ex.Message}";
                            ErrorMessage.IsVisible = true;
                        }

                        if (RetryLoadButton != null)
                            RetryLoadButton.IsVisible = true;
                    });
                }
            }, _loadCts.Token);
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Parameterladung abgebrochen");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim erneuten Laden der Parameter: {ex.Message}");

            if (ErrorMessage != null)
            {
                ErrorMessage.Text = $"Parameter konnten nicht geladen werden: {ex.Message}";
                ErrorMessage.IsVisible = true;
            }

            if (RetryLoadButton != null)
                RetryLoadButton.IsVisible = true;
        }
        finally
        {
            _isLocalLoading = false;
        }
    }

    public Task RefreshAsync()
    {
        if (_viewModel == null || _device == null) return Task.CompletedTask;
        if (_isRefreshing) return Task.CompletedTask;

        _isRefreshing = true;

        try
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1500);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var layout = GetContentLayout();
                    if (layout == null) return;

                    if (_viewModel.IsDoorOpen)
                    {
                        if (layout.FindByName<Label>("ErrorMessage") is Label errorLabel)
                        {
                            errorLabel.Text = "Tür ist geöffnet - Parameter können eingeschränkt sein";
                            errorLabel.TextColor = Colors.Orange;
                            errorLabel.IsVisible = true;
                        }

                        if (layout.FindByName<Button>("RetryLoadButton") is Button retryButton)
                        {
                            retryButton.Text = "Erneut versuchen wenn Tür geschlossen";
                            retryButton.IsVisible = true;
                        }
                    }
                    else
                    {
                        if (_device.Parameters != null && _device.Parameters.Count > 0)
                        {
                            ParameterHelper.AssignParametersToEntries(_device.Parameters, layout);

                            if (layout.FindByName<Label>("ErrorMessage") is Label errorLabel)
                                errorLabel.IsVisible = false;

                            if (layout.FindByName<Button>("RetryLoadButton") is Button retryButton)
                                retryButton.IsVisible = false;
                        }
                        else if (!_isLocalLoading)
                        {
                            if (layout.FindByName<Label>("ErrorMessage") is Label errorLabel)
                            {
                                errorLabel.Text = "Parameter konnten nicht geladen werden";
                                errorLabel.TextColor = Colors.Red;
                                errorLabel.IsVisible = true;
                            }

                            if (layout.FindByName<Button>("RetryLoadButton") is Button retryButton)
                            {
                                retryButton.Text = "Parameter neu laden";
                                retryButton.IsVisible = true;
                            }
                        }
                        else
                        {
                            if (layout.FindByName<Label>("ErrorMessage") is Label errorLabel)
                            {
                                errorLabel.Text = "Parameter werden geladen...";
                                errorLabel.TextColor = Colors.Blue;
                                errorLabel.IsVisible = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Fehler beim Aktualisieren der UI: {ex.Message}");
                }
                finally
                {
                    _isRefreshing = false;
                }
            });
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("UI-Aktualisierung wegen Timeout abgebrochen");
            _isRefreshing = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler in RefreshAsync: {ex.Message}");
            _isRefreshing = false;
        }

        return Task.CompletedTask;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;

        _isRefreshing = false;
        _isLocalLoading = false;
    }
}
