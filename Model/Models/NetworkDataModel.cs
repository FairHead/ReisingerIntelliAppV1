using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ReisingerIntelliAppV1.Model.Models
{
    public partial class NetworkDataModel : ObservableObject
    {
        public string? Name { get; set; }
        public string? Ssid { get; set; }
        public string SsidName
        {
            get => !string.IsNullOrWhiteSpace(Ssid) ? Ssid : "Unknown"; 
            set => Ssid = value;
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string? DeviceId { get; set; }
        public int IpAddress { get; set; }
        public string? deviceId { get; set; }
        public string? BearerToken { get; set; }
        public string IpAddressString => new System.Net.IPAddress(BitConverter.GetBytes(IpAddress)).ToString();
        public string? GatewayAddress { get; set; }
        public object? Bssid { get; set; }
        public object? SignalStrength { get; set; }
        public object? SecurityType { get; set; }
        public bool IsConnected { get; set; }
        public bool IsAlreadySaved { get; set; }
        public string SerialNumber { get; set; }

        public string FirmwareVersion { get; set; }
        public bool LatestFirmware { get; set; }
        public string SoftwareVersion{ get; set; }
        public string ModuleType { get; set; }
        public string ModuleId { get; set; }


        //Mapper
        public static NetworkDataModel FromAuthResponseAndAuthData(AuthResponseDataModel responseDataModel, NetworkDataModel networkDataModel, AuthDataModel authDataModel, VersionResponseDataModel versionResponse)
        {
            return new NetworkDataModel
            {
                DeviceId = responseDataModel?.DeviceId,
                SerialNumber = responseDataModel?.Content?.DEVICE_ID ?? "Unknown",
                Username = authDataModel?.Username ?? "",
                Password = authDataModel?.Password ?? "",
                Ssid = networkDataModel?.Ssid,
                BearerToken = "", // kann später erweitert werden
                IsAlreadySaved = false,
                FirmwareVersion = versionResponse?.FirmwareVersion ?? "Unknown",
                SoftwareVersion = versionResponse?.Message ?? "Unknown",
                LatestFirmware = versionResponse?.LatestFirmware ?? false,
                // Ensure ModuleType and ModuleId are initialized to avoid NullReferenceException
                ModuleType = "Default",
                ModuleId = responseDataModel?.DeviceId ?? "Unknown"
            };
        }
    }
}
