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

                    // 🔧 Get current zoom scale from PanPinchContainer
                    var zoomScale = GetContainerScale();

                    // When the content is scaled, the pan gestures work in scaled coordinates
                    // but the final position needs to be in unscaled coordinates for AbsoluteLayout
                    var scaledX = this.X + this.TranslationX;
                    var scaledY = this.Y + this.TranslationY;

                    // Convert back to unscaled coordinates
                    var unscaledX = scaledX / zoomScale;
                    var unscaledY = scaledY / zoomScale;

                    // Calculate relative position based on the unscaled parent size
                    var unscaledParentWidth = parent.Width / zoomScale;
                    var unscaledParentHeight = parent.Height / zoomScale;

                    placedDevice.RelativeX = unscaledX / unscaledParentWidth;
                    placedDevice.RelativeY = unscaledY / unscaledParentHeight;

                    Debug.WriteLine($"[Drag Completed] Zoom: {zoomScale:F2}, ScaledPos: ({scaledX:F1}, {scaledY:F1}), UnscaledPos: ({unscaledX:F1}, {unscaledY:F1}), UnscaledParent: ({unscaledParentWidth:F1}, {unscaledParentHeight:F1}), Relative: ({placedDevice.RelativeX:F2}, {placedDevice.RelativeY:F2})");

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

    private void UpdateModelPosition()
    {
        if (BindingContext is not PlacedDeviceModel model || Parent is not VisualElement parent)
            return;

        var zoomScale = GetContainerScale();
        
        double centerX = this.X + this.Width / 2;
        double centerY = this.Y + this.Height / 2;

        model.RelativeX = centerX / parent.Width;
        model.RelativeY = centerY / parent.Height;

        Debug.WriteLine($"[UpdateModelPosition] Zoom: {zoomScale:F2}, CenterX: {centerX:F1}, CenterY: {centerY:F1}, RelX: {model.RelativeX:F2}, RelY: {model.RelativeY:F2}");
    }
}
