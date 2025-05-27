using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace ReisingerIntelliAppV1.Services
{
    public static class PageNavigator
    {
        public static async Task GoToAsync(string route, Dictionary<string, object> parameters = null)
        {
            if (parameters != null)
                await Shell.Current.GoToAsync(route, parameters);
            else
                await Shell.Current.GoToAsync(route);
        }

        public static async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}