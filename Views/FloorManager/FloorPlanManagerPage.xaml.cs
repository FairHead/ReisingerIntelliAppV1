using Microsoft.Maui.Layouts;
using ReisingerIntelliAppV1.Model.Models;
using ReisingerIntelliAppV1.Model.ViewModels;
using System.Diagnostics;
using System.ComponentModel;

namespace ReisingerIntelliAppV1.Views.FloorManager;

public static class VisualElementExtensions
{
    public static Point GetAbsolutePosition(this VisualElement view, VisualElement relativeTo)
    {
        var location = new Point(view.X, view.Y);
        var parent = view.Parent as VisualElement;
        while (parent != null && parent != relativeTo)
        {
            location = new Point(location.X + parent.X, location.Y + parent.Y);
            parent = parent.Parent as VisualElement;
        }
        return location;
    }
}

public partial class FloorPlanManagerPage : ContentPage
{
    private readonly FloorPlanViewModel _viewModel;

    private Button buildingButton, floorButton, savedDevicesButton, localDevicesButton;
    private Border buildingDropdown, floorDropdown, savedDevicesDropdown, localDevicesDropdown;
    private CollectionView buildingsCollection, floorsCollection, savedDevicesCollection, localDevicesCollection;
    private Grid overlayGrid;

    public FloorPlanManagerPage(FloorPlanViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            InitializeUIReferences();
            ConfigureOverlayGrid();
        });

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.LoadBuildingsAsync();
            await _viewModel.LoadDevicesAsync();
            _viewModel.StartOnlineStatusUpdater();

            UpdateButtonTexts();
            InitializeUIReferences();
            ConfigureOverlayGrid();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnAppearing error: {ex}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopOnlineStatusUpdater();
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.SelectedFloor))
        {
            // Neue Methode zum Laden des PNGs kommt später hier rein
        }
        else if (e.PropertyName == nameof(_viewModel.IsBuildingDropdownVisible)
              || e.PropertyName == nameof(_viewModel.IsFloorDropdownVisible)
              || e.PropertyName == nameof(_viewModel.IsSavedDevicesDropdownVisible)
              || e.PropertyName == nameof(_viewModel.IsLocalDevicesDropdownVisible))
        {
            UpdateOverlayGridState();
        }
    }

    private void InitializeUIReferences()
    {
        buildingButton = this.FindByName<Button>("BuildingButton");
        floorButton = this.FindByName<Button>("FloorButton");
        savedDevicesButton = this.FindByName<Button>("SavedDevicesButton");
        localDevicesButton = this.FindByName<Button>("LocalDevicesButton");
        buildingDropdown = this.FindByName<Border>("BuildingDropdown");
        floorDropdown = this.FindByName<Border>("FloorDropdown");
        savedDevicesDropdown = this.FindByName<Border>("SavedDevicesDropdown");
        localDevicesDropdown = this.FindByName<Border>("LocalDevicesDropdown");
        buildingsCollection = this.FindByName<CollectionView>("BuildingsCollection");
        floorsCollection = this.FindByName<CollectionView>("FloorsCollection");
        savedDevicesCollection = this.FindByName<CollectionView>("SavedDevicesCollection");
        localDevicesCollection = this.FindByName<CollectionView>("LocalDevicesCollection");
        overlayGrid = this.FindByName<Grid>("OverlayGrid");
    }

    private void ConfigureOverlayGrid()
    {
        overlayGrid.InputTransparent = false;
        overlayGrid.IsVisible = false;
    }

    private void UpdateOverlayGridState()
    {
        bool anyDropdownVisible = _viewModel.IsBuildingDropdownVisible
            || _viewModel.IsFloorDropdownVisible
            || _viewModel.IsSavedDevicesDropdownVisible
            || _viewModel.IsLocalDevicesDropdownVisible;

        overlayGrid.IsVisible = anyDropdownVisible;
    }

    private void UpdateButtonTexts()
    {
        if (_viewModel.SelectedBuilding != null && buildingButton != null)
            buildingButton.Text = _viewModel.SelectedBuilding.BuildingName;
        else if (buildingButton != null)
            buildingButton.Text = "Gebäude auswählen";

        if (_viewModel.SelectedFloor != null && floorButton != null)
            floorButton.Text = _viewModel.SelectedFloor.FloorName;
        else if (floorButton != null)
            floorButton.Text = "Stockwerk auswählen";
    }

    private void OnBuildingTapped(object sender, EventArgs e)
    {
        if (sender is Element element && element.BindingContext is Building building)
        {
            _viewModel.SelectedBuilding = building;
            _viewModel.IsBuildingDropdownVisible = false;
            UpdateButtonTexts();
        }
    }

    private void OnFloorTapped(object sender, EventArgs e)
    {
        if (sender is Element element && element.BindingContext is Floor floor)
        {
            _viewModel.SelectedFloor = floor;
            _viewModel.IsFloorDropdownVisible = false;
            UpdateButtonTexts();
        }
    }

    private void OnFloorSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var floor = e.CurrentSelection[0] as Floor;
            if (floor != null)
            {
                _viewModel.SelectedFloor = floor;
                floorsCollection.SelectedItem = null;
                _viewModel.IsFloorDropdownVisible = false;
                UpdateButtonTexts();
            }
        }
    }

    private void OnBuildingSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var building = e.CurrentSelection[0] as Building;
            if (building != null)
            {
                _viewModel.SelectedBuilding = building;
                buildingsCollection.SelectedItem = null;
                _viewModel.IsBuildingDropdownVisible = false;
                UpdateButtonTexts();
            }
        }
    }

    private async void OnAddBuildingClicked(object sender, EventArgs e)
    {
        var editorPage = new BuildingEditorPage(_viewModel);
        if (Navigation != null)
            await Navigation.PushAsync(editorPage);
    }

    private void OnPageTapped(object sender, TappedEventArgs e)
    {
        var pos = e.GetPosition(this);
        if (!pos.HasValue) return;

        Point tapPosition = pos.Value;
        var dropdownElements = new List<VisualElement?> { buildingsCollection, floorsCollection, savedDevicesCollection, localDevicesCollection, buildingDropdown, floorDropdown, savedDevicesDropdown, localDevicesDropdown };
        var dropdownButtons = new List<VisualElement?> { buildingButton, floorButton, savedDevicesButton, localDevicesButton };

        bool tapInsideDropdown = dropdownElements.Where(el => el != null && el.IsVisible).Any(el => IsTapInsideElement(el!, tapPosition));
        bool tapOnButtons = dropdownButtons.Where(el => el != null && el.IsVisible).Any(el => IsTapInsideElement(el!, tapPosition));

        if (!tapInsideDropdown && !tapOnButtons)
        {
            _viewModel.IsBuildingDropdownVisible = false;
            _viewModel.IsFloorDropdownVisible = false;
            _viewModel.IsSavedDevicesDropdownVisible = false;
            _viewModel.IsLocalDevicesDropdownVisible = false;
        }
    }

    private bool IsTapInsideElement(VisualElement element, Point tapPosition)
    {
        try
        {
            var absolutePosition = element.GetAbsolutePosition(this);
            var bounds = new Rect(absolutePosition.X, absolutePosition.Y, element.Width, element.Height);
            return bounds.Contains(tapPosition);
        }
        catch
        {
            return false;
        }
    }
}
