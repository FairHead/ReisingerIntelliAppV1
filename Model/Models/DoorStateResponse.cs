using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReisingerIntelliAppV1.Model.Models
{
    public class DoorStateResponse
    {
        public string DeviceId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool LatestFirmware { get; set; }
        public string FirmwareVersion { get; set; }
        public DoorContent Content { get; set; }
    }

    public class DoorContent
    {
        public string DOOR_STATE { get; set; } = "";
        public string DOOR_LOCK_STATE { get; set; } = ""; // Neu
        public string SUMMER_MODE { get; set; } = "";     // Neu
    }
}
