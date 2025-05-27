using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ReisingerIntelliAppV1.Model.Models
{
    public partial class VersionResponseDataModel : ObservableObject
    {
        public string DeviceId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool LatestFirmware { get; set; }
        public string FirmwareVersion { get; set; }
        public AuthContent Content {get; set; }
    public class AuthContent
    {
            [JsonPropertyName("DEVICE_SERIALNO")]
            public string DeviceSerialNo{ get; set; }
    }
    

    }
}
