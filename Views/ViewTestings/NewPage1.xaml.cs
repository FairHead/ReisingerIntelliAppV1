using ReisingerIntelliAppV1.Views.DeviceControlViews;

namespace ReisingerIntelliAppV1.Views.ViewTestings;

public partial class NewPage1 : ContentPage
{
	public NewPage1()
	{
		InitializeComponent();
	}


    private async void Basis(object sender, EventArgs e)
    {
        var basis = App.ServiceProvider.GetRequiredService<DeviceBasisPage>();
        await Navigation.PushAsync(basis);
    }



    private async void Weiten(object sender, EventArgs e)
    {
        var basis = App.ServiceProvider.GetRequiredService<DeviceDistancesPage>();
        await Navigation.PushAsync(basis);
    }

    private async void Türfunktionen(object sender, EventArgs e)
    {
        var basis = App.ServiceProvider.GetRequiredService<DeviceDoorFunctionPage>();
        await Navigation.PushAsync(basis);
    }

    private async void Funktionen(object sender, EventArgs e)
    {
        var basis = App.ServiceProvider.GetRequiredService<DeviceFunctionPage>();
        await Navigation.PushAsync(basis);
    }
    private async void IO(object sender, EventArgs e)
    {
        var basis = App.ServiceProvider.GetRequiredService<DeviceIOPage>();
        await Navigation.PushAsync(basis);
    }

    private async void Protokoll(object sender, EventArgs e)
    {
        var basis = App.ServiceProvider.GetRequiredService<DeviceProtocolPage>();
        await Navigation.PushAsync(basis);
    }

    private async void Speed(object sender, EventArgs e)
    {
        var basis = App.ServiceProvider.GetRequiredService<DeviceSpeedPage>();
        await Navigation.PushAsync(basis);
    }

    private async void Status(object sender, EventArgs e)
    {
        var basis = App.ServiceProvider.GetRequiredService<DeviceStatusPage>();
        await Navigation.PushAsync(basis);
    }

    private async void Zeiten(object sender, EventArgs e)
    {
        var basis = App.ServiceProvider.GetRequiredService<DeviceTimePage>();
        await Navigation.PushAsync(basis);
    }


}