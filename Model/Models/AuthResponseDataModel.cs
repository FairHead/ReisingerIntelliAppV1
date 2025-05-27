using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReisingerIntelliAppV1.Model.Models
{
    public class AuthResponseDataModel
    {
        public string? DeviceId { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public bool LatestFirmware { get; set; }
        public AuthContent? Content { get; set; }
    }
    public class AuthContent
    {
        [System.Text.Json.Serialization.JsonPropertyName("DEVICE_ID")]
        public string? DEVICE_ID { get; set; }
    }

}
