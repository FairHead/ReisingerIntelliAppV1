using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReisingerIntelliAppV1.Model.Models
{
    public class LocalNetworkDeviceModel : ObservableObject
    {
        public string IpAddress { get; set; }
        public string DeviceId { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareVersion { get; set; }
        public string SoftwareVersion { get; set; }
        public bool LatestFirmware { get; set; }
        public bool IsAlreadySaved { get; set; }
        public bool IsNotAlreadySaved => !IsAlreadySaved;

    }

}
