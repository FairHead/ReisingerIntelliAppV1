using ReisingerIntelliAppV1.Base;
using ReisingerIntelliAppV1.Helpers;
using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;

namespace ReisingerIntelliAppV1.Views.DeviceControlViews;

[QueryProperty(nameof(Device), "Device")]
public partial class DeviceIOPage : BasePage, IDevicePage
{
    private new DeviceModel? _device;
    private new DeviceSettingsViewModel? _viewModel;
    private readonly IntellidriveApiService _apiService;

    public DeviceModel Device
    {
        get => _device!;
        set => _device = value;
    }

    public DeviceIOPage(IntellidriveApiService apiService) : base(apiService)
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


    public async Task RefreshAsync()
    {
        if (_viewModel != null && _device != null && _apiService != null)
        {
            try
            {
                _viewModel.IsBusy = true;
                _device.Parameters = await _apiService.GetParametersAsync(_device);
                var layout = GetContentLayout();
                if (layout != null)
                {
                    ParameterHelper.AssignParametersToEntries(_device.Parameters, layout);
                }
            }
            finally
            {
                _viewModel.IsBusy = false;
            }
        }
    }

    private async void OnClick(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await NavigateAsync(button);
        }
    }
}