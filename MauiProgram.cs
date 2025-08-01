﻿using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ReisingerIntelliAppV1.Interface;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
#if ANDROID
using ReisingerIntelliAppV1.Platforms.Android;
#endif
using ReisingerIntelliAppV1.Services;
using ReisingerIntelliAppV1.Views;
using ReisingerIntelliAppV1.Views.DeviceControlViews;
using ReisingerIntelliAppV1.Views.FloorManager;
using ReisingerIntelliAppV1.Views.PopUp;
using ReisingerIntelliAppV1.Views.ViewTestings;

namespace ReisingerIntelliAppV1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp(sp => new App(sp)) // nur diese Zeile!
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        builder.UseMauiCommunityToolkit(options =>
        {
            options.SetShouldSuppressExceptionsInBehaviors(false);
        });

        // Services
        builder.Services.AddHttpClient("IntellidriveAPI", client =>
        {
            // Don't set BaseAddress here since it will be set dynamically for each device
            // client.BaseAddress = new Uri("http://192.168.4.100/");
            
            // Set reasonable timeout
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        builder.Services.AddSingleton<IntellidriveApiService>();
        builder.Services.AddSingleton<WifiService>();
        builder.Services.AddSingleton<IntellidriveApiService>();
        builder.Services.AddSingleton<DeviceService>(); 
        builder.Services.AddSingleton<BuildingStorageService>();
        builder.Services.AddSingleton<PdfConversionService>();

        // In your service registration section:

#if ANDROID
builder.Services.AddSingleton<IPdfToPngConverter, PdfToPngConverter_Android>();

#endif


        // Neue Zeile für BuildingStorageService

        // Seiten
        builder.Services.AddTransient<IntellidriveAppMainPage>();
       
        builder.Services.AddTransient<DeviceBasisPage>();
        builder.Services.AddTransient<DeviceDistancesPage>();
        builder.Services.AddTransient<DeviceDoorFunctionPage>();
        builder.Services.AddTransient<DeviceIOPage>();
        builder.Services.AddTransient<DeviceProtocolPage>();
        builder.Services.AddTransient<DeviceSpeedPage>();
        builder.Services.AddTransient<DeviceStatusPage>();
        builder.Services.AddTransient<DeviceTimePage>();
        builder.Services.AddTransient<DeviceFunctionPage>();
        builder.Services.AddTransient<DeviceInformationsPage>();
        builder.Services.AddTransient<ChooseNetworkForLocalScan>();
        builder.Services.AddTransient<LocalSavedDeviceListPage>();
        builder.Services.AddTransient<IpRangePopup>();
        builder.Services.AddTransient<KeyInputPopup>();

        builder.Services.AddTransient<FloorPlanManagerPage>();

        builder.Services.AddTransient<BuildingEditorPage>();

        builder.Services.AddTransient<DeviceSettingsTabbedPage>();
        builder.Services.AddTransient<ScanListPage>(provider =>
        {
            var vm = provider.GetRequiredService<ScanListViewModel>();
            var api = provider.GetRequiredService<IntellidriveApiService>();
            return new ScanListPage(vm, api);
        });

        builder.Services.AddTransient<SavedDeviceListPage>(provider =>
        {
            var vm = provider.GetRequiredService<SavedDeviceListViewModel>();
            var api = provider.GetRequiredService<IntellidriveApiService>();
            var wifi = provider.GetRequiredService<WifiService>();
            return new SavedDeviceListPage(vm, api, wifi);
        });



        // ViewModels
        builder.Services.AddTransient<LocalNetworkScanViewModel>(provider => {
            var api = provider.GetRequiredService<IntellidriveApiService>();
            var deviceService = provider.GetRequiredService<DeviceService>();
            // Default SSID - will be replaced when needed
            return new LocalNetworkScanViewModel(api, deviceService, ""); 
        });

        // Pages
        // vorher: fehlte api
        builder.Services.AddTransient<LocalNetworkScanPage>(provider =>
        {
            var vm = provider.GetRequiredService<LocalNetworkScanViewModel>();
            var ds = provider.GetRequiredService<DeviceService>();
            var api = provider.GetRequiredService<IntellidriveApiService>();
            return new LocalNetworkScanPage(vm, ds, api);
        });


        // ViewModels
        builder.Services.AddSingleton<SavedDeviceListViewModel>();
        builder.Services.AddTransient<ScanListViewModel>();
        builder.Services.AddTransient<LocalSavedDeviceListViewModel>();
        builder.Services.AddTransient<VersionResponseDataModel>();
        builder.Services.AddTransient<DeviceSettingsViewModel>();
        builder.Services.AddTransient<DeviceInformationsViewModel>();
        builder.Services.AddSingleton<FloorPlanViewModel>(); // Zu Singleton geändert für bessere Persistenz



#if DEBUG
        builder.Logging.AddDebug();
  
#endif



        return builder.Build();
    }


}