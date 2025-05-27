using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ReisingerIntelliAppV1.Model.Models
{
    public partial class AuthDataModel : ObservableObject
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
