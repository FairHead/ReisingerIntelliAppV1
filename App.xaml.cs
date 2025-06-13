
using System.Diagnostics;
using ReisingerIntelliAppV1.Views;


namespace ReisingerIntelliAppV1
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public App(IServiceProvider serviceProvider)
        {
            

            InitializeComponent();

#if DEBUG
        
            //SecureStorage.Default.RemoveAll(); // Löscht ALLE gespeicherten Keys im SecureStorage
          
            //Preferences.Clear();               // Optional: auch normale App-Settings löschen
#endif

            ServiceProvider = serviceProvider;

            MainPage = new NavigationPage(ServiceProvider.GetRequiredService<IntellidriveAppMainPage>());



            //XamlDesignMode
            //MainPage = new NavigationPage(ServiceProvider.GetRequiredService<NewPage1>());
            //MainPage = new NavigationPage(ServiceProvider.GetRequiredService<DeviceBasisPage>());
            //MainPage = new NavigationPage(ServiceProvider.GetRequiredService<DeviceDistancesPage>());
            //MainPage = new NavigationPage(ServiceProvider.GetRequiredService<DeviceDoorFunctionPage>());
            //MainPage = new NavigationPage(ServiceProvider.GetRequiredService<DeviceIOPage>());
            //MainPage = new NavigationPage(ServiceProvider.GetRequiredService<DeviceProtocolPage>());
            //MainPage = new NavigationPage(ServiceProvider.GetRequiredService<DeviceSpeedPage>());
            //MainPage = new NavigationPage(ServiceProvider.GetRequiredService<DeviceStatusPage>());
            //MainPage = new NavigationPage(ServiceProvider.GetRequiredService<DeviceTimePage>());
            //MainPage = new NavigationPage(ServiceProvider.GetRequiredService<DeviceInformationsPage>());




        }



    }
}