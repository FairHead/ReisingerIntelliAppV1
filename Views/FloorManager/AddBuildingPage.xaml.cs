using ReisingerIntelliAppV1.Views.FloorManager;
using ReisingerIntelliAppV1.Model.ViewModels;
using ReisingerIntelliAppV1.Model.Models;

namespace ReisingerIntelliAppV1.Views.FloorManager;

public partial class AddBuildingPage : ContentPage
{
    private readonly FloorPlanViewModel _floorPlanViewModel;
    public AddBuildingPage(FloorPlanViewModel floorPlanViewModel)
    {
        InitializeComponent();
        _floorPlanViewModel = floorPlanViewModel;
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is BuildingEditorViewModel vm && !string.IsNullOrWhiteSpace(vm.BuildingName) && vm.Floors.Count > 0)
        {
            var building = new Building
            {
                Name = vm.BuildingName,
                Floors = new System.Collections.ObjectModel.ObservableCollection<Floor>(vm.Floors.Select(f => new Floor { Name = f.Name, PdfPath = f.PdfPath }))
            };
            _floorPlanViewModel.AddBuilding(building);
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Fehler", "Bitte Gebaeudename und mindestens ein Stockwerk angeben.", "OK");
        }
    }
}