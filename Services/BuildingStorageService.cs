using System.Collections.ObjectModel;
using System.Text.Json;
using ReisingerIntelliAppV1.Model.Models;

namespace ReisingerIntelliAppV1.Services;

public class BuildingStorageService
{
    private const string BuildingsKey = "Buildings";

    /// <summary>
    /// Speichert die Liste der Gebäude im SecureStorage
    /// </summary>
    public async Task SaveBuildingsAsync(ObservableCollection<Building> buildings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(buildings, options);
            await SecureStorage.SetAsync(BuildingsKey, json);
            
            System.Diagnostics.Debug.WriteLine($"[BuildingStorageService] Gebäude gespeichert: {buildings.Count} Einträge");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BuildingStorageService] Fehler beim Speichern der Gebäude: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Lädt die Liste der Gebäude aus dem SecureStorage
    /// </summary>
    public async Task<ObservableCollection<Building>> LoadBuildingsAsync()
    {
        try
        {
            var json = await SecureStorage.GetAsync(BuildingsKey);

            if (string.IsNullOrEmpty(json))
            {
                System.Diagnostics.Debug.WriteLine("[BuildingStorageService] Keine gespeicherten Gebäude gefunden, leere Liste zurückgegeben");
                return new ObservableCollection<Building>();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var buildings = JsonSerializer.Deserialize<List<Building>>(json, options);
            
            // Konvertiere zu ObservableCollection für die Binding-Unterstützung
            var observableBuildings = new ObservableCollection<Building>(buildings ?? new List<Building>());
            
            System.Diagnostics.Debug.WriteLine($"[BuildingStorageService] {observableBuildings.Count} Gebäude geladen");
            
            return observableBuildings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BuildingStorageService] Fehler beim Laden der Gebäude: {ex.Message}");
            return new ObservableCollection<Building>(); // Im Fehlerfall leere Liste zurückgeben
        }
    }
}