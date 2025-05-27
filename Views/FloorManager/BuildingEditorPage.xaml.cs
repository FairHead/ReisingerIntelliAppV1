using ReisingerIntelliAppV1.Views.FloorManager;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Model.Models;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace ReisingerIntelliAppV1.Views.FloorManager;

public partial class BuildingEditorPage : ContentPage
{
    private readonly FloorPlanViewModel _floorPlanViewModel;
    private readonly BuildingEditorViewModel _editorViewModel;
    private readonly Building _buildingToEdit;
    private readonly bool _isEditMode;
    
    // Constructor for adding a new building
    public BuildingEditorPage(FloorPlanViewModel floorPlanViewModel)
    {
        InitializeComponent();
        Debug.WriteLine("[BuildingEditorPage] Constructor called for adding new building");
        
        _floorPlanViewModel = floorPlanViewModel;
        _editorViewModel = new BuildingEditorViewModel();
        _isEditMode = false;
        
        // Create at least one floor by default to improve user experience
        _editorViewModel.AddFloor();
        
        // Set page title
        _editorViewModel.PageTitle = "Gebäude hinzufügen";
        
        // Set the BindingContext1234567890ß
        BindingContext = _editorViewModel;
        
        Debug.WriteLine("[BuildingEditorPage] BindingContext set to BuildingEditorViewModel");
    }
    
    // Constructor for editing an existing building
    public BuildingEditorPage(FloorPlanViewModel floorPlanViewModel, Building buildingToEdit)
    {
        InitializeComponent();
        Debug.WriteLine($"[BuildingEditorPage] Constructor called for editing building: {buildingToEdit.Name}");
        
        _floorPlanViewModel = floorPlanViewModel;
        _buildingToEdit = buildingToEdit;
        _isEditMode = true;
        
        // Create editor view model and fill it with building data
        _editorViewModel = new BuildingEditorViewModel
        {
            BuildingName = buildingToEdit.Name,
            PageTitle = "Gebäude bearbeiten"
        };
        
        // Copy floor data
        foreach (var floor in buildingToEdit.Floors)
        {
            _editorViewModel.Floors.Add(new Floor
            {
                Name = floor.Name,
                PdfPath = floor.PdfPath
            });
        }
        
        // Set the BindingContext
        BindingContext = _editorViewModel;
        
        Debug.WriteLine("[BuildingEditorPage] BindingContext set to BuildingEditorViewModel with existing data");
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("[BuildingEditorPage] OnAppearing");
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("[BuildingEditorPage] SaveButton_Clicked");
        
        try
        {
            if (_editorViewModel != null && !string.IsNullOrWhiteSpace(_editorViewModel.BuildingName) && _editorViewModel.Floors.Count > 0)
            {
                Debug.WriteLine($"[BuildingEditorPage] Saving building: {_editorViewModel.BuildingName} with {_editorViewModel.Floors.Count} floors");
                
                if (_isEditMode)
                {
                    // Update existing building
                    _buildingToEdit.Name = _editorViewModel.BuildingName;
                    
                    // We need to clear and re-add floors to keep existing references intact
                    _buildingToEdit.Floors.Clear();
                    
                    foreach (var floor in _editorViewModel.Floors)
                    {
                        _buildingToEdit.Floors.Add(new Floor
                        {
                            Name = floor.Name,
                            PdfPath = floor.PdfPath
                        });
                    }
                    
                    // Make sure UI updates with changed values
                    _floorPlanViewModel.NotifyBuildingChanged(_buildingToEdit);
                    
                    Debug.WriteLine("[BuildingEditorPage] Building updated");
                }
                else
                {
                    // Create new building
                    var building = new Building
                    {
                        Name = _editorViewModel.BuildingName,
                        Floors = new ObservableCollection<Floor>(
                            _editorViewModel.Floors.Select(f => new Floor 
                            { 
                                Name = f.Name, 
                                PdfPath = f.PdfPath 
                            })
                        )
                    };
                    
                    _floorPlanViewModel.AddBuilding(building);
                    Debug.WriteLine("[BuildingEditorPage] Building added to ViewModel");
                }
                
                await Navigation.PopAsync();
                Debug.WriteLine("[BuildingEditorPage] Navigation.PopAsync completed");
            }
            else
            {
                Debug.WriteLine("[BuildingEditorPage] Validation failed - showing error");
                await DisplayAlert("Fehler", "Bitte Gebäudename und mindestens ein Stockwerk angeben.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BuildingEditorPage] ERROR in SaveButton_Clicked: {ex.Message}");
            await DisplayAlert("Fehler", $"Ein Fehler ist aufgetreten: {ex.Message}", "OK");
        }
    }
    
    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("[BuildingEditorPage] CancelButton_Clicked");
        await Navigation.PopAsync();
    }
}