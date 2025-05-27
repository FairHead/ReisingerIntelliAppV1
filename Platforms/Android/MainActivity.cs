using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Plugin.MauiWifiManager;

namespace ReisingerIntelliAppV1
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                               ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            WifiNetworkService.Init(this);
            CheckPermissions();
        }

        private void CheckPermissions()
        {
            var permissions = new[]
            {
                Android.Manifest.Permission.AccessFineLocation,
                Android.Manifest.Permission.AccessCoarseLocation,
                Android.Manifest.Permission.AccessWifiState,
                Android.Manifest.Permission.ChangeWifiState
            };

            foreach (var permission in permissions)
            {
                if (ContextCompat.CheckSelfPermission(this, permission) != Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, permissions, 0);
                    return;
                }
            }
        }
    }
}
