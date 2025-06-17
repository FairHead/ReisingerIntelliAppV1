using CommunityToolkit.Maui.Behaviors;
using ReisingerIntelliAppV1.Model.Models;
using System.Diagnostics;
using System.Windows.Input;
using ReisingerIntelliAppV1.Model.ViewModels;

namespace ReisingerIntelliAppV1.Views.FloorManager;

public partial class PlacedDeviceControl : ContentView
{
    private bool isDragging = false;
    private double startX, startY;
    private double panStartX, panStartY;

    public PlacedDeviceControl()
    {
        InitializeComponent();
    }

    // Public property to expose drag status
    public bool IsDragMode => isDragging;

    // 🔹 Bindbare Properties (für Commands von außen)
    public static readonly BindableProperty OpenSettingsCommandProperty =
        BindableProperty.Create(nameof(OpenSettingsCommand), typeof(ICommand), typeof(PlacedDeviceControl));

    public static readonly BindableProperty DeleteDeviceCommandProperty =
        BindableProperty.Create(nameof(DeleteDeviceCommand), typeof(ICommand), typeof(PlacedDeviceControl));

    public static readonly BindableProperty ToggleDoorCommandProperty =
        BindableProperty.Create(nameof(ToggleDoorCommand), typeof(ICommand), typeof(PlacedDeviceControl));


    public ICommand OpenSettingsCommand
    {
        get => (ICommand)GetValue(OpenSettingsCommandProperty);
        set => SetValue(OpenSettingsCommandProperty, value);
    }

    public ICommand DeleteDeviceCommand
    {
        get => (ICommand)GetValue(DeleteDeviceCommandProperty);
        set => SetValue(DeleteDeviceCommandProperty, value);
    }

    public ICommand ToggleDoorCommand
    {
        get => (ICommand)GetValue(ToggleDoorCommandProperty);
        set => SetValue(ToggleDoorCommandProperty, value);
    }

    // Aktiviert Drag (von LongPress)
    public ICommand StartDragCommand => new Command(() =>
    {
        isDragging = true;
        this.ScaleTo(1.1, 100); // Visuelles Feedback
        Debug.WriteLine("[PlacedDeviceControl] Drag aktiviert (LongPress)");
    });

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        if (BindingContext is PlacedDeviceModel device)
        {
            e.Data.Properties["DeviceId"] = device.DeviceId;
        }
    }

    private async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (!isDragging) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                startX = TranslationX;
                startY = TranslationY;
                panStartX = e.TotalX;
                panStartY = e.TotalY;
                break;

            case GestureStatus.Running:
                TranslationX = startX + (e.TotalX - panStartX);
                TranslationY = startY + (e.TotalY - panStartY);
                break;

            case GestureStatus.Completed:
                if (BindingContext is PlacedDeviceModel placedDevice)
                {
                    var parent = this.Parent as VisualElement;
                    if (parent == null) return;

                    var absX = this.X + this.TranslationX;
                    var absY = this.Y + this.TranslationY;

                    placedDevice.RelativeX = absX / parent.Width;
                    placedDevice.RelativeY = absY / parent.Height;

                    this.TranslationX = 0;
                    this.TranslationY = 0;

                    // Speicheränderung
                    if (Application.Current.MainPage is Shell shell &&
                        shell.CurrentPage is FloorPlanManagerPage page &&
                        page.BindingContext is FloorPlanViewModel vm)
                    {
                        await vm.SaveBuildingsAsync();
                    }
                }

                isDragging = false;
                this.ScaleTo(1.0, 100);
                break;
            case GestureStatus.Canceled:
                isDragging = false;
                this.ScaleTo(1.0, 100);
                UpdateModelPosition();
                break;
        }
    }

    private void UpdateModelPosition()
    {
        if (BindingContext is not PlacedDeviceModel model || Parent is not VisualElement parent)
            return;

        double centerX = this.X + this.Width / 2;
        double centerY = this.Y + this.Height / 2;

        model.RelativeX = centerX / parent.Width;
        model.RelativeY = centerY / parent.Height;

        Debug.WriteLine($"Neue Position: X={model.RelativeX:F2}, Y={model.RelativeY:F2}");
    }
}
