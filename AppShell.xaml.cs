
using ReisingerIntelliAppV1.Views;
using ReisingerIntelliAppV1.Views.DeviceControlViews;
using ReisingerIntelliAppV1.Views.FloorManager;
using ReisingerIntelliAppV1.Views.ViewTestings;

namespace ReisingerIntelliAppV1;

public partial class AppShell 
{
    public AppShell()
    {
        InitializeComponent();

        //Main-Menu Pages
        Routing.RegisterRoute("ScanListPage", typeof(ScanListPage));
        Routing.RegisterRoute("SavedDeviceListPage", typeof(SavedDeviceListPage));


        //Devce Control Pages
        Routing.RegisterRoute("DeviceStatusPage", typeof(DeviceStatusPage));
        Routing.RegisterRoute("DeviceFunctionPage", typeof(DeviceFunctionPage));
        Routing.RegisterRoute("DeviceDoorFunctionPage", typeof(DeviceDoorFunctionPage));
        Routing.RegisterRoute("DeviceDistancesPage", typeof(DeviceDistancesPage));
        Routing.RegisterRoute("DeviceSpeedPage", typeof(DeviceSpeedPage));
        Routing.RegisterRoute("DeviceIOPage", typeof(DeviceIOPage));
        Routing.RegisterRoute("DeviceProtocolPage", typeof(DeviceProtocolPage));
        Routing.RegisterRoute("DeviceSettingsTabbedPage", typeof(DeviceSettingsTabbedPage));
        Routing.RegisterRoute("DeviceTimePage", typeof(DeviceTimePage));
        Routing.RegisterRoute("DeviceBasisPage", typeof(DeviceBasisPage));
        Routing.RegisterRoute("NewPage1", typeof(NewPage1));
        Routing.RegisterRoute("FloorPlanManagerPage", typeof(FloorPlanManagerPage));
        Routing.RegisterRoute("BuildingEditorPage", typeof(BuildingEditorPage));


        //Test-Pages
        Routing.RegisterRoute("DeviceInformationsPage", typeof(DeviceInformationsPage));
    }
}