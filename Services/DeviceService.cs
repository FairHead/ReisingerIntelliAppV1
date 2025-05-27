using System.Text.Json;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;

namespace ReisingerIntelliAppV1.Services;

public class DeviceService
{
    private const string DevicesKey = "Devices";
    private readonly IntellidriveApiService _apiService;

    // ✅ Konstruktor mit Dependency Injection
    public DeviceService(IntellidriveApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task SaveDeviceListToSecureStore(List<DeviceModel> devices)
    {
        try
        {
            var json = JsonSerializer.Serialize(devices);
            await SecureStorage.SetAsync(DevicesKey, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Speichern der Geräte: {ex.Message}");
        }
    }

    public async Task<(bool IsSuccessful, bool IsEmpty, List<DeviceModel> Devices)> LoadDeviceList()
    {
        try
        {
            var json = await SecureStorage.GetAsync(DevicesKey);

            if (string.IsNullOrEmpty(json))
                return (true, true, new List<DeviceModel>());

            var devices = JsonSerializer.Deserialize<List<DeviceModel>>(json);
            return (true, devices == null || !devices.Any(), devices ?? new List<DeviceModel>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Geräte: {ex.Message}");
            return (false, false, new List<DeviceModel>());
        }
    }

    public async Task<bool> DeviceExists(string deviceId)
    {
        var devices = (await LoadDeviceList()).Devices;
        return devices.Any(d => d.DeviceId == deviceId);
    }

    public async Task<bool> DeviceExistsBySsidAsync(string ssid)
    {
        var allDevices = (await LoadDeviceList()).Devices;
        return allDevices.Any(d => d.Ssid.Equals(ssid, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<DeviceModel>> AddDeviceAndReturnUpdatedList(DeviceModel device)
    {
        if (await DeviceExists(device.DeviceId))
        {
            Console.WriteLine($"Gerät mit der ID {device.DeviceId} existiert bereits und wird nicht erneut gespeichert.");
            return (await LoadDeviceList()).Devices;
        }

        var devices = (await LoadDeviceList()).Devices;
        devices.Add(device);
        await SaveDeviceListToSecureStore(devices);

        return devices;
    }

    public async Task DeleteDeviceAsync(DeviceModel device)
    {
        var devices = (await LoadDeviceList()).Devices;
        var deviceToRemove = devices.FirstOrDefault(d => d.DeviceId == device.DeviceId);
        
        if (deviceToRemove != null)
        {
            devices.Remove(deviceToRemove);
            await SaveDeviceListToSecureStore(devices);
        }
    }

    public async Task SaveDeviceAsync(DeviceModel device)
    {
        await AddDeviceAndReturnUpdatedList(device);
    }

    public static void ClearSecureStorage()
    {
        try
        {
            SecureStorage.RemoveAll();
            Console.WriteLine("SecureStorage wurde erfolgreich geleert.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Leeren des SecureStorage: {ex.Message}");
        }
    }
}
