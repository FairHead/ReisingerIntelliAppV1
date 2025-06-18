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
        this.ScaleTo(1.2, 150); // Visuelles Feedback - mehr prominent
        LogCoordinateInfo("Long Press Activated");
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
                LogCoordinateInfo("Drag Started");
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

                    // 🔧 Get current zoom scale from PanPinchContainer
                    var zoomScale = GetContainerScale();

                    // Calculate the drag movement in scaled coordinates
                    var dragDeltaX = this.TranslationX;
                    var dragDeltaY = this.TranslationY;

                    // Convert drag movement to original coordinate space
                    var originalDeltaX = dragDeltaX / zoomScale;
                    var originalDeltaY = dragDeltaY / zoomScale;

                    // Get current position in original coordinate space
                    var currentOriginalX = placedDevice.RelativeX * parent.Width;
                    var currentOriginalY = placedDevice.RelativeY * parent.Height;

                    // Apply the movement
                    var newOriginalX = currentOriginalX + originalDeltaX;
                    var newOriginalY = currentOriginalY + originalDeltaY;

                    // Convert back to relative coordinates
                    placedDevice.RelativeX = Math.Clamp(newOriginalX / parent.Width, 0.0, 1.0);
                    placedDevice.RelativeY = Math.Clamp(newOriginalY / parent.Height, 0.0, 1.0);

                    Debug.WriteLine($"[Drag Completed] Zoom: {zoomScale:F2}, DragDelta: ({dragDeltaX:F1}, {dragDeltaY:F1}), OriginalDelta: ({originalDeltaX:F1}, {originalDeltaY:F1}), NewPos: ({newOriginalX:F1}, {newOriginalY:F1}), NewRelative: ({placedDevice.RelativeX:F2}, {placedDevice.RelativeY:F2})");

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
                this.ScaleTo(1.0, 150);
                break;
            case GestureStatus.Canceled:
                isDragging = false;
                this.ScaleTo(1.0, 150);
                UpdateModelPosition();
                break;
        }
    }

    /// <summary>
    /// Find the PanPinchContainer parent to get the current zoom scale
    /// </summary>
    private double GetContainerScale()
    {
        Element current = this.Parent;
        while (current != null)
        {
            if (current is ReisingerIntelliAppV1.Controls.PanPinchContainer container)
            {
                // Access the Content.Scale property which represents the current zoom level
                return container.Content?.Scale ?? 1.0;
            }
            current = current.Parent;
        }
        return 1.0; // Default scale if no container found
    }

    /// <summary>
    /// Debug helper to log current coordinate information
    /// </summary>
    private void LogCoordinateInfo(string context)
    {
        if (BindingContext is PlacedDeviceModel model && Parent is VisualElement parent)
        {
            var zoomScale = GetContainerScale();
            Debug.WriteLine($"[{context}] Device: {model.Name}, Zoom: {zoomScale:F2}, X: {this.X:F1}, Y: {this.Y:F1}, TransX: {this.TranslationX:F1}, TransY: {this.TranslationY:F1}, RelX: {model.RelativeX:F2}, RelY: {model.RelativeY:F2}, ParentSize: {parent.Width:F1}x{parent.Height:F1}");
        }
    }

    private void UpdateModelPosition()
    {
        if (BindingContext is not PlacedDeviceModel model || Parent is not VisualElement parent)
            return;

        var zoomScale = GetContainerScale();
        
        double centerX = this.X + this.Width / 2;
        double centerY = this.Y + this.Height / 2;

        model.RelativeX = centerX / parent.Width;
        model.RelativeY = centerY / parent.Height;

        Debug.WriteLine($"[UpdateModelPosition] Zoom: {zoomScale:F2}, Center: ({centerX:F1}, {centerY:F1}), Parent: ({parent.Width:F1}, {parent.Height:F1}), NewRelative: ({model.RelativeX:F2}, {model.RelativeY:F2})");
    }
}
