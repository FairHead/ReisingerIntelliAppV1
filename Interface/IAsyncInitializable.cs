using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReisingerIntelliAppV1.Interface
{
    internal interface IAsyncInitializable
    {
        Task InitializeAsync();
    }
}
