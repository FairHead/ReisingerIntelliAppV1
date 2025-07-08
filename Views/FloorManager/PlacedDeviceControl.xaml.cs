using CommunityToolkit.Maui.Behaviors;
using ReisingerIntelliAppV1.Model.Models;
using System.Diagnostics;
using System.Windows.Input;
using ReisingerIntelliAppV1.Model.ViewModels;
using System.ComponentModel;

namespace ReisingerIntelliAppV1.Views.FloorManager;

public partial class PlacedDeviceControl : ContentView, INotifyPropertyChanged
{
    private bool isDragging = false;
    private bool isPositioningMode = false;
    private double startX, startY;
    private double panStartX, panStartY;

    public PlacedDeviceControl()
    {
        InitializeComponent();
    }

    // Public property to expose drag status
    public bool IsDragMode => isDragging && isPositioningMode;

    // Public property to expose positioning mode status
    public bool IsPositioningMode 
    { 
        get => isPositioningMode; 
        private set 
        { 
            isPositioningMode = value; 
            OnPropertyChanged(); 
        } 
    }

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

    // Toggle positioning mode command
    public ICommand TogglePositioningMode => new Command(() =>
    {
        IsPositioningMode = !IsPositioningMode;
        if (IsPositioningMode)
        {
            isDragging = true;
            Debug.WriteLine("[PlacedDeviceControl] Positioning mode activated");
        }
        else
        {
            isDragging = false;
            Debug.WriteLine("[PlacedDeviceControl] Positioning mode deactivated");
        }
        OnPropertyChanged(nameof(IsDragMode)); // Notify PanPinchContainer about drag mode change
    });

    // Aktiviert Drag (von LongPress) - keep backward compatibility
    public ICommand StartDragCommand => new Command(() =>
    {
        IsPositioningMode = true;
        isDragging = true;
        OnPropertyChanged(nameof(IsDragMode)); // Notify PanPinchContainer about drag mode change
        Debug.WriteLine("[PlacedDeviceControl] Drag aktiviert (LongPress)");
    });

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        if (BindingContext is PlacedDeviceModel device)
        {
            e.Data.Properties["DeviceId"] = device.DeviceId;
        }
    }

    private async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (!isDragging || !isPositioningMode) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                startX = TranslationX;
                startY = TranslationY;
                panStartX = e.TotalX;
                panStartY = e.TotalY;
                Debug.WriteLine("[PlacedDeviceControl] Pan started");
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

                    Debug.WriteLine($"[PlacedDeviceControl] Device moved to relative position: X={placedDevice.RelativeX:F3}, Y={placedDevice.RelativeY:F3}");

                    // Save changes
                    if (Application.Current.MainPage is Shell shell &&
                        shell.CurrentPage is FloorPlanManagerPage page &&
                        page.BindingContext is FloorPlanViewModel vm)
                    {
                        await vm.SaveBuildingsAsync();
                    }
                }

                // Exit positioning mode after successful drag
                IsPositioningMode = false;
                isDragging = false;
                OnPropertyChanged(nameof(IsDragMode)); // Notify PanPinchContainer about drag mode change
                Debug.WriteLine("[PlacedDeviceControl] Pan completed, exiting positioning mode");
                break;
            case GestureStatus.Canceled:
                // Reset positioning mode on cancel
                IsPositioningMode = false;
                isDragging = false;
                OnPropertyChanged(nameof(IsDragMode)); // Notify PanPinchContainer about drag mode change
                UpdateModelPosition();
                Debug.WriteLine("[PlacedDeviceControl] Pan canceled");
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
