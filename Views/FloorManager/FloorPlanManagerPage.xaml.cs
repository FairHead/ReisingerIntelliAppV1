using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Services;
using ReisingerIntelliAppV1.Views.FloorManager;
using Microsoft.Maui.Controls;

namespace ReisingerIntelliAppV1.Views.FloorManager;

public partial class FloorPlanManagerPage : ContentPage
{
    private readonly FloorPlanViewModel _viewModel;
    private readonly DeviceService _deviceService;

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

        // Initialize UI element references
        buildingButton = this.FindByName<Button>("BuildingButton");
        floorButton = this.FindByName<Button>("FloorButton");
        buildingDropdown = this.FindByName<Border>("BuildingDropdown");
        floorDropdown = this.FindByName<Border>("FloorDropdown");
        buildingsCollection = this.FindByName<CollectionView>("BuildingsCollection");
        floorsCollection = this.FindByName<CollectionView>("FloorsCollection");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDevicesAsync();
        

        // Update button text based on selected items
        UpdateButtonTexts();
    }

    // Update the text on buttons to show currently selected items
    private void UpdateButtonTexts()
    {
        // Update Building button text
        if (_viewModel.SelectedBuilding != null)
        {
            buildingButton.Text = _viewModel.SelectedBuilding.Name;
            // Show Floor button when a building is selected
            floorButton.IsVisible = true;
        }
        else
        {
            buildingButton.Text = "Gebäude auswählen";
            floorButton.IsVisible = true;
        }

        // Update Floor button text
        if (_viewModel.SelectedFloor != null)
        {
            floorButton.Text = _viewModel.SelectedFloor.Name;
        }
        else
        {
            floorButton.Text = "Stockwerk auswählen";
        }
    }

    private async void OnAddBuildingClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddBuildingPage(_viewModel));
    }

    // Toggle visibility of building dropdown
    private void OnToggleBuildingDropdownClicked(object sender, EventArgs e)
    {
        // Hide all other dropdowns
        HideAllDropdowns();

        // Toggle building dropdown
        buildingDropdown.IsVisible = !buildingDropdown.IsVisible;

        // Show a message if there are no buildings
        if (buildingDropdown.IsVisible && (_viewModel.Buildings == null || _viewModel.Buildings.Count == 0))
        {
            DisplayAlert("Hinweis", "Keine Gebäude gefunden. Bitte fügen Sie ein Gebäude hinzu.", "OK");
            buildingDropdown.IsVisible = false;
        }
    }

    // Toggle visibility of floor dropdown
    private void OnToggleFloorDropdownClicked(object sender, EventArgs e)
    {
        // Hide all other dropdowns
        HideAllDropdowns();

        // Toggle floor dropdown
        floorDropdown.IsVisible = !floorDropdown.IsVisible;

        // Show a message if there are no floors
        if (floorDropdown.IsVisible && (_viewModel.Floors == null || _viewModel.Floors.Count == 0))
        {
            DisplayAlert("Hinweis", "Keine Stockwerke für dieses Gebäude gefunden.", "OK");
            floorDropdown.IsVisible = false;
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
                _viewModel.SelectedBuilding = building;

                // Clear selection
                buildingsCollection.SelectedItem = null;

                // Close dropdown after selection
                buildingDropdown.IsVisible = false;

                // Update button texts
                UpdateButtonTexts();
            }
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
                _viewModel.SelectedFloor = floor;

                // Clear selection
                floorsCollection.SelectedItem = null;

                // Close dropdown after selection
                floorDropdown.IsVisible = false;

                // Update button texts
                UpdateButtonTexts();
            }
        }
    }

    // Helper method to hide all dropdowns
    private void HideAllDropdowns()
    {
        buildingDropdown.IsVisible = false;
        floorDropdown.IsVisible = false;
        SavedDevicesDropdown.IsVisible = false;
        LocalDevicesDropdown.IsVisible = false;
    }

    // Toggle visibility of saved devices dropdown
    private void OnToggleSavedDevicesClicked(object sender, EventArgs e)
    {
        // Hide all other dropdowns
        HideAllDropdowns();

        // Toggle the saved devices dropdown
        SavedDevicesDropdown.IsVisible = !SavedDevicesDropdown.IsVisible;

        // Show a message if there are no saved devices
        if (SavedDevicesDropdown.IsVisible && _viewModel.SavedDevices.Count == 0)
        {
            DisplayAlert("Hinweis", "Keine gespeicherten Geräte gefunden.", "OK");
            SavedDevicesDropdown.IsVisible = false;
        }
    }

    // Toggle visibility of local devices dropdown
    private void OnToggleLocalDevicesClicked(object sender, EventArgs e)
    {
        // Hide all other dropdowns
        HideAllDropdowns();

        // Toggle the local devices dropdown
        LocalDevicesDropdown.IsVisible = !LocalDevicesDropdown.IsVisible;

        // Show a message if there are no local devices
        if (LocalDevicesDropdown.IsVisible && _viewModel.LocalDevices.Count == 0)
        {
            DisplayAlert("Hinweis", "Keine lokalen Geräte gefunden.", "OK");
            LocalDevicesDropdown.IsVisible = false;
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
                await DisplayAlert("Gerät gewählt", $"Du hast {device.Name} gewählt", "OK");
                // TODO: Gerät auf PDF platzieren

                // Clear selection
                SavedDevicesCollection.SelectedItem = null;

                // Close dropdown after selection
                SavedDevicesDropdown.IsVisible = false;
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
                await DisplayAlert("Gerät gewählt", $"Du hast {device.Name} gewählt", "OK");
                // TODO: Gerät auf PDF platzieren

                // Clear selection
                LocalDevicesCollection.SelectedItem = null;

                // Close dropdown after selection
                LocalDevicesDropdown.IsVisible = false;
            }
        }
    }
}