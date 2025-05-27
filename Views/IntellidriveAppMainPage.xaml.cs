using ReisingerIntelliAppV1.Views.FloorManager;
using ReisingerIntelliAppV1.Views;

namespace ReisingerIntelliAppV1.Views;


public partial class IntellidriveAppMainPage : ContentPage
{
    public IntellidriveAppMainPage()
    {
        InitializeComponent();
    }

    public async void GoToScanListPage(object sender, EventArgs e)
    {
        var scanPage = App.ServiceProvider.GetRequiredService<ScanListPage>();
        await Navigation.PushAsync(scanPage);
    }

    public async void GoToSavedDeviceListPage(object sender, EventArgs e)
    {
        var savedPage = App.ServiceProvider.GetRequiredService<SavedDeviceListPage>();
        await Navigation.PushAsync(savedPage);
    }

    public async void GoToLocalNetworkScanPage(object sender, EventArgs e)
    {
        var page = App.ServiceProvider.GetRequiredService<ChooseNetworkForLocalScan>();
        await Navigation.PushAsync(page);
    }

    public async void GoToLocalSavedNetworkScanPage(object sender, EventArgs e)
    {
        var page = App.ServiceProvider.GetRequiredService<LocalSavedDeviceListPage>();
        await Navigation.PushAsync(page);
    }
    public async void GoToLocalFloorPlanManagerPage(object sender, EventArgs e)
    {
        var page = App.ServiceProvider.GetRequiredService<FloorPlanManagerPage>();
        await Navigation.PushAsync(page);
    }


}
