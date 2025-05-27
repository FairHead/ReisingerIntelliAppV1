using ReisingerIntelliAppV1.Model.Models;
using Syncfusion.Maui.Core.Carousel;

namespace ReisingerIntelliAppV1.Views.ViewTestings;

public partial class DeviceInformationsPage : ContentPage
{
    private readonly DeviceInformationsViewModel _viewModel;
    public DeviceInformationsPage(DeviceInformationsViewModel viewModel)
    {
		InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public async Task InitializeWithAsync(DeviceModel device)
    {
        await _viewModel.InitializeAsync(device);
    }

    public async void GoToDeviceInformationsPage(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var device = button?.CommandParameter as DeviceModel;
        if (device == null) return;

        var infoPage = App.ServiceProvider.GetRequiredService<DeviceInformationsPage>();
        await infoPage.InitializeWithAsync(device);
        await Navigation.PushAsync(infoPage);
    }

}