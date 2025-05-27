using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Services;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Helpers;

namespace ReisingerIntelliAppV1.Base;
public abstract class BasePage : ContentPage
{
    private bool _isNavigating;
    private bool _initialized;
    protected DeviceSettingsViewModel? _viewModel;
    protected DeviceModel? _device;
    private readonly IntellidriveApiService? _apiService;

    protected BasePage(IntellidriveApiService apiService)
    {
        _apiService = apiService;
    }

    protected Layout GetContentLayout()
    {
        var layout = Content as Layout;
        if (layout == null)
        {
            throw new InvalidOperationException("Page content must be a Layout");
        }
        return layout;
    }    public virtual void InitializeWith(DeviceSettingsViewModel viewModel, DeviceModel device)
    {
        _viewModel = viewModel;
        _device = device;
        _viewModel.SelectedDevice = _device;
        BindingContext = _viewModel;
        
        // Set up a property changed handler to update UI when parameters are loaded
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }
    
    private async void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Check if the parameters have been loaded
        if (e.PropertyName == nameof(DeviceSettingsViewModel.IsParameterLoadComplete) && 
            _viewModel != null && _viewModel.IsParameterLoadComplete)
        {
            // Parameters are now loaded, apply them to the UI
            await ApplyParametersToUI();
        }
    }    // Helper method to apply parameters to UI elements
    protected async Task ApplyParametersToUI()
    {
        if (_device != null && _viewModel != null)
        {
            try
            {
                // Only try to apply parameters if they are already loaded
                // We don't want to trigger additional API calls here
                if (_device.Parameters != null && _device.Parameters.Count > 0)
                {
                    // Apply parameters to UI elements
                    var layout = GetContentLayout();
                    if (layout != null)
                    {
                        ParameterHelper.AssignParametersToEntries(_device.Parameters, layout);
                    }
                    
                    // Hide any error messages that might be visible
                    var errorLabel = layout?.FindByName<Label>("ErrorMessage");
                    if (errorLabel != null)
                    {
                        errorLabel.IsVisible = false;
                    }
                    
                    var retryButton = layout?.FindByName<Button>("RetryLoadButton");
                    if (retryButton != null)
                    {
                        retryButton.IsVisible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying parameters to UI: {ex.Message}");
                
                // Show error message if present in the layout
                var layout = GetContentLayout();
                var errorLabel = layout?.FindByName<Label>("ErrorMessage");
                if (errorLabel != null)
                {
                    errorLabel.Text = $"Error loading parameters: {ex.Message}";
                    errorLabel.IsVisible = true;
                }
                
                var retryButton = layout?.FindByName<Button>("RetryLoadButton");
                if (retryButton != null)
                {
                    retryButton.IsVisible = true;
                }
            }
        }
    }

    // This method was causing compilation issues
    // We'll use the ApplyParametersToUI method directly in the InitializeWith method instead
    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();
    //
    //    if (!_initialized && _device != null && _viewModel != null)
    //    {
    //        _initialized = true;
    //        await ApplyParametersToUI();
    //    }
    //}

    protected async Task NavigateAsync(object sender)
    {
        if (_isNavigating)
        {
            await DisplayAlert(
                "Navigation läuft",
                "Die Navigation ist noch nicht abgeschlossen. Bitte warten.",
                "OK");
            return;
        }

        try
        {
            _isNavigating = true;

            if (sender is Button button && button.CommandParameter is string route)
            {
                await PageNavigator.GoToAsync(route);
            }
            else
            {
                await DisplayAlert(
                    "Navigation Fehler",
                    "Ungültiger Button oder fehlender CommandParameter.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Fehler",
                $"Ein Fehler ist während der Navigation aufgetreten: {ex.Message}",
                "OK");
        }
        finally
        {
            _isNavigating = false;
        }
    }
}