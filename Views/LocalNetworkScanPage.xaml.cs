using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Views.PopUp;
using ReisingerIntelliAppV1.Services;

namespace ReisingerIntelliAppV1.Views
{
    public partial class LocalNetworkScanPage : ContentPage
    {
        readonly LocalNetworkScanViewModel _vm;
        readonly DeviceService _deviceService;
        readonly IntellidriveApiService _api;

        public LocalNetworkScanPage(LocalNetworkScanViewModel vm, DeviceService deviceService, IntellidriveApiService api)
        {
            InitializeComponent();

            _vm = vm;
            _deviceService = deviceService;
            _api = api;

            BindingContext = _vm;
        }

       

        public async void OnShowIpRangePopupClicked(object sender, EventArgs e)
        {
            var popup = new IpRangePopup();
            var raw = await this.ShowPopupAsync(popup);
            if (raw is IpRangePopupResult result)
            {
                _vm.IpRange = $"{result.StartIp}-{result.EndIp}";
                await _vm.LoadLocalNetworkDevicesAsync();
            }
        }

        private async void OnSaveLocalDeviceClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is LocalNetworkDeviceModel device)
            {
                await _vm.SaveDeviceAsync(device);
            }
        }

    }
}
