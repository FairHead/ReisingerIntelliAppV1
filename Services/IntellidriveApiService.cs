using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ReisingerIntelliAppV1.Model.Models;

namespace ReisingerIntelliAppV1.Services;

public class IntellidriveApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly int _defaultTimeoutSeconds = 10; // Default timeout

    public IntellidriveApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private async Task<string> SendPostRequestWithAuthentication(DeviceModel device, string endpoint)
    {
        try
        {
            // 1) HttpClient holen
            var client = _httpClientFactory.CreateClient("IntellidriveAPI");

            // Set shorter timeout for local connections
            client.Timeout = TimeSpan.FromSeconds(_defaultTimeoutSeconds);

            // 2) BaseAddress direkt auf http://<device.Ip> setzen
            client.BaseAddress = new Uri($"http://{device.Ip}");

            // 3) POST-Request bauen
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("User", $"{device.Username}:{device.Password}");



            Debug.WriteLine($"Sending POST request to: {client.BaseAddress}{endpoint}");
            Debug.WriteLine($"Authorization: User {device.Username}:******");

            // 4) Abschicken und Inhalt zurückgeben
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine($"Request timed out: {endpoint}");
            throw new TimeoutException($"Die Anfrage an {endpoint} hat das Zeitlimit überschritten. Bitte überprüfe die Verbindung zum Gerät.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SendPostRequestWithAuthentication: {ex}");
            throw;
        }
    }

    // Hilfsmethoden für authentifizierte Anfragen
    private async Task<string> SendGetRequestWithAuthentication(DeviceModel device, string endpoint)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("IntellidriveAPI");

            // Set shorter timeout for local connections
            client.Timeout = TimeSpan.FromSeconds(_defaultTimeoutSeconds);

            // Ensure a valid IP
            if (string.IsNullOrEmpty(device.Ip) || device.Ip == "0.0.0.0")
            {
                throw new ArgumentException("Invalid device IP address");
            }

            client.BaseAddress = new Uri($"http://{device.Ip}");

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("User", $"{device.Username}:{device.Password}");



            Debug.WriteLine($"Sending GET request to: {client.BaseAddress}{endpoint}");
            Debug.WriteLine($"Authorization: User {device.Username}:******");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine($"Request timed out: {endpoint}");
            throw new TimeoutException($"Die Anfrage an {endpoint} hat das Zeitlimit überschritten. Bitte überprüfe die Verbindung zum Gerät.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SendGetRequestWithAuthentication: {ex}");
            throw;
        }
    }

    public async Task<AuthResponseDataModel?> TestUserAuthAsync(string username, string password)
    {
        try
        {
            Debug.WriteLine("🟡 Starte Authentifizierung...");
            Debug.WriteLine($"Benutzername: {username}, Passwort-Länge: {password?.Length}");

            var client = _httpClientFactory.CreateClient("IntellidriveAPI");
            Debug.WriteLine("🟡 HttpClient erfolgreich erstellt.");

            // For WiFi devices, we use the standard IP
            var baseUri = new Uri("http://192.168.4.100/");
            client.BaseAddress = baseUri;

            var request = new HttpRequestMessage(HttpMethod.Get, "intellidrive/serialnumber");
            request.Headers.Authorization = new AuthenticationHeaderValue("User", $"{username}:{password}");


            Debug.WriteLine("🟡 Request vorbereitet:");
            Debug.WriteLine($"Methode: {request.Method}, URL: {baseUri}intellidrive/serialnumber");
            Debug.WriteLine($"Authorization: {request.Headers.Authorization}");

            var response = await client.SendAsync(request);
            Debug.WriteLine($"🟢 Antwort erhalten: StatusCode = {response.StatusCode}");

            response.EnsureSuccessStatusCode(); // 💥 Wenn hier Fehler → wird Catch ausgelöst

            var responseString = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"🟢 Antworttext: {responseString}");

            var result = JsonSerializer.Deserialize<AuthResponseDataModel>(responseString);

            if (result == null)
                Debug.WriteLine("🔴 Deserialisierung ergab NULL!");
            else
                Debug.WriteLine("✅ AuthResponseDataModel erfolgreich erstellt.");

            return result;
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine("❌ HTTP-Fehler bei Authentifizierung:");
            Debug.WriteLine(httpEx.ToString());
        }
        catch (JsonException jsonEx)
        {
            Debug.WriteLine("❌ JSON-Fehler beim Deserialisieren:");
            Debug.WriteLine(jsonEx.ToString());
        }
        catch (Exception ex)
        {
            Debug.WriteLine("❌ Allgemeiner Fehler:");
            Debug.WriteLine(ex.ToString());
        }

        return null;
    }

    public async Task<AuthResponseDataModel?> TestUserAuthLocalAsync(
        string ipAddress,
        string username,
        string password)
    {
        try
        {
            Debug.WriteLine($"🟡 Starte lokale Authentifizierung mit IP: {ipAddress}");
            Debug.WriteLine($"Benutzername: {username}, Passwort-Länge: {password?.Length}");

            var client = _httpClientFactory.CreateClient("IntellidriveAPI");
            Debug.WriteLine("🟡 HttpClient für lokale Auth erfolgreich erstellt.");

            var baseUri = new Uri($"http://{ipAddress}/");
            client.BaseAddress = baseUri;

            var request = new HttpRequestMessage(HttpMethod.Get, "intellidrive/serialnumber");
            request.Headers.Authorization = new AuthenticationHeaderValue("User", $"{username}:{password}");



            Debug.WriteLine("🟡 Lokaler Auth-Request vorbereitet:");
            Debug.WriteLine($"Methode: {request.Method}, URL: {baseUri}intellidrive/serialnumber");
            Debug.WriteLine($"Authorization: User {username}:******");

            var response = await client.SendAsync(request);
            Debug.WriteLine($"🟢 Lokale Auth-Antwort erhalten: StatusCode = {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"🔴 Lokale Auth fehlgeschlagen: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"🟢 Lokale Auth-Antworttext: {json}");

            var result = JsonSerializer.Deserialize<AuthResponseDataModel>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (result == null)
                Debug.WriteLine("🔴 Deserialisierung der lokalen Auth ergab NULL!");
            else
                Debug.WriteLine($"✅ Lokale Auth erfolgreich: {result.Success}");

            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Fehler bei lokaler Authentifizierung: {ex.Message}");
            Debug.WriteLine(ex.ToString());
            return null;
        }
    }

    public async Task<string> RestartDriveAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/restart/drive");
    }

    // Antriebs-Befehle
    public async Task<string> RestartIntellidriveAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/restart");
    }


    public async Task<VersionResponseDataModel?> GetRequestNoAuthForWifi(string endpoint)
    {
        var client = _httpClientFactory.CreateClient("IntellidriveAPI");
        client.BaseAddress = new Uri("http://192.168.4.100/");

        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

        try
        {
            Debug.WriteLine($"🌐 [WiFi-AP] Sending request to: http://192.168.4.100/{endpoint}");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"📦 Response JSON: {json}");

            var parsed = JsonSerializer.Deserialize<VersionResponseDataModel>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (parsed == null)
                Debug.WriteLine("❌ Deserialization failed.");
            else
                Debug.WriteLine($"✅ Version parsed: DeviceId = {parsed.DeviceId}");

            return parsed;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Exception during WiFi-AP GET request: {ex.GetType().Name} - {ex.Message}");
            return null;
        }
    }


    public async Task<VersionResponseDataModel?> GetRequestNoAuth(string endpoint)
    {
        var client = _httpClientFactory.CreateClient("IntellidriveAPI");
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

        try
        {
            Debug.WriteLine($"Sending no-auth request to: {endpoint}");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine("RAW JSON response: " + json);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = JsonSerializer.Deserialize<VersionResponseDataModel>(json, options);
            if (parsed == null)
                Debug.WriteLine("❌ Deserialization returned null!");
            else
                Debug.WriteLine($"✅ Version response deserialized. DeviceId: {parsed.DeviceId}");

            return parsed;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Exception in no-auth request: {ex.GetType().Name} - {ex.Message}");
            return null;
        }
    }

    public async Task<string> FactoryResetAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/factoryreset");
    }

    public async Task<string> CalibrateAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/calibrate");
    }

    public async Task<string> GetSerialNumberAsync(DeviceModel device)
    {
        return await SendGetRequestWithAuthentication(device, "intellidrive/serialnumber");
    }

    public async Task<string> BeepAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/beep");
    }

    // Tür-Befehle
    public async Task<string> GetDoorStateAsync(DeviceModel device)
    {
        return await SendGetRequestWithAuthentication(device, "intellidrive/door/state");
    }

    public async Task<string> GetDoorPositionAsync(DeviceModel device)
    {
        return await SendGetRequestWithAuthentication(device, "intellidrive/door/position");
    }

    public async Task<string> OpenDoorAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/door/open");
    }

    public async Task<string> OpenDoorFullAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/door/open-full");
    }

    public async Task<string> OpenDoorShortAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/door/open-short");
    }

    public async Task<string> CloseDoorAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/door/close");
    }

    public async Task<string> ForceCloseDoorAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/door/force-close");
    }

    public async Task<string> LockDoorAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/door/lock");
    }

    public async Task<string> UnlockDoorAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/door/unlock");
    }

    public async Task<string> EnableSummerModeAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/summer/enable");
    }

    public async Task<string> DisableSummerModeAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/summer/disable");
    }

    public async Task<string> EnableOneWayModeAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/oneway/enable");
    }

    public async Task<string> DisableOneWayModeAsync(DeviceModel device)
    {
        return await SendPostRequestWithAuthentication(device, "intellidrive/oneway/disable");
    }

    public async Task<bool> SendDoorCommandAsync(DeviceModel device, string command)
    {
        try
        {
            // Validate command
            if (command != "open" && command != "close")
            {
                throw new ArgumentException("Invalid door command. Use 'open' or 'close'.");
            }

            // Send POST request with authentication
            var response = await SendPostRequestWithAuthentication(device, $"intellidrive/door/{command}");

            // Log response
            Debug.WriteLine($"Door command '{command}' sent successfully. Response: {response}");

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error sending door command '{command}': {ex.Message}");
            return false;
        }
    }

    // Parameter-Befehle
    public async Task<Dictionary<string, string>> GetParametersAsync(DeviceModel device)
    {
        try
        {
            // 1) Hol dir den HttpClient
            var client = _httpClientFactory.CreateClient("IntellidriveAPI");

            // Set shorter timeout for local connections 
            client.Timeout = TimeSpan.FromSeconds(_defaultTimeoutSeconds);

            // 2) Überschreibe die BaseAddress mit der Geräte-IP
            if (string.IsNullOrEmpty(device.Ip) || device.Ip == "0.0.0.0")
            {
                throw new ArgumentException("Invalid device IP address");
            }

            var baseUri = new Uri($"http://{device.Ip}/");
            client.BaseAddress = baseUri;

            Debug.WriteLine($"Getting parameters from device. IP: {device.Ip}, ConnectionType: {device.ConnectionType}");

            // 3) Baue die GET-Anfrage
            var request = new HttpRequestMessage(HttpMethod.Get, "intellidrive/parameters");
            request.Headers.Authorization = new AuthenticationHeaderValue("User", $"{device.Username}:{device.Password}");



            // 4) Anfrage senden
            Debug.WriteLine($"Sending parameters request to: {baseUri}intellidrive/parameters");
            Debug.WriteLine($"Authorization: User {device.Username}:******");

            var response = await client.SendAsync(request);
            Debug.WriteLine($"Parameters response status: {response.StatusCode}");

            response.EnsureSuccessStatusCode();

            // 5) Antwort einlesen & parsen
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Received parameters response: {jsonResponse.Substring(0, Math.Min(100, jsonResponse.Length))}...");

            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (apiResponse?.Success == true && apiResponse.Values != null)
            {
                Debug.WriteLine($"Successfully parsed {apiResponse.Values.Count} parameters");
                return apiResponse.Values
                    .ToDictionary(v => $"id{v.Id}", v => v.V.ToString());
            }
            else
            {
                Debug.WriteLine($"API response success: {apiResponse?.Success}, Values count: {apiResponse?.Values?.Count ?? 0}");
                return new Dictionary<string, string>();
            }
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("Parameter request timed out");
            // Return empty dictionary instead of throwing, so UI can still load
            return new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fehler beim Abrufen der Parameter: {ex}");
            // If there's a specific network error, try to provide a more helpful message
            if (ex.ToString().Contains("net_http_client_execution_error"))
            {
                throw new Exception("Netzwerkfehler: Verbindung zum Gerät fehlgeschlagen. Bitte überprüfe die WLAN-Verbindung.", ex);
            }
            throw;
        }
    }

    public async Task<string> SetParametersAsync(DeviceModel device, string parametersJson)
    {
        var content = new StringContent(parametersJson, Encoding.UTF8, "application/json");
        return await SendPostRequestWithAuthentication(device, "intellidrive/parameters/set");
    }

    public async Task<string> GetMinParameterValuesAsync(DeviceModel device)
    {
        return await SendGetRequestWithAuthentication(device, "intellidrive/parameters/min-values");
    }

    public async Task<string> GetMaxParameterValuesAsync(DeviceModel device)
    {
        return await SendGetRequestWithAuthentication(device, "intellidrive/parameters/max-values");
    }

    public async Task<string> GetParameterUnitsAsync(DeviceModel device)
    {
        return await SendGetRequestWithAuthentication(device, "intellidrive/parameters/units");
    }

    public async Task<string> GetParameterNamesAsync(DeviceModel device)
    {
        return await SendGetRequestWithAuthentication(device, "intellidrive/parameters/names");
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<ParameterValue>? Values { get; set; }
    }

    public class ParameterValue
    {
        public int Id { get; set; }
        public int V { get; set; }
    }
}