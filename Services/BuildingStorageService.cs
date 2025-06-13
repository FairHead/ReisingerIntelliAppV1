using System.Collections.ObjectModel;
using System.Text.Json;
using ReisingerIntelliAppV1.Model.Models;

namespace ReisingerIntelliAppV1.Services;

public class BuildingStorageService
{
    private const string BuildingsKey = "Buildings";

    /// <summary>
    /// Speichert die Liste der Geb�ude im SecureStorage
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
            
            System.Diagnostics.Debug.WriteLine($"[BuildingStorageService] Geb�ude gespeichert: {buildings.Count} Eintr�ge");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BuildingStorageService] Fehler beim Speichern der Geb�ude: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// L�dt die Liste der Geb�ude aus dem SecureStorage
    /// </summary>
    public async Task<ObservableCollection<Building>> LoadBuildingsAsync()
    {
        try
        {
            var json = await SecureStorage.GetAsync(BuildingsKey);

            if (string.IsNullOrEmpty(json))
            {
                System.Diagnostics.Debug.WriteLine("[BuildingStorageService] Keine gespeicherten Geb�ude gefunden, leere Liste zur�ckgegeben");
                return new ObservableCollection<Building>();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var buildings = JsonSerializer.Deserialize<List<Building>>(json, options);
            
            // Konvertiere zu ObservableCollection f�r die Binding-Unterst�tzung
            var observableBuildings = new ObservableCollection<Building>(buildings ?? new List<Building>());
            
            System.Diagnostics.Debug.WriteLine($"[BuildingStorageService] {observableBuildings.Count} Geb�ude geladen");
            
            return observableBuildings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BuildingStorageService] Fehler beim Laden der Geb�ude: {ex.Message}");
            return new ObservableCollection<Building>(); // Im Fehlerfall leere Liste zur�ckgeben
        }
    }
}