using System;
using System.Collections.ObjectModel;
using Plugin.MauiWifiManager;
using ReisingerIntelliAppV1.Model.Models;

namespace ReisingerIntelliAppV1.Services
{
    public class WifiService
    {
        private readonly int _connectionTimeoutMilliseconds = 10000; // 10 seconds

        public WifiService()
        {

        }

        public async Task<ObservableCollection<NetworkDataModel>> ScanNetworksAsync()
        {
            var networks = new ObservableCollection<NetworkDataModel>();

            try
            {
                var networkScanResult = await CrossWifiManager.Current.ScanWifiNetworks();

                foreach (var network in networkScanResult)
                {
                    networks.Add(new NetworkDataModel
                    {
                        Ssid = network.Ssid,
                        IpAddress = network.IpAddress,
                        GatewayAddress = network.GatewayAddress,
                        Bssid = network.Bssid,
                        SignalStrength = network.SignalStrength,
                        SecurityType = network.SecurityType
                    });

                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error during network scan: {e.Message}");
            }
            return networks;
        }


        public async Task<bool> ConnectToNetworkAsync(string ssid, string password)
        {
            try
            {
                var result = await CrossWifiManager.Current.ConnectWifi(ssid, password);

                if (result?.NativeObject != null)
                {
                    // Wait a moment for the connection to fully establish
                    await Task.Delay(3000);
                    return true; // Verbindung erfolgreich
                }
                else
                {
                    throw new Exception("Verbindung fehlgeschlagen.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler bei der Verbindung: {ex.Message}");
            }
        }

        public async Task<bool> IsNetworkAvailableAsync(string ssid)
        {
            try
            {
                // Verfügbare Netzwerke scannen
                var networkScanResult = await CrossWifiManager.Current.ScanWifiNetworks();

                // Prüfen, ob die gewünschte SSID in den gescannten Netzwerken enthalten ist
                return networkScanResult.Any(network => network.Ssid.Equals(ssid, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler bei der Prüfung der Netzwerkverfügbarkeit: {ex.Message}");
                return false; // Fehler bedeutet, dass das Netzwerk nicht als online betrachtet werden kann
            }
        }


        public async Task<List<DeviceModel>> CheckDeviceNetworkStatusAsync(List<DeviceModel> devices)
        {
            try
            {
                var availableNetworks = await CrossWifiManager.Current.ScanWifiNetworks();

                // Console.WriteLine("▶️ Scan-Ergebnisse:");
                foreach (var net in availableNetworks)
                    // Console.WriteLine($"🔍 SSID gefunden: {net.Ssid}");

                foreach (var device in devices)
                {
                    device.IsOnline = availableNetworks.Any(network =>
                        network.Ssid.Equals(device.Ssid, StringComparison.OrdinalIgnoreCase));
                }

                return devices;
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"❌ Fehler beim WLAN-Scan: {ex.Message}");
                return devices;
            }
        }


        public async Task<string> GetCurrentSsidAsync()
        {
            try
            {
                var networkInfo = await CrossWifiManager.Current.GetNetworkInfo();
                return networkInfo?.Ssid ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Ermitteln der SSID: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<bool> EnsureConnectedToSsidAsync(string targetSsid)
        {
            // First, check if we're already connected to the target network
            var currentSsid = await GetCurrentSsidAsync();

            if (currentSsid.Equals(targetSsid, StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"Already connected to target network: {targetSsid}");
                return true;
            }

            bool tryConnect = await Application.Current.MainPage.DisplayAlert(
                "Nicht verbunden",
                $"Du bist nicht mit dem WLAN „{targetSsid}“ verbunden.\nMöchtest du versuchen, dich zu verbinden?",
                "Ja", "Abbrechen");

            if (!tryConnect)
                return false;

            try
            {
                // Check if the network is available before trying to connect
                bool networkAvailable = await IsNetworkAvailableAsync(targetSsid);
                if (!networkAvailable)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Netzwerk nicht gefunden",
                        $"Das WLAN „{targetSsid}“ wurde nicht gefunden. Stelle sicher, dass das Gerät eingeschaltet und in Reichweite ist.",
                        "OK");
                    return false;
                }

                // Show a loading indicator
                await Application.Current.MainPage.DisplayAlert("Verbindung wird hergestellt", $"Verbinde mit „{targetSsid}“...", "OK");

                await ConnectToNetworkAsync(targetSsid, "");

                // Allow time for the connection to establish
                await Task.Delay(5000);

                // Check if we're now connected to the target network
                currentSsid = await GetCurrentSsidAsync();

                if (currentSsid.Equals(targetSsid, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully connected to: {targetSsid}");
                    await Application.Current.MainPage.DisplayAlert("Verbunden", $"Mit „{targetSsid}“ erfolgreich verbunden.", "OK");

                    // Wait a bit more after successful connection to make sure network services are ready
                    await Task.Delay(2000);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to connect. Current network: {currentSsid}");
                    bool retry = await Application.Current.MainPage.DisplayAlert(
                        "Falsches Netzwerk",
                        $"Du bist nicht mit dem richtigen WLAN verbunden (aktuell: {currentSsid}).\n" +
                        "Nochmal versuchen?",
                        "Ja", "Nein");

                    if (!retry)
                        return false;

                    await ConnectToNetworkAsync(targetSsid, "");
                    await Task.Delay(5000);

                    currentSsid = await GetCurrentSsidAsync();

                    if (currentSsid.Equals(targetSsid, StringComparison.OrdinalIgnoreCase))
                    {
                        await Application.Current.MainPage.DisplayAlert("Verbunden", $"Mit „{targetSsid}“ erfolgreich verbunden.", "OK");
                        // Wait a bit more after successful connection
                        await Task.Delay(2000);
                        return true;
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Nicht verbunden", "Du bist immer noch nicht mit dem richtigen WLAN verbunden.", "OK");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Fehler", $"Verbindungsfehler: {ex.Message}", "OK");
                return false;
            }
        }
    }
}


