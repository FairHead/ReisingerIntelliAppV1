using ReisingerIntelliAppV1.Base;
using ReisingerIntelliAppV1.Helpers;
using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Services;

namespace ReisingerIntelliAppV1.Views.DeviceControlViews;

[QueryProperty(nameof(Device), "Device")]
public partial class DeviceBasisPage : BasePage, IDevicePage
{
    private new DeviceModel? _device;
    private new DeviceSettingsViewModel? _viewModel;
    private readonly IntellidriveApiService _apiService;
    private bool _isLocalLoading = false; // Lokaler Status für Ladevorgang
    private bool _isInitialized = false;
    private CancellationTokenSource? _loadCts;
    
    public DeviceModel Device
    {
        get => _device!;
        set => _device = value;
    }

    public DeviceBasisPage(IntellidriveApiService apiService) : base(apiService)
    {
        _apiService = apiService;
        InitializeComponent();
    }

    public override void InitializeWith(DeviceSettingsViewModel viewModel, DeviceModel device)
    {
        // Verhindere Mehrfach-Initialisierung
        if (_isInitialized) return;
        
        _device = device;
        _viewModel = viewModel;
        
        // Nur die Basis-Properties initialisieren, keine blockierenden Aufrufe
        base.InitializeWith(viewModel, device);
        
        // Optimierung: Sofort leere Parameter setzen, falls noch keine vorhanden
        if (_device.Parameters == null)
        {
            _device.Parameters = new Dictionary<string, string>();
        }
        
        // Initialisiere UI-Elemente ohne Netzwerkanfragen
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
        
        // Türstatus-Anzeige konfigurieren, falls Türstatus bereits bekannt ist
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

    public async Task RefreshAsync()
    {
        if (_viewModel == null || _device == null) return;

        try
        {
            // WICHTIG: Kein IsBusy setzen, um UI nicht zu blockieren
            // Stattdessen lokale Flags verwenden
            
            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(1500); // 1,5 Sekunden Timeout
                
                // Vermeiden von UI-Blockierung durch BeginInvokeOnMainThread
                MainThread.BeginInvokeOnMainThread(() => 
                {
                    try
                    {
                        var layout = GetContentLayout();
                        if (layout == null) return;
                        
                        // Überprüfen, ob die Tür geöffnet ist - kann Parameter beeinflussen
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
                            // Verwenden der existierenden Parameter, wenn sie bereits geladen sind
                            if (_device.Parameters != null && _device.Parameters.Count > 0)
                            {
                                // Parameter auf UI-Elemente anwenden
                                ParameterHelper.AssignParametersToEntries(_device.Parameters, layout);
                                
                                // Fehlermeldungen ausblenden
                                if (layout.FindByName<Label>("ErrorMessage") is Label errorLabel)
                                {
                                    errorLabel.IsVisible = false;
                                }
                                
                                if (layout.FindByName<Button>("RetryLoadButton") is Button retryButton)
                                {
                                    retryButton.IsVisible = false;
                                }
                            }
                            else if (!_isLocalLoading)
                            {
                                // Parameter sollten geladen sein, sind es aber nicht - Fehler anzeigen
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
                        }
                    }
                    catch (Exception ex)
                    {
                        // Fehler loggen, aber nicht blockieren
                        System.Diagnostics.Debug.WriteLine($"Fehler beim Aktualisieren der UI: {ex.Message}");
                    }
                });
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("UI-Aktualisierung wegen Timeout abgebrochen");
            }
            catch (Exception ex)
            {
                // Log the error but don't rethrow
                System.Diagnostics.Debug.WriteLine($"Error applying parameters to UI: {ex.Message}");
                
                // Show a non-blocking indicator that parameter loading failed
                MainThread.BeginInvokeOnMainThread(() => {
                    if (Content is Layout layout && layout.FindByName<Label>("ErrorMessage") is Label errorLabel)
                    {
                        errorLabel.Text = "Parameter konnten nicht geladen werden";
                        errorLabel.IsVisible = true;
                    }
                });
            }
        }
        finally
        {
            // WICHTIG: Kein IsBusy zurücksetzen, da wir es nicht gesetzt haben
        }
    }
    
    private async void OnClick(object sender, EventArgs e)
    {
        // Verhindere mehrfache gleichzeitige Klicks
        if (_isLocalLoading) return;
        
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
            // Verhindere doppelte Ladevorgänge
            if (_isLocalLoading) return;
            
            // Lokalen Ladestatus setzen
            _isLocalLoading = true;
            
            // Abbrechen vorheriger Operationen
            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();
            
            // Update UI mit Ladehinweis (ohne IsBusy zu setzen)
            _viewModel.LoadingMessage = "Parameter werden neu geladen...";
            
            // Hide the error message and retry button
            if (ErrorMessage != null)
                ErrorMessage.IsVisible = false;
            
            if (RetryLoadButton != null)
                RetryLoadButton.IsVisible = false;

            // Optimierung: Lade Parameter asynchron ohne UI zu blockieren
            await Task.Run(async () => 
            {
                try 
                {
                    // Try to load parameters again
                    await _viewModel.LoadParametersAsync();
                
                    // Wenn erfolgreich, UI aktualisieren auf dem UI-Thread
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
                    
                    // Fehler auf UI-Thread anzeigen
                    await MainThread.InvokeOnMainThreadAsync(() => 
                    {
                        if (ErrorMessage != null)
                        {
                            ErrorMessage.Text = $"Fehler: {ex.Message}";
                            ErrorMessage.IsVisible = true;
                        }
                        
                        if (RetryLoadButton != null)
                        {
                            RetryLoadButton.IsVisible = true;
                        }
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
            System.Diagnostics.Debug.WriteLine($"Error retrying parameter load: {ex.Message}");
            
            // Show error message
            if (ErrorMessage != null)
            {
                ErrorMessage.Text = $"Failed to load parameters: {ex.Message}";
                ErrorMessage.IsVisible = true;
            }
            
            // Keep retry button visible
            if (RetryLoadButton != null)
                RetryLoadButton.IsVisible = true;
        }
        finally
        {
            _isLocalLoading = false;
        }
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Laufende Operationen abbrechen
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;
        
        // Flags zurücksetzen
        _isLocalLoading = false;
    }
}