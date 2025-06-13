using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels; // for NetworkDataModel
using ReisingerIntelliAppV1.Services;          // for IntellidriveApiService, DeviceService
// for LocalNetworkScanViewModel
using ReisingerIntelliAppV1.Views;             // for LocalNetworkScanPage

namespace ReisingerIntelliAppV1.Views
{
    public partial class ChooseNetworkForLocalScan : ContentPage
    {
        private readonly ScanListViewModel _scanListViewModel;
        private readonly IntellidriveApiService _intellidriveApiService;
        private readonly DeviceService _deviceService;

        // Inject DeviceService so you can pass it along
        public ChooseNetworkForLocalScan(
            ScanListViewModel scanListViewModel,
            IntellidriveApiService intellidriveApiService,
            DeviceService deviceService)
        {
            InitializeComponent();

            _scanListViewModel = scanListViewModel;
            _intellidriveApiService = intellidriveApiService;
            _deviceService = deviceService;

            BindingContext = _scanListViewModel;
        }

        // your existing OnAppearing
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _scanListViewModel.LoadWifiNetworksAsync();
        }

        // Updated method with error handling
        public async void OnScanForDevicesClicked(object sender, EventArgs e)
        {
            if (sender is Button btn
                && btn.CommandParameter is NetworkDataModel network)
            {
                var result = await _scanListViewModel.TryPairDevice(network);

                try {
                    // Get a configured instance from the service provider
                    var localVm = App.Current?.Handler?.MauiContext?.Services
                        .GetRequiredService<LocalNetworkScanViewModel>();
                    
                    if (localVm == null) {
                        await DisplayAlert("Fehler", "Konnte ViewModel nicht initialisieren", "OK");
                        return;
                    }
                    
                    // Set the SSID name for this instance
                    localVm.SsidName = network.SsidName;
                    
                    // Create and navigate to the LocalNetworkScanPage
                    var scanPage = new LocalNetworkScanPage(localVm, _deviceService, _intellidriveApiService);
                    await Navigation.PushAsync(scanPage);
                }
                catch (Exception ex) {
                    await DisplayAlert("Fehler", $"Fehler beim Öffnen der Scan-Seite: {ex.Message}", "OK");
                }
            }
        }
    }
}
