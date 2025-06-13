using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReisingerIntelliAppV1.Model.Models
{

    public class Floor
    {
        public string FloorName { get; set; } = string.Empty;
        public string? PdfPath { get; set; } = string.Empty;
        public string? PngPath { get; set; }  // Pfad zum konvertierten PNG
        // Collection für Geräte, die auf diesem Stockwerk platziert wurden
        public ObservableCollection<PlacedDeviceModel> PlacedDevices { get; set; } = new ObservableCollection<PlacedDeviceModel>();
    }
}
