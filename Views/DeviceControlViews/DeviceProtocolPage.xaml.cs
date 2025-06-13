using ReisingerIntelliAppV1.Base;
using ReisingerIntelliAppV1.Helpers;
using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Services;

namespace ReisingerIntelliAppV1.Views.DeviceControlViews;

[QueryProperty(nameof(Device), "Device")]
public partial class DeviceProtocolPage : BasePage, IDevicePage
{
    private new DeviceModel? _device;
    private new DeviceSettingsViewModel? _viewModel;
    private readonly IntellidriveApiService _apiService;

    public DeviceModel Device
    {
        get => _device!;
        set => _device = value;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine($"📋 Tab sichtbar: {GetType().Name}");
    }

    public DeviceProtocolPage(IntellidriveApiService apiService) : base(apiService)
    {
        _apiService = apiService;
        InitializeComponent();
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
    private void Parameterprotokoll(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Ereignisprotokoll(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Parametereinstellungen(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}