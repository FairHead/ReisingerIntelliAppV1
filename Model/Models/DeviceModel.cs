using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace ReisingerIntelliAppV1.Model.Models;


public partial class DeviceModel : ObservableObject
{


    [PrimaryKey] [AutoIncrement] public int Id { get; set; }
    public string DeviceId { get; set; }
    public string Name { get; set; }
    public string Ssid { get; set; }
    public string BearerToken { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    [ObservableProperty]
    private bool isOnline;
    [ObservableProperty]
    private bool isDoorClosed;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public Dictionary<string, string> Parameters { get; set; } = new();
    public string Ip { get; set; } = string.Empty;
    public ConnectionType ConnectionType { get; set; }
    //HeaderData
    public string SerialNumber { get; set; }
    public string FirmwareVersion { get; set; }
    public bool LatestFirmware { get; set; }
    public string SoftwareVersion { get; set; }
    public string ModuleType { get; set; }
    public string ModuleId { get; set; }

    //Mapper
    public static DeviceModel FromNetworkData(NetworkDataModel network)
    {
        // Create the device with the basic properties
        var deviceModel = new DeviceModel
        {
            DeviceId = network.DeviceId,
            Name = network.Name,
            Ssid = network.Ssid,
            Username = network.Username,
            Password = network.Password,
            BearerToken = network.BearerToken,
            SerialNumber = network.SerialNumber,
            IsOnline = true,
            LastUpdated = DateTime.Now,
            Parameters = new Dictionary<string, string>(),
            FirmwareVersion = network.FirmwareVersion,
            LatestFirmware = network.LatestFirmware,
            SoftwareVersion = network.SoftwareVersion,
            ModuleType = "Default",
            ModuleId = network.DeviceId ?? "Unknown"
        };
        
        // Let the calling code set the ConnectionType and appropriate IP
        // based on the connection context, rather than hardcoding it here
        
        return deviceModel;
    }
}