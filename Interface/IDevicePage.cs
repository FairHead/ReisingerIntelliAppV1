using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;

namespace ReisingerIntelliAppV1.Interface;

public interface IDevicePage
{
    void InitializeWith(DeviceSettingsViewModel viewModel, DeviceModel device);
    Task RefreshAsync();

}
