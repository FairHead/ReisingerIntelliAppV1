using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReisingerIntelliAppV1.Model.Models
{
    public class Building
    {
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<Floor> Floors { get; set; } = new();
    }

    public class Floor
    {
        public string Name { get; set; } = string.Empty;
        public string? PdfPath { get; set; } = string.Empty;
    }
}
