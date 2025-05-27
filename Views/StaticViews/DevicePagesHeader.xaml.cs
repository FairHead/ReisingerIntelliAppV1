using ReisingerIntelliAppV1.Model.Models;

namespace ReisingerIntelliAppV1.Views.StaticViews;

public partial class DevicePagesHeader : ContentView
{
	public DevicePagesHeader()
	{
		InitializeComponent();
	}
    public static readonly BindableProperty DeviceProperty =
        BindableProperty.Create(nameof(Device), typeof(DeviceModel), typeof(DevicePagesHeader), propertyChanged: OnDeviceChanged);

    public DeviceModel Device
    {
        get => (DeviceModel)GetValue(DeviceProperty);
        set => SetValue(DeviceProperty, value);
    }

    private static void OnDeviceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DevicePagesHeader control && newValue is DeviceModel device)
        {
            control.BindingContext = new { Device = device };
        }
    }
}