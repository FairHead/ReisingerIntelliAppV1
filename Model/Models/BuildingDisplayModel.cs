using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReisingerIntelliAppV1.Model.Models
{
    public class BuildingDisplayModel
    {
        public Building Building { get; set; }
        public bool IsExpanded { get; set; } = false;
    }
}
