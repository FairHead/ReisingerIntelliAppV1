using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Services;
using ReisingerIntelliAppV1.Views.FloorManager;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace ReisingerIntelliAppV1.Views.FloorManager;

public partial class FloorPlanManagerPage : ContentPage
{
    private readonly FloorPlanViewModel _viewModel;

    // References to UI elements
    private Button buildingButton;
    private Button floorButton;
    private Border buildingDropdown;
    private Border floorDropdown;
    private CollectionView buildingsCollection;
    private CollectionView floorsCollection;

    public FloorPlanManagerPage(FloorPlanViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        Debug.WriteLine("[FloorPlanManagerPage] Constructor called");

        // Initialize UI element references
        buildingButton = this.FindByName<Button>("BuildingButton");
        floorButton = this.FindByName<Button>("FloorButton");
        buildingDropdown = this.FindByName<Border>("BuildingDropdown");
        floorDropdown = this.FindByName<Border>("FloorDropdown");
        buildingsCollection = this.FindByName<CollectionView>("BuildingsCollection");
        floorsCollection = this.FindByName<CollectionView>("FloorsCollection");

        // Subscribe to property changed events for dropdown visibility
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.IsBuildingDropdownVisible) ||
                e.PropertyName == nameof(_viewModel.IsFloorDropdownVisible) ||
                e.PropertyName == nameof(_viewModel.IsSavedDevicesDropdownVisible) ||
                e.PropertyName == nameof(_viewModel.IsLocalDevicesDropdownVisible))
            {
                Debug.WriteLine($"[FloorPlanManagerPage] ViewModel property changed: {e.PropertyName}");

                // This helps catch any potential binding issues
                if (e.PropertyName == nameof(_viewModel.IsBuildingDropdownVisible))
                {
                    Debug.WriteLine($"[FloorPlanManagerPage] Building dropdown visibility: ViewModel={_viewModel.IsBuildingDropdownVisible}, UI={buildingDropdown.IsVisible}");
                }
            }
            else if (e.PropertyName == nameof(_viewModel.SelectedFloor))
            {
                HandleSelectedFloorChanged();
            }
        };
    }

    // Handle changes to the selected floor
    private void HandleSelectedFloorChanged()
    {
        var floor = _viewModel.SelectedFloor;
        
        if (floor == null)
        {
            Debug.WriteLine("[FloorPlanManagerPage] Selected floor is null");
            return;
        }
        
        Debug.WriteLine($"[FloorPlanManagerPage] Selected floor: {floor.Name}, PDF path: {floor.PdfPath ?? "null"}");
        
        // Safely check if the PDF file exists - avoid accessing a null or empty path
        bool pdfExists = false;
        try
        {
            pdfExists = !string.IsNullOrEmpty(floor.PdfPath) && File.Exists(floor.PdfPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FloorPlanManagerPage] Error checking PDF file: {ex.Message}");
        }

        Debug.WriteLine($"[FloorPlanManagerPage] PDF file exists: {pdfExists}");
        
        // If PDF doesn't exist, let the user know they can upload one
        if (!pdfExists)
        {
            // Use MainThread.BeginInvokeOnMainThread to ensure UI operations run on the main thread
            MainThread.BeginInvokeOnMainThread(async () => {
                bool uploadNow = await DisplayAlert("Hinweis", 
                    "Für dieses Stockwerk ist noch kein Bauplan verfügbar. Möchten Sie jetzt einen hochladen?", 
                    "Ja", "Später");
                
                if (uploadNow)
                {
                    await _viewModel.UploadPdfAsync();
                }
            });
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("[FloorPlanManagerPage] OnAppearing");
        await _viewModel.LoadDevicesAsync();

        // Update button text based on selected items
        UpdateButtonTexts();
    }

    // Update the text on buttons to show currently selected items
    private void UpdateButtonTexts()
    {
        Debug.WriteLine("[FloorPlanManagerPage] UpdateButtonTexts");

        // Update Building button text
        if (_viewModel.SelectedBuilding != null)
        {
            buildingButton.Text = _viewModel.SelectedBuilding.Name;
            // Show Floor button when a building is selected
            floorButton.IsVisible = true;
            Debug.WriteLine($"[FloorPlanManagerPage] Building button text set to: {buildingButton.Text}");
        }
        else
        {
            buildingButton.Text = "Gebäude auswählen";
            floorButton.IsVisible = true;
            Debug.WriteLine("[FloorPlanManagerPage] Building button text set to default");
        }

        // Update Floor button text
        if (_viewModel.SelectedFloor != null)
        {
            floorButton.Text = _viewModel.SelectedFloor.Name;
            Debug.WriteLine($"[FloorPlanManagerPage] Floor button text set to: {floorButton.Text}");
        }
        else
        {
            floorButton.Text = "Stockwerk auswählen";
            Debug.WriteLine("[FloorPlanManagerPage] Floor button text set to default");
        }
    }

    private async void OnAddBuildingClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("[FloorPlanManagerPage] OnAddBuildingClicked - Button clicked to add a building");
        
        try
        {
            // Use the new BuildingEditorPage instead of AddBuildingPage
            var editorPage = new BuildingEditorPage(_viewModel);
            Debug.WriteLine("[FloorPlanManagerPage] BuildingEditorPage instance created");
            
            // Ensure navigation is possible
            if (Navigation != null)
            {
                Debug.WriteLine("[FloorPlanManagerPage] Navigating to BuildingEditorPage");
                await Navigation.PushAsync(editorPage);
                Debug.WriteLine("[FloorPlanManagerPage] Navigation to BuildingEditorPage completed");
            }
            else
            {
                Debug.WriteLine("[FloorPlanManagerPage] ERROR: Navigation is null!");
                await DisplayAlert("Fehler", "Navigation nicht möglich", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FloorPlanManagerPage] ERROR in OnAddBuildingClicked: {ex.Message}");
            await DisplayAlert("Fehler", $"Ein Fehler ist aufgetreten: {ex.Message}", "OK");
        }
    }

    // Toggle visibility of building dropdown
    private void OnToggleBuildingDropdownClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("[FloorPlanManagerPage] OnToggleBuildingDropdownClicked");

        // Let the ViewModel handle the dropdown toggle logic
        _viewModel.ToggleBuildingDropdown();

        // Show a message if there are no buildings and the dropdown is visible
        if (_viewModel.IsBuildingDropdownVisible && (_viewModel.Buildings == null || _viewModel.Buildings.Count == 0))
        {
            DisplayAlert("Hinweis", "Keine Gebäude gefunden. Bitte fügen Sie ein Gebäude hinzu.", "OK");
            _viewModel.ToggleBuildingDropdown(); // Close the dropdown
        }
    }

    // Toggle visibility of floor dropdown
    private void OnToggleFloorDropdownClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("[FloorPlanManagerPage] OnToggleFloorDropdownClicked");

        // Let the ViewModel handle the dropdown toggle logic
        _viewModel.ToggleFloorDropdown();

        // Show a message if there are no floors and the dropdown is visible
        if (_viewModel.IsFloorDropdownVisible && (_viewModel.Floors == null || _viewModel.Floors.Count == 0))
        {
            DisplayAlert("Hinweis", "Keine Stockwerke für dieses Gebäude gefunden.", "OK");
            _viewModel.ToggleFloorDropdown(); // Close the dropdown
        }
    }

    // Handle building selection
    private void OnBuildingSelected(object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var building = e.CurrentSelection[0] as Building;
            if (building != null)
            {
                Debug.WriteLine($"[FloorPlanManagerPage] Building selected: {building.Name}");
                _viewModel.SelectedBuilding = building;

                // Clear selection
                buildingsCollection.SelectedItem = null;

                // Close dropdown
                _viewModel.IsBuildingDropdownVisible = false;

                // Update button texts
                UpdateButtonTexts();
            }
        }
    }

    // New handler for tapping on a building (used with SwipeView)
    private void OnBuildingTapped(object sender, EventArgs e)
    {
        if (sender is Element element && element.BindingContext is Building building)
        {
            Debug.WriteLine($"[FloorPlanManagerPage] Building tapped: {building.Name}");
            _viewModel.SelectedBuilding = building;

            // Close dropdown
            _viewModel.IsBuildingDropdownVisible = false;

            // Update button texts
            UpdateButtonTexts();
        }
    }

    // New handler for tapping on a floor (used with SwipeView)
    private void OnFloorTapped(object sender, EventArgs e)
    {
        if (sender is Element element && element.BindingContext is Floor floor)
        {
            Debug.WriteLine($"[FloorPlanManagerPage] Floor tapped: {floor.Name}");
            
            // Safely check if PDF exists before setting the selected floor
            bool pdfExists = false;
            try 
            {
                pdfExists = !string.IsNullOrEmpty(floor.PdfPath) && File.Exists(floor.PdfPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FloorPlanManagerPage] Error checking PDF file: {ex.Message}");
            }
            
            Debug.WriteLine($"[FloorPlanManagerPage] Floor PDF path: {floor.PdfPath ?? "null"}, PDF exists: {pdfExists}");

            // Set the selected floor
            _viewModel.SelectedFloor = floor;

            // Close dropdown
            _viewModel.IsFloorDropdownVisible = false;

            // Update button texts
            UpdateButtonTexts();
        }
    }

    // Handle floor selection
    private void OnFloorSelected(object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var floor = e.CurrentSelection[0] as Floor;
            if (floor != null)
            {
                Debug.WriteLine($"[FloorPlanManagerPage] Floor selected: {floor.Name}");
                
                // Safely check if PDF exists before setting the selected floor
                bool pdfExists = false;
                try 
                {
                    pdfExists = !string.IsNullOrEmpty(floor.PdfPath) && File.Exists(floor.PdfPath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[FloorPlanManagerPage] Error checking PDF file: {ex.Message}");
                }
                
                Debug.WriteLine($"[FloorPlanManagerPage] Floor PDF path: {floor.PdfPath ?? "null"}, PDF exists: {pdfExists}");

                // Set the selected floor regardless of whether a PDF exists
                // The HandleSelectedFloorChanged method will handle the PDF check and prompt
                _viewModel.SelectedFloor = floor;

                // Clear selection
                floorsCollection.SelectedItem = null;

                // Close dropdown
                _viewModel.IsFloorDropdownVisible = false;

                // Update button texts
                UpdateButtonTexts();
            }
        }
    }

    // Toggle visibility of saved devices dropdown
    private void OnToggleSavedDevicesClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("[FloorPlanManagerPage] OnToggleSavedDevicesClicked");

        // Let the ViewModel handle the dropdown toggle logic
        _viewModel.ToggleSavedDevicesDropdown();

        // Show a message if there are no saved devices and the dropdown is visible
        if (_viewModel.IsSavedDevicesDropdownVisible && _viewModel.SavedDevices.Count == 0)
        {
            DisplayAlert("Hinweis", "Keine gespeicherten Geräte gefunden.", "OK");
            _viewModel.ToggleSavedDevicesDropdown(); // Close the dropdown
        }
    }

    // Toggle visibility of local devices dropdown
    private void OnToggleLocalDevicesClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("[FloorPlanManagerPage] OnToggleLocalDevicesClicked");

        // Let the ViewModel handle the dropdown toggle logic
        _viewModel.ToggleLocalDevicesDropdown();

        // Show a message if there are no local devices and the dropdown is visible
        if (_viewModel.IsLocalDevicesDropdownVisible && _viewModel.LocalDevices.Count == 0)
        {
            DisplayAlert("Hinweis", "Keine lokalen Geräte gefunden.", "OK");
            _viewModel.ToggleLocalDevicesDropdown(); // Close the dropdown
        }
    }

    // Handle selection of a saved device
    private async void OnSavedDeviceSelected(object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var device = e.CurrentSelection[0] as DeviceModel;
            if (device != null)
            {
                Debug.WriteLine($"[FloorPlanManagerPage] Saved device selected: {device.Name}");
                await DisplayAlert("Gerät gewählt", $"Du hast {device.Name} gewählt", "OK");
                // TODO: Gerät auf PDF platzieren

                // Clear selection
                SavedDevicesCollection.SelectedItem = null;

                // Close dropdown
                _viewModel.IsSavedDevicesDropdownVisible = false;
            }
        }
    }

    // Handle selection of a local device
    private async void OnLocalDeviceSelected(object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var device = e.CurrentSelection[0] as DeviceModel;
            if (device != null)
            {
                Debug.WriteLine($"[FloorPlanManagerPage] Local device selected: {device.Name}");
                await DisplayAlert("Gerät gewählt", $"Du hast {device.Name} gewählt", "OK");
                // TODO: Gerät auf PDF platzieren

                // Clear selection
                LocalDevicesCollection.SelectedItem = null;

                // Close dropdown
                _viewModel.IsLocalDevicesDropdownVisible = false;
            }
        }
    }
}