using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Services;
using ReisingerIntelliAppV1.Views.PopUp;
// für Select, Reverse

namespace ReisingerIntelliAppV1.Model.ViewModels;

public partial class LocalNetworkScanViewModel : ObservableObject
{
    // <<< neu hinzufügen >>>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    private readonly IntellidriveApiService _api;
    private readonly DeviceService _deviceService;
    public IAsyncRelayCommand<LocalNetworkDeviceModel> SaveDeviceAsyncCommand { get; }

    [ObservableProperty]
    private string ssidName;
    public LocalNetworkScanViewModel(
        IntellidriveApiService api,
        DeviceService deviceService,
        string ssidName = "")        // Made optional with default value
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
        SsidName = ssidName;
        LocalDevices = new ObservableCollection<LocalNetworkDeviceModel>();
    }

    [ObservableProperty]
    string ipRange;

    public ObservableCollection<LocalNetworkDeviceModel> LocalDevices { get; }

    // bleibt so erhalten
    public bool IsNotBusy => !IsBusy;

    [RelayCommand]
    public async Task SaveDeviceAsync(LocalNetworkDeviceModel device)
    {
        if (device == null)
        {
            await ShowAlert("Fehler", "Kein Gerät ausgewählt", "OK");
            return;
        }

        try
        {
            // Ensure all needed properties are not null
            if (string.IsNullOrEmpty(device.IpAddress))
            {
                await ShowAlert("Fehler", "Keine IP-Adresse für das Gerät vorhanden", "OK");
                return;
            }

            // 1) ZUERST: Authentifizierungsdaten abfragen
            var authRaw = await ShowPopupAsync(new KeyInputPopup());
            if (authRaw is not AuthDataModel authData)
                return;

            if (string.IsNullOrEmpty(authData.Username) || string.IsNullOrEmpty(authData.Password))
            {
                await ShowAlert("Fehler", "Benutzername oder Passwort fehlt", "OK");
                return;
            }

            // Log authentication information for debugging
            Debug.WriteLine($"Attempting to authenticate with local device at IP: {device.IpAddress}");
            Debug.WriteLine($"Using username: {authData.Username}, password length: {authData.Password?.Length ?? 0}");

            // 2) Mit den Daten authentifizieren
            AuthResponseDataModel? authResp = null;
            try
            {
                authResp = await _api.TestUserAuthLocalAsync(
                    device.IpAddress,
                    authData.Username,
                    authData.Password
                );
                
                Debug.WriteLine($"Authentication response: {authResp?.Success}");
            }
            catch (Exception authEx)
            {
                Debug.WriteLine($"Authentication error: {authEx.Message}");
                await ShowAlert(
                    "Authentifizierungsfehler",
                    $"Fehler bei der Verbindung zum Gerät: {authEx.Message}",
                    "OK"
                );
                return;
            }

            if (authResp?.Success != true)
            {
                Debug.WriteLine($"Authentication failed: {authResp?.Message}");
                await ShowAlert(
                    "Login fehlgeschlagen",
                    authResp?.Message ?? "Authentifizierung fehlgeschlagen.",
                    "OK"
                );
                return;
            }

            // 3) Versionsdaten abrufen
            VersionResponseDataModel? version = null;
            try
            {
                version = await _api.GetRequestNoAuth(
                    $"http://{device.IpAddress}/intellidrive/version"
                );

                // Ensure version is not null
                if (version == null)
                {
                    Debug.WriteLine("Version information couldn't be retrieved");
                    await ShowAlert(
                        "Fehler",
                        "Versionsinformationen konnten nicht abgerufen werden",
                        "OK"
                    );
                    return;
                }
                
                Debug.WriteLine($"Retrieved version info: {version.FirmwareVersion}");
            }
            catch (Exception versionEx)
            {
                Debug.WriteLine($"Version retrieval error: {versionEx.Message}");
                await ShowAlert(
                    "Versionsfehler",
                    $"Fehler beim Abrufen der Versionsinfo: {versionEx.Message}",
                    "OK"
                );
                return;
            }
            
            // 4) DANACH (nach erfolgreicher Authentifizierung): Gerät benennen
            var namePopupResult = await ShowPopupAsync(new LocalDeviceNamePopup(device));
            if (namePopupResult is not LocalNetworkDeviceModel namedDevice)
            {
                Debug.WriteLine("Device naming canceled by user");
                return;
            }
            
            // Use the device with the user-provided name
            device = namedDevice;

            // 5) DeviceModel erstellen
            var model = new DeviceModel
            {
                DeviceId = device.DeviceId ?? "Unknown",
                // Use the custom name if provided, keep Serial Number separate
                Name = !string.IsNullOrEmpty(device.CustomName) ? device.CustomName : (device.SerialNumber ?? "Unknown Device"),
                Ssid = SsidName ?? "",
                Ip = device.IpAddress, // Use the actual scanned IP for local devices
                Username = authData.Username,
                Password = authData.Password,
                ConnectionType = ConnectionType.Local, // Explicitly set as Local type
                SerialNumber = device.SerialNumber ?? "Unknown", // Keep the real serial number separate from name
                FirmwareVersion = version.FirmwareVersion ?? "Unknown",
                LatestFirmware = version.LatestFirmware,
                SoftwareVersion = version.Message ?? "Unknown",
                // Make sure these properties are initialized
                ModuleType = "Default",
                ModuleId = device.DeviceId ?? "Unknown"
            };
            
            Debug.WriteLine($"Saving LOCAL device with name: {model.Name}, Serial: {model.SerialNumber}, IP: {model.Ip}, Connection Type: {model.ConnectionType}");

            // 6) Speichern & Ergebnis anzeigen
            var devices = await _deviceService.AddDeviceAndReturnUpdatedList(model);

            await ShowAlert(
                "Gerät gespeichert!",
                $"Aktuelle lokale Geräte: {string.Join(", ", devices.Where(d => d.ConnectionType == ConnectionType.Local).Select(d => d.Name))}",
                "OK"
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unexpected error saving device: {ex.Message}");
            await ShowAlert("Fehler", $"Unerwarteter Fehler: {ex.Message}", "OK");
        }
    }

    // Helper method to handle showing alerts - handles null check for Shell.Current.CurrentPage
    private async Task ShowAlert(string title, string message, string cancel)
    {
        var page = GetCurrentPage();
        if (page != null)
        {
            await page.DisplayAlert(title, message, cancel);
        }
        else
        {
            // Fallback if page is null
            var mainPage = Application.Current?.MainPage;
            if (mainPage != null)
            {
                await mainPage.DisplayAlert(title, message, cancel);
            }
        }
    }

    // Helper method to handle showing popups - handles null check for Shell.Current.CurrentPage
    private async Task<object> ShowPopupAsync(Popup popup)
    {
        var page = GetCurrentPage();
        if (page != null)
        {
            return await page.ShowPopupAsync(popup);
        }
        
        // Fallback if page is null
        var mainPage = Application.Current?.MainPage;
        if (mainPage != null)
        {
            return await mainPage.ShowPopupAsync(popup);
        }
        
        return null;
    }

    // Helper method to get the current page safely
    private Page GetCurrentPage()
    {
        if (Shell.Current?.CurrentPage != null)
        {
            return Shell.Current.CurrentPage;
        }
        return Application.Current?.MainPage;
    }

    [RelayCommand]
    public async Task LoadLocalNetworkDevicesAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            LocalDevices.Clear();

            var saved = (await _deviceService.LoadDeviceList())
                            .Devices
                            .Select(d => d.SerialNumber)
                            .ToHashSet();

            // 1) Range auseinandernehmen wie gehabt
            var parts = IpRange.Split('-', StringSplitOptions.RemoveEmptyEntries);
            var startBytes = IPAddress.Parse(parts[0]).GetAddressBytes();
            var endBytes = IPAddress.Parse(parts[1]).GetAddressBytes();
            uint start = BitConverter.ToUInt32(startBytes.Reverse().ToArray(), 0);
            uint end = BitConverter.ToUInt32(endBytes.Reverse().ToArray(), 0);

            // 2) Liste aller IP-Strings erzeugen
            var ips = Enumerable
                .Range(0, (int)(end - start + 1))
                .Select(i =>
                {
                    var addrBytes = BitConverter.GetBytes(start + (uint)i).Reverse().ToArray();
                    return new IPAddress(addrBytes).ToString();
                })
                .ToList();

            Debug.WriteLine($"Scanning IP range: {parts[0]} to {parts[1]} ({ips.Count} addresses)");

            // 3) Konkurrierende Scans mit Maximal 20 gleichzeitig
            var semaphore = new SemaphoreSlim(20);

            var scanTasks = ips.Select(async ip =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var resp = await _api.GetRequestNoAuth($"http://{ip}/intellidrive/version");
                    if (resp?.Content?.DeviceSerialNo != null)
                    {
                        Debug.WriteLine($"Found device at IP: {ip}, Serial: {resp.Content.DeviceSerialNo}");
                        
                        // UI‐Update muss auf dem Main-Thread laufen
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            LocalDevices.Add(new LocalNetworkDeviceModel
                            {
                                IpAddress = ip,
                                DeviceId = resp.DeviceId,
                                SerialNumber = resp.Content.DeviceSerialNo,
                                FirmwareVersion = resp.FirmwareVersion,
                                SoftwareVersion = resp.Message,
                                LatestFirmware = resp.LatestFirmware,
                                IsAlreadySaved = saved.Contains(resp.Content.DeviceSerialNo)
                            });
                        });
                    }
                }
                catch
                {
                    // optional: Fehler pro IP ignorieren
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();

            // 4) Auf alle Scans warten
            await Task.WhenAll(scanTasks);
            
            Debug.WriteLine($"Scan completed, found {LocalDevices.Count} devices");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Scan failed: {ex.Message}");
            await ShowAlert("Fehler", $"Scan fehlgeschlagen: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}